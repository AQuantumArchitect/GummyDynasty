using System;
using System.Collections.Generic;
using System.Text;

namespace GummyDynasty.Core
{
    /// <summary>
    /// Compact QR (version 2–3, EC L, byte mode). Enough for a LAN join URL.
    /// Unity-free so the harness can prove finder patterns exist.
    /// </summary>
    public static class QrEncode
    {
        public const int Version2Size = 25;
        public const int Version3Size = 29;

        public static bool[,] Encode(string text)
        {
            if (text == null)
                text = "";
            var bytes = Encoding.UTF8.GetBytes(text);
            var version = bytes.Length <= 32 ? 2 : 3;
            if (bytes.Length > 53)
            {
                var cut = new byte[53];
                Array.Copy(bytes, cut, 53);
                bytes = cut;
                version = 3;
            }

            return EncodeVersion(bytes, version);
        }

        public static int SizeOf(bool[,] modules) => modules != null ? modules.GetLength(0) : 0;

        static bool[,] EncodeVersion(byte[] data, int version)
        {
            var size = 17 + 4 * version;
            var dataCw = version == 2 ? 44 : 70;
            var ecCw = version == 2 ? 10 : 15;
            var bits = new BitBuf();
            bits.Put(4, 4);
            bits.Put(data.Length, 8);
            for (var i = 0; i < data.Length; i++)
                bits.Put(data[i], 8);
            var capacity = dataCw * 8;
            var remain = capacity - bits.Length;
            if (remain > 4) remain = 4;
            if (remain > 0)
                bits.Put(0, remain);
            while (bits.Length % 8 != 0)
                bits.Put(0, 1);
            var pad = true;
            while (bits.Length < capacity)
            {
                bits.Put(pad ? 0xEC : 0x11, 8);
                pad = !pad;
            }

            var dataBytes = bits.ToBytes(dataCw);
            var ec = ReedSolomon(dataBytes, ecCw);
            var all = new byte[dataCw + ecCw];
            Array.Copy(dataBytes, 0, all, 0, dataCw);
            Array.Copy(ec, 0, all, dataCw, ecCw);

            var best = (bool[,])null;
            var bestScore = int.MaxValue;
            for (var mask = 0; mask < 8; mask++)
            {
                var grid = new bool[size, size];
                var reserved = new bool[size, size];
                PlaceFunctions(grid, reserved, version);
                PlaceData(grid, reserved, all, version);
                ApplyMask(grid, reserved, mask);
                PlaceFormat(grid, reserved, mask);
                var score = Penalty(grid);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = grid;
                }
            }

            return best;
        }

        static void PlaceFunctions(bool[,] g, bool[,] reserved, int version)
        {
            var n = g.GetLength(0);
            Finder(g, reserved, 0, 0);
            Finder(g, reserved, n - 7, 0);
            Finder(g, reserved, 0, n - 7);
            for (var i = 0; i < 8; i++)
            {
                Reserve(reserved, i, 7);
                Reserve(reserved, 7, i);
                Reserve(reserved, n - 8 + i, 7);
                Reserve(reserved, n - 8, i);
                Reserve(reserved, i, n - 8);
                Reserve(reserved, 7, n - 8 + i);
            }

            for (var i = 8; i < n - 8; i++)
            {
                SetR(g, reserved, i, 6, i % 2 == 0);
                SetR(g, reserved, 6, i, i % 2 == 0);
            }

            if (version >= 2)
            {
                var a = version == 2 ? 18 : 22;
                Alignment(g, reserved, a, a);
            }

            SetR(g, reserved, 8, 4 * version + 9, true);
            for (var i = 0; i < 9; i++)
            {
                Reserve(reserved, i, 8);
                Reserve(reserved, 8, i);
            }

            for (var i = 0; i < 8; i++)
            {
                Reserve(reserved, n - 1 - i, 8);
                Reserve(reserved, 8, n - 1 - i);
            }
        }

        static void Finder(bool[,] g, bool[,] reserved, int x, int y)
        {
            for (var dy = -1; dy <= 7; dy++)
            for (var dx = -1; dx <= 7; dx++)
            {
                var xx = x + dx;
                var yy = y + dy;
                var dark = dx >= 0 && dx <= 6 && dy >= 0 && dy <= 6
                    && (dx == 0 || dx == 6 || dy == 0 || dy == 6 || (dx >= 2 && dx <= 4 && dy >= 2 && dy <= 4));
                if (In(g, xx, yy))
                    SetR(g, reserved, xx, yy, dark);
            }
        }

