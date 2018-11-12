using System.Linq;
using Bleatingsheep.OsuMixedApi;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Osu
{
    internal static class BloodcatUtilities
    {
        public static Message GetMusicMessage(OsuApiClient osuApi, BloodcatBeatmapSet set)
        {
            int setId = set.Id;
            string info = string.Empty;
            info += set.Beatmaps.Max(b => b.TotalLength) + "s, ";
            if (set.Beatmaps.Length > 1)
                info += $"{set.Beatmaps.Min(b => b.Stars):0.##}* - {set.Beatmaps.Max(b => b.Stars):0.##}*";
            else
                info += $"{set.Beatmaps.Single()?.Stars:0.##}*";

            // Creator and BPM
            info += "\r\n" + $"by {set.Creator}";
            var bpms = set.Beatmaps.Select(b => b.Bpm).Distinct().ToList();
            if (bpms.Count == 1)
            {
                info += $" | ♩{bpms.First()}";
            }

            if (!string.IsNullOrEmpty(set.Source))
                info += "\r\n" + $"From {set.Source}";
            Message message = SendingMessage.MusicCustom(osuApi.PageOfSetOld(setId), osuApi.PreviewAudioOf(setId), $"{set.Title}/{set.Artist}", info, osuApi.ThumbOf(setId));
            return message;
        }
    }
}
