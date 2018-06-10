using Bleatingsheep.OsuMixedApi;

namespace OsuQqBot.Query
{
    internal static class ModeExtends
    {
        public static string GetModeString(this Mode mode)
        {
            switch (mode)
            {
                case Mode.Standard:
                    return "osu!";
                case Mode.Taiko:
                    return "taiko";
                case Mode.Ctb:
                    return "catch";
                case Mode.Mania:
                    return "mania";
                default:
                    return null;
            }
        }
    }
}
