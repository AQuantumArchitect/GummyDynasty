using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>Play-mode benches write here so the agent can read them without a license handshake.</summary>
    public static class BenchSink
    {
        public static string FilePath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "play-bench.jsonl"));

        public static void Write(string bench, string setup, int n, float p50, float p95, float p99, string notes)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var sb = new StringBuilder(256);
                sb.Append('{');
                sb.Append("\"at\":\"").Append(DateTime.Now.ToString("s")).Append("\",");
                sb.Append("\"bench\":\"").Append(Esc(bench)).Append("\",");
                sb.Append("\"setup\":\"").Append(Esc(setup)).Append("\",");
                sb.Append("\"n\":").Append(n).Append(',');
                sb.Append("\"p50\":").Append(p50.ToString("0.000")).Append(',');
                sb.Append("\"p95\":").Append(p95.ToString("0.000")).Append(',');
                sb.Append("\"p99\":").Append(p99.ToString("0.000")).Append(',');
                sb.Append("\"notes\":\"").Append(Esc(notes)).Append('"');
                sb.Append('}').Append('\n');
                File.AppendAllText(FilePath, sb.ToString());
            }
            catch (Exception e)
            {
                Debug.LogWarning("BenchSink failed: " + e.Message);
            }
        }

        static string Esc(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
