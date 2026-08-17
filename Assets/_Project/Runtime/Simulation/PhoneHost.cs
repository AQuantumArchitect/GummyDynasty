using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GummyDynasty.Core;

namespace GummyDynasty.Simulation
{
    /// <summary>
    /// Local LAN HTTP. Phones send intentions; the host sim stays authoritative.
    /// Join is applied here. World commands are queued for the main thread.
    /// </summary>
    public sealed class PhoneHost : IDisposable
    {
        public const int DefaultPort = 8787;

        readonly PhoneSession _session;
        readonly ConcurrentQueue<PhoneCommand> _world = new ConcurrentQueue<PhoneCommand>();
        TcpListener _listen;
        Thread _thread;
        volatile bool _run;
        volatile string _stateJson = "{}";
        bool[,] _qr;

        public int Port { get; private set; }
        public string LocalUrl { get; private set; } = "http://127.0.0.1:8787/";
        public string LanUrl { get; private set; } = "http://127.0.0.1:8787/";
        public bool Listening { get; private set; }
        public string LastError { get; private set; }
        public bool[,] Qr => _qr;

        public PhoneHost(PhoneSession session)
        {
            _session = session ?? new PhoneSession();
        }

        public bool TryStart(int port = DefaultPort)
        {
            Stop();
            Port = port;
            Exception last = null;
            for (var i = 0; i < 4; i++)
            {
                var tryPort = port + i;
                try
                {
                    _listen = new TcpListener(IPAddress.Any, tryPort);
                    _listen.Start();
                    port = tryPort;
                    last = null;
                    break;
                }
                catch (Exception ex)
                {
                    last = ex;
                    _listen = null;
                }
            }

            if (_listen == null)
            {
                LastError = last != null ? last.Message : "bind failed";
                Listening = false;
                return false;
            }

            Port = port;
            LocalUrl = "http://127.0.0.1:" + port + "/";
            var lan = FirstLanIpv4();
            LanUrl = lan != null ? "http://" + lan + ":" + port + "/" : LocalUrl;
            _qr = QrEncode.Encode(LanUrl);
            _run = true;
            Listening = true;
            _thread = new Thread(AcceptLoop) { IsBackground = true, Name = "GummyPhoneHost" };
            _thread.Start();
            return true;
        }

        public void PublishState(string json)
        {
            if (!string.IsNullOrEmpty(json))
                _stateJson = json;
        }

        public bool TryDequeue(out PhoneCommand cmd) => _world.TryDequeue(out cmd);

        public void Stop()
        {
            _run = false;
            Listening = false;
            try { _listen?.Stop(); } catch { /* ignore */ }
            _listen = null;
            _thread = null;
        }

        public void Dispose() => Stop();

        void AcceptLoop()
        {
            while (_run && _listen != null)
            {
                try
                {
                    var client = _listen.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(_ => Serve(client));
                }
                catch
                {
                    if (!_run)
                        return;
                }
            }
        }

        void Serve(TcpClient client)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                stream.ReadTimeout = 4000;
                stream.WriteTimeout = 4000;
                string request;
                try
                {
                    request = ReadRequest(stream);
                }
                catch
                {
                    return;
                }

                if (string.IsNullOrEmpty(request))
                    return;

                var first = FirstLine(request);
                var method = Token(first, 0);
                var path = Token(first, 1);
                var q = path.IndexOf('?');
                if (q >= 0)
                    path = path.Substring(0, q);
                if (string.IsNullOrEmpty(path))
                    path = "/";

