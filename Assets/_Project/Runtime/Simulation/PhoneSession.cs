using System;

namespace GummyDynasty.Simulation
{
    /// <summary>
    /// Authoritative join + role table. Host sim applies validated commands.
    /// Thread-safe so the HTTP worker can join without touching Unity.
    /// </summary>
    public sealed class PhoneSession
    {
        readonly object _gate = new object();
        int _next = 1;

        public string CommanderId { get; private set; }
        public string ArtilleryId { get; private set; }
        public int PlayerCount
        {
            get
            {
                var n = 0;
                if (!string.IsNullOrEmpty(CommanderId)) n++;
                if (!string.IsNullOrEmpty(ArtilleryId)) n++;
                return n;
            }
        }

        public void Reset()
        {
            lock (_gate)
            {
                CommanderId = null;
                ArtilleryId = null;
                _next = 1;
            }
        }

        public bool TryJoin(string role, out string playerId, out string error)
        {
            playerId = null;
            error = null;
            role = string.IsNullOrEmpty(role) ? null : role.Trim().ToLowerInvariant();
            if (role != PhoneCommand.RoleCommander && role != PhoneCommand.RoleArtillery)
            {
                error = "role must be commander or artillery";
                return false;
            }

            lock (_gate)
            {
                if (role == PhoneCommand.RoleCommander)
                {
                    if (!string.IsNullOrEmpty(CommanderId))
                    {
                        error = "commander taken";
                        return false;
                    }

                    CommanderId = "p" + _next++;
                    playerId = CommanderId;
                    return true;
                }

                if (!string.IsNullOrEmpty(ArtilleryId))
                {
                    error = "artillery taken";
                    return false;
                }

                ArtilleryId = "p" + _next++;
                playerId = ArtilleryId;
                return true;
            }
        }

        public bool TryValidate(PhoneCommand cmd, out string error)
        {
            error = null;
            if (cmd == null)
            {
                error = "empty";
                return false;
            }

            if (cmd.Op == PhoneCommand.OpJoin)
                return true;

            if (string.IsNullOrEmpty(cmd.Player))
            {
                error = "missing player";
                return false;
            }

            lock (_gate)
            {
                if (cmd.Role == PhoneCommand.RoleCommander)
                {
                    if (cmd.Player != CommanderId)
                    {
                        error = "not the commander";
                        return false;
                    }
                }
                else if (cmd.Player != ArtilleryId)
                {
                    error = "not the artillery";
                    return false;
                }
            }

            return true;
        }
    }
}