        static void Alignment(bool[,] g, bool[,] reserved, int cx, int cy)
        {
            for (var dy = -2; dy <= 2; dy++)
            for (var dx = -2; dx <= 2; dx++)
            {
                var dark = Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1;
                SetR(g, reserved, cx + dx, cy + dy, dark);
            }
        }

        static void PlaceData(bool[,] g, bool[,] reserved, byte[] data, int version)
        {
            var n = g.GetLength(0);
            var bit = 0;
            var total = data.Length * 8;
            var up = true;
            for (var col = n - 1; col > 0; col -= 2)
            {
                if (col == 6)
                    col--;
                for (var i = 0; i < n; i++)
                {
                    var y = up ? n - 1 - i : i;
                    for (var k = 0; k < 2; k++)
                    {
                        var x = col - k;
                        if (reserved[x, y])
                            continue;
                        var dark = false;
                        if (bit < total)
                        {
                            var b = data[bit >> 3];
                            dark = ((b >> (7 - (bit & 7))) & 1) != 0;
                            bit++;
                        }

                        g[x, y] = dark;
                    }
                }

                up = !up;
            }
        }

        static void ApplyMask(bool[,] g, bool[,] reserved, int mask)
        {
            var n = g.GetLength(0);
            for (var y = 0; y < n; y++)
            for (var x = 0; x < n; x++)
            {
                if (reserved[x, y])
                    continue;
                if (Mask(mask, x, y))
                    g[x, y] = !g[x, y];
            }
        }

        static bool Mask(int mask, int x, int y)
        {
            switch (mask)
            {
                case 0: return (x + y) % 2 == 0;
                case 1: return y % 2 == 0;
                case 2: return x % 3 == 0;
                case 3: return (x + y) % 3 == 0;
                case 4: return (y / 2 + x / 3) % 2 == 0;
                case 5: return (x * y) % 2 + (x * y) % 3 == 0;
                case 6: return ((x * y) % 2 + (x * y) % 3) % 2 == 0;
                default: return ((x + y) % 2 + (x * y) % 3) % 2 == 0;
            }
        }

        static void PlaceFormat(bool[,] g, bool[,] reserved, int mask)
        {
            var bits = FormatBits(mask);
            var n = g.GetLength(0);
            var coords = new[]
            {
                new[] {0, 8}, new[] {1, 8}, new[] {2, 8}, new[] {3, 8}, new[] {4, 8}, new[] {5, 8}, new[] {7, 8},
                new[] {8, 8}, new[] {8, 7}, new[] {8, 5}, new[] {8, 4}, new[] {8, 3}, new[] {8, 2}, new[] {8, 1}, new[] {8, 0}
            };
            var coords2 = new[]
            {
                new[] {8, n - 1}, new[] {8, n - 2}, new[] {8, n - 3}, new[] {8, n - 4}, new[] {8, n - 5}, new[] {8, n - 6},
                new[] {8, n - 7},
                new[] {n - 8, 8}, new[] {n - 7, 8}, new[] {n - 6, 8}, new[] {n - 5, 8}, new[] {n - 4, 8}, new[] {n - 3, 8},
                new[] {n - 2, 8}, new[] {n - 1, 8}
            };
            for (var i = 0; i < 15; i++)
            {
                var dark = ((bits >> (14 - i)) & 1) != 0;
                g[coords[i][0], coords[i][1]] = dark;
                g[coords2[i][0], coords2[i][1]] = dark;
            }
        }

        static int FormatBits(int mask)
        {
            var data = (1 << 3) | mask;
            var bits = data << 10;
            var gen = 0x537;
            for (var i = 14; i >= 10; i--)
            {
                if (((bits >> i) & 1) != 0)
                    bits ^= gen << (i - 10);
            }

            return (data << 10 | bits) ^ 0x5412;
        }

