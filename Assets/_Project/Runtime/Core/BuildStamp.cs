namespace GummyDynasty.Core
{
    /// <summary>
    /// Compile-visible day stamp. Exists so Unity's asset pipeline
    /// (and Hub last-modified) notice work that landed outside the editor.
    /// </summary>
    public static class BuildStamp
    {
        public const string Day = "2026-08-17";
        public const string ClosedWave = "S";
        public const string NextWave = "R";
        public const string PlayFix = "behavior-lab";

        public static string HudMark => Day + "  next " + NextWave;
    }
}