                var body = BodyOf(request);
                string status;
                string ctype;
                byte[] payload;
                Handle(method, path, body, out status, out ctype, out payload);
                WriteResponse(stream, status, ctype, payload);
            }
        }

        void Handle(string method, string path, string body, out string status, out string ctype, out byte[] payload)
        {
            status = "200 OK";
            ctype = "text/html; charset=utf-8";
            payload = Empty;

            if (method == "OPTIONS")
            {
                payload = Empty;
                return;
            }

            if (path == "/" || path == "/join")
            {
                if (method == "POST")
                {
                    ctype = "application/json";
                    payload = Enc(HandleJoin(body, out status));
                    return;
                }

                payload = Enc(PhonePages.Join(LanUrl));
                return;
            }

            if (path == "/commander")
            {
                payload = Enc(PhonePages.Commander());
                return;
            }

            if (path == "/artillery")
            {
                payload = Enc(PhonePages.Artillery());
                return;
            }

            if (path == "/state")
            {
                ctype = "application/json";
                payload = Enc(_stateJson);
                return;
            }

            if (path == "/qr.svg")
            {
                ctype = "image/svg+xml";
                payload = Enc(QrSvg(_qr));
                return;
            }

            if (path == "/cmd" && method == "POST")
            {
                ctype = "application/json";
                payload = Enc(HandleCmd(body, out status));
                return;
            }

            status = "404 Not Found";
            payload = Enc("no");
        }

        string HandleJoin(string body, out string status)
        {
            status = "200 OK";
            if (!PhoneCommand.TryParse(string.IsNullOrEmpty(body) ? "join" : body, out var cmd, out var err)
                || cmd.Op != PhoneCommand.OpJoin)
            {
                status = "400 Bad Request";
                return PhonePages.JsonErr(err ?? "bad join");
            }

            if (!_session.TryJoin(cmd.Role, out var id, out err))
            {
                status = "409 Conflict";
                return PhonePages.JsonErr(err);
            }

            return PhonePages.JsonOk("\"player\":\"" + PhonePages.Esc(id) + "\",\"role\":\"" + PhonePages.Esc(cmd.Role) + "\"");
        }

        string HandleCmd(string body, out string status)
        {
            status = "200 OK";
            if (!PhoneCommand.TryParse(body, out var cmd, out var err))
            {
                status = "400 Bad Request";
                return PhonePages.JsonErr(err);
            }

            if (cmd.Op != PhoneCommand.OpCmd)
            {
                status = "400 Bad Request";
                return PhonePages.JsonErr("not a command");
            }

            if (!_session.TryValidate(cmd, out err))
            {
                status = "403 Forbidden";
                return PhonePages.JsonErr(err);
            }

            _world.Enqueue(cmd);
            return PhonePages.JsonOk("\"action\":\"" + PhonePages.Esc(cmd.Action) + "\"");
        }

        static string QrSvg(bool[,] modules)
        {
            if (modules == null)
                return "<svg xmlns='http://www.w3.org/2000/svg'/>";
            var n = modules.GetLength(0);
            var sb = new StringBuilder(n * n * 8);
            sb.Append("<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 ").Append(n).Append(' ').Append(n);
            sb.Append("' shape-rendering='crispEdges'><rect width='100%' height='100%' fill='#fff'/>");
            for (var y = 0; y < n; y++)
            for (var x = 0; x < n; x++)
            {
                if (!modules[x, y])
                    continue;
                sb.Append("<rect x='").Append(x).Append("' y='").Append(y).Append("' width='1' height='1'/>");
            }

            sb.Append("</svg>");
            return sb.ToString();
        }

        static string FirstLanIpv4()
        {
            try
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus != OperationalStatus.Up)
                        continue;
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;
                    var props = nic.GetIPProperties();
                    foreach (var addr in props.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily != AddressFamily.InterNetwork)
                            continue;
                        var ip = addr.Address.ToString();
                        if (ip.StartsWith("127."))
                            continue;
                        return ip;
                    }
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        static string ReadRequest(NetworkStream stream)
        {
            var buf = new byte[4096];
            var total = 0;
            var headerEnd = -1;
            var needed = -1;
            using (var ms = new MemoryStream(1024))
            {
                while (total < 65536)
                {
                    var n = stream.Read(buf, 0, buf.Length);
                    if (n <= 0)
                        break;
                    ms.Write(buf, 0, n);
                    total += n;
                    var text = Encoding.UTF8.GetString(ms.ToArray());
                    if (headerEnd < 0)
                    {
                        headerEnd = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                        if (headerEnd < 0)
                            continue;
                        var headers = text.Substring(0, headerEnd);
                        var cl = Header(headers, "Content-Length");
                        needed = 0;
                        if (!string.IsNullOrEmpty(cl))
                            int.TryParse(cl, NumberStyles.Integer, CultureInfo.InvariantCulture, out needed);
                    }

                    if (headerEnd >= 0)
                    {
                        var bodyLen = total - (headerEnd + 4);
                        if (bodyLen >= needed)
                            return text;
                    }
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        static string BodyOf(string request)
        {
            var i = request.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            return i < 0 ? "" : request.Substring(i + 4);
        }

        static string FirstLine(string request)
        {
            var i = request.IndexOf('\n');
            return i < 0 ? request : request.Substring(0, i).Trim();
        }

        static string Token(string line, int index)
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return index < parts.Length ? parts[index] : "";
        }

        static string Header(string headers, string name)
        {
            var needle = name + ":";
            var i = headers.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            if (i < 0)
                return null;
            i += needle.Length;
            var end = headers.IndexOf('\n', i);
            if (end < 0)
                end = headers.Length;
            return headers.Substring(i, end - i).Trim();
        }

        static void WriteResponse(NetworkStream stream, string status, string ctype, byte[] payload)
        {
            if (payload == null)
                payload = Empty;
            var head = "HTTP/1.1 " + status + "\r\n"
                + "Content-Type: " + ctype + "\r\n"
                + "Content-Length: " + payload.Length + "\r\n"
                + "Access-Control-Allow-Origin: *\r\n"
                + "Access-Control-Allow-Headers: Content-Type\r\n"
                + "Connection: close\r\n\r\n";
            var hb = Encoding.UTF8.GetBytes(head);
            stream.Write(hb, 0, hb.Length);
            if (payload.Length > 0)
                stream.Write(payload, 0, payload.Length);
        }

        static readonly byte[] Empty = new byte[0];

        static byte[] Enc(string s) => Encoding.UTF8.GetBytes(s ?? "");
    }
}