        static int Penalty(bool[,] g)
        {
            var n = g.GetLength(0);
            var score = 0;
            for (var y = 0; y < n; y++)
            {
                var run = 1;
                for (var x = 1; x < n; x++)
                {
                    if (g[x, y] == g[x - 1, y]) run++;
                    else
                    {
                        if (run >= 5) score += run - 2;
                        run = 1;
                    }
                }

                if (run >= 5) score += run - 2;
            }

            for (var x = 0; x < n; x++)
            {
                var run = 1;
                for (var y = 1; y < n; y++)
                {
                    if (g[x, y] == g[x, y - 1]) run++;
                    else
                    {
                        if (run >= 5) score += run - 2;
                        run = 1;
                    }
                }

                if (run >= 5) score += run - 2;
            }

            for (var y = 0; y < n - 1; y++)
            for (var x = 0; x < n - 1; x++)
            {
                var v = g[x, y];
                if (g[x + 1, y] == v && g[x, y + 1] == v && g[x + 1, y + 1] == v)
                    score += 3;
            }

            var dark = 0;
            for (var y = 0; y < n; y++)
            for (var x = 0; x < n; x++)
                if (g[x, y]) dark++;
            var percent = dark * 100 / (n * n);
            score += Math.Abs(percent - 50) / 5 * 10;
            return score;
        }

        static byte[] ReedSolomon(byte[] data, int ec)
        {
            var gen = Generator(ec);
            var res = new byte[ec];
            for (var i = 0; i < data.Length; i++)
            {
                var factor = (byte)(data[i] ^ res[0]);
                Array.Copy(res, 1, res, 0, ec - 1);
                res[ec - 1] = 0;
                if (factor == 0)
                    continue;
                for (var j = 0; j < ec; j++)
                    res[j] ^= Mul(gen[j], factor);
            }

            return res;
        }

        static byte[] Generator(int degree)
        {
            var poly = new List<byte> { 1 };
            for (var i = 0; i < degree; i++)
            {
                var next = new List<byte>(poly.Count + 1);
                for (var k = 0; k < poly.Count + 1; k++)
                    next.Add(0);
                for (var j = 0; j < poly.Count; j++)
                {
                    next[j] ^= poly[j];
                    next[j + 1] ^= Mul(poly[j], Exp(i));
                }

                poly = next;
            }

            var g = new byte[degree];
            for (var i = 0; i < degree; i++)
                g[i] = poly[i + 1];
            return g;
        }

        static readonly byte[] ExpTable = BuildExp();
        static readonly byte[] LogTable = BuildLog();

        static byte[] BuildExp()
        {
            var exp = new byte[512];
            var x = 1;
            for (var i = 0; i < 255; i++)
            {
                exp[i] = (byte)x;
                x <<= 1;
                if (x >= 256)
                    x ^= 0x11d;
            }

            for (var i = 255; i < 512; i++)
                exp[i] = exp[i - 255];
            return exp;
        }

        static byte[] BuildLog()
        {
            var log = new byte[256];
            for (var i = 0; i < 255; i++)
                log[ExpTable[i]] = (byte)i;
            return log;
        }

        static byte Exp(int i) => ExpTable[i];

        static byte Mul(byte a, byte b)
        {
            if (a == 0 || b == 0)
                return 0;
            return ExpTable[LogTable[a] + LogTable[b]];
        }

        static void SetR(bool[,] g, bool[,] reserved, int x, int y, bool dark)
        {
            if (!In(g, x, y))
                return;
            g[x, y] = dark;
            reserved[x, y] = true;
        }

        static void Reserve(bool[,] reserved, int x, int y)
        {
            if (x >= 0 && y >= 0 && x < reserved.GetLength(0) && y < reserved.GetLength(1))
                reserved[x, y] = true;
        }

        static bool In(bool[,] g, int x, int y)
        {
            return x >= 0 && y >= 0 && x < g.GetLength(0) && y < g.GetLength(1);
        }

        sealed class BitBuf
        {
            readonly List<byte> _bytes = new List<byte>(80);
            int _bit;

            public int Length { get; private set; }

            public void Put(int value, int bits)
            {
                for (var i = bits - 1; i >= 0; i--)
                {
                    if (_bit == 0)
                        _bytes.Add(0);
                    if (((value >> i) & 1) != 0)
                        _bytes[_bytes.Count - 1] |= (byte)(1 << (7 - _bit));
                    _bit = (_bit + 1) & 7;
                    Length++;
                }
            }

            public byte[] ToBytes(int count)
            {
                var a = new byte[count];
                for (var i = 0; i < count && i < _bytes.Count; i++)
                    a[i] = _bytes[i];
                return a;
            }
        }
    }
}
