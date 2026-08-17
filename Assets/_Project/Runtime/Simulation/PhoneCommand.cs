using System;
using System.Globalization;
using System.Text;

namespace GummyDynasty.Simulation
{
    /// <summary>
    /// Phone intention. Unity-free so the harness can reject garbage
    /// without HttpListener or PhysX.
    /// </summary>
    public sealed class PhoneCommand
    {
        public const string OpJoin = "join";
        public const string OpCmd = "cmd";
        public const string RoleCommander = "commander";
        public const string RoleArtillery = "artillery";
        public const string ActWest = "west";
        public const string ActHold = "hold";
        public const string ActAim = "aim";
        public const string ActFire = "fire";
        public const string ActLoad = "load";
        public const string MachineCatapult = "catapult";
        public const string MachineCannon = "cannon";

        public string Op;
        public string Player;
        public string Role;
        public string Action;
        public string Machine;
        public float X;
        public float Z;
        public bool HasPoint;

        public static bool TryParse(string raw, out PhoneCommand cmd, out string error)
        {
            cmd = null;
            error = null;
            if (string.IsNullOrEmpty(raw))
            {
                error = "empty";
                return false;
            }

            raw = raw.Trim();
            var parsed = new PhoneCommand();
            if (raw.Length > 0 && raw[0] == '{')
            {
                if (!TryReadJson(raw, parsed, out error))
                    return false;
            }
            else if (!TryReadLine(raw, parsed, out error))
                return false;

            if (!Normalize(parsed, out error))
                return false;
            cmd = parsed;
            return true;
        }

        static bool Normalize(PhoneCommand c, out string error)
        {
            error = null;
            c.Op = Norm(c.Op);
            c.Role = Norm(c.Role);
            c.Action = Norm(c.Action);
            c.Machine = Norm(c.Machine);
            c.Player = string.IsNullOrEmpty(c.Player) ? null : c.Player.Trim();

            if (c.Op != OpJoin && c.Op != OpCmd)
            {
                error = "unknown op";
                return false;
            }

            if (c.Role != RoleCommander && c.Role != RoleArtillery)
            {
                error = "role must be commander or artillery";
                return false;
            }

            if (c.Op == OpJoin)
                return true;

            if (c.Role == RoleCommander)
            {
                if (c.Action != ActWest && c.Action != ActHold)
                {
                    error = "commander action must be west or hold";
                    return false;
                }
                return true;
            }

            if (c.Action != ActAim && c.Action != ActFire && c.Action != ActLoad)
            {
                error = "artillery action must be aim, load, or fire";
                return false;
            }

            if (string.IsNullOrEmpty(c.Machine))
                c.Machine = MachineCatapult;
            if (c.Machine != MachineCatapult && c.Machine != MachineCannon)
            {
                error = "machine must be catapult or cannon";
                return false;
            }

            if (c.Action == ActAim && !c.HasPoint)
            {
                error = "aim needs x and z";
                return false;
            }

            return true;
        }

        static bool TryReadLine(string raw, PhoneCommand c, out string error)
        {
            error = null;
            var parts = raw.Split(new[] { ' ', '\t', '&' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                error = "empty";
                return false;
            }

            c.Op = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var p = parts[i];
                var eq = p.IndexOf('=');
                if (eq <= 0)
                    continue;
                var key = p.Substring(0, eq);
                var val = p.Substring(eq + 1);
                Assign(c, key, val);
            }

            return true;
        }

        static bool TryReadJson(string raw, PhoneCommand c, out string error)
        {
            error = null;
            if (!TryField(raw, "op", out c.Op) && !TryField(raw, "kind", out c.Op))
            {
                error = "missing op";
                return false;
            }

            TryField(raw, "player", out c.Player);
            TryField(raw, "role", out c.Role);
            TryField(raw, "action", out c.Action);
            TryField(raw, "machine", out c.Machine);
            if (TryNumber(raw, "x", out c.X))
                c.HasPoint = true;
            if (TryNumber(raw, "z", out c.Z))
                c.HasPoint = true;
            return true;
        }

        static void Assign(PhoneCommand c, string key, string val)
        {
            key = Norm(key);
            if (key == "role") c.Role = val;
            else if (key == "player") c.Player = val;
            else if (key == "action" || key == "act") c.Action = val;
            else if (key == "machine") c.Machine = val;
            else if (key == "x")
            {
                if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out c.X))
                    c.HasPoint = true;
            }
            else if (key == "z")
            {
                if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out c.Z))
                    c.HasPoint = true;
            }
        }

        static bool TryField(string json, string key, out string value)
        {
            value = null;
            var needle = "\"" + key + "\"";
            var i = IndexOfKey(json, needle);
            if (i < 0)
                return false;
            i = json.IndexOf(':', i + needle.Length);
            if (i < 0)
                return false;
            i++;
            while (i < json.Length && char.IsWhiteSpace(json[i]))
                i++;
            if (i >= json.Length)
                return false;
            if (json[i] == '"')
            {
                i++;
                var end = json.IndexOf('"', i);
                if (end < 0)
                    return false;
                value = json.Substring(i, end - i);
                return true;
            }

            var j = i;
            while (j < json.Length && json[j] != ',' && json[j] != '}' && !char.IsWhiteSpace(json[j]))
                j++;
            value = json.Substring(i, j - i);
            return value.Length > 0;
        }

        static bool TryNumber(string json, string key, out float value)
        {
            value = 0f;
            if (!TryField(json, key, out var raw) || string.IsNullOrEmpty(raw))
                return false;
            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        static int IndexOfKey(string json, string needle)
        {
            var i = 0;
            while (i < json.Length)
            {
                var at = json.IndexOf(needle, i, StringComparison.Ordinal);
                if (at < 0)
                    return -1;
                if (at == 0 || json[at - 1] != '\\')
                    return at;
                i = at + needle.Length;
            }

            return -1;
        }

        static string Norm(string s)
        {
            return string.IsNullOrEmpty(s) ? null : s.Trim().ToLowerInvariant();
        }

        public string ToLine()
        {
            var sb = new StringBuilder();
            sb.Append(Op ?? "");
            if (!string.IsNullOrEmpty(Role))
                sb.Append(" role=").Append(Role);
            if (!string.IsNullOrEmpty(Player))
                sb.Append(" player=").Append(Player);
            if (!string.IsNullOrEmpty(Action))
                sb.Append(" action=").Append(Action);
            if (!string.IsNullOrEmpty(Machine))
                sb.Append(" machine=").Append(Machine);
            if (HasPoint)
            {
                sb.Append(" x=").Append(X.ToString("0.###", CultureInfo.InvariantCulture));
                sb.Append(" z=").Append(Z.ToString("0.###", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
    }
}
