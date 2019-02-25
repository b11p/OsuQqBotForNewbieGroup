using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bleatingsheep.OsuMixedApi
{
    public class BloodcatApi
    {
        public static BloodcatApi Client { get; } = new BloodcatApi();

        private BloodcatApi() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="modes"></param>
        /// <returns></returns>
        public async Task<BloodcatBeatmapSet[]> SearchRankedByKeywordAsync(string keyword = "", params Mode[] modes)
        {
            if (modes == null)
            {
                throw new ArgumentNullException(nameof(modes));
            }

            if (modes.Length == 0)
                modes = new Mode[] { Mode.Standard };
            (string, string)[] para = BloodcatParam("o", keyword, modes);
            var response = await HttpMethods
                .GetJsonArrayDeserializeAsync<BloodcatBeatmapSet>("https://bloodcat.com/osu/", para);
            return response;
        }

        public async Task<BloodcatBeatmap[]> SearchRankedByMd5Async(string md5)
        {
            var response = await HttpMethods.GetJsonArrayDeserializeAsync<BloodcatBeatmapSet>("https://bloodcat.com/osu/", ("q", $"md5={md5}"), ("mod", "json"));
            return response?.SelectMany(s => s.Beatmaps).Where(b => string.Equals(b.FileMD5, md5, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        public async Task<BloodcatBeatmapSet> GetBeatmapNoFilterAsync(int bid)
        {
            (string, string)[] para = BloodcatParam("b", bid.ToString(), (Mode[])Enum.GetValues(typeof(Mode)));
            var response = await HttpMethods.GetJsonArrayDeserializeAsync<BloodcatBeatmapSet>("https://bloodcat.com/osu/", para);
            var result = response?.SingleOrDefault();
            return result;
        }

        public async Task<BloodcatBeatmapSet> GetBeatmapAsync(int bid)
        {
            BloodcatBeatmapSet result = await GetBeatmapNoFilterAsync(bid);
            if (result != null)
                result.Beatmaps = result.Beatmaps.Where(b => b.Id == bid).ToArray();
            return result;
        }

        private static (string, string)[] BloodcatParam(string type, string keyword, Mode[] modes)
        {
            return new[]
            {
                ("mod", "json"),
                ("q", keyword),
                ("c", type),
                ("s", string.Join(",", new[] { Approved.Ranked, Approved.Approved }.Select(a => (int)a))),
                ("m", string.Join(",", modes?.Select(m => (int)m))),
            };
        }
    }

    public class BloodcatBeatmapSet
    {
        private static readonly TimeSpan Kst = new TimeSpan(9, 0, 0);
#pragma warning disable CS0649
        [JsonProperty("synced")]
        private DateTime synced;
        [JsonIgnore]
        public DateTimeOffset SyncDateOffset => new DateTimeOffset(synced, Kst);
        /// <summary>
        /// Rank 状态，只会出现 <see
        /// cref="Approved.Pending"/>、<see
        /// cref="Approved.Ranked"/>、<see
        /// cref="Approved.Approved"/>、<see
        /// cref="Approved.Qualified"/>
        /// 以及 <see cref="Approved.Loved"/>。
        /// </summary>
        [JsonProperty("status")]
        public Approved Approved { get; private set; }
        [JsonProperty("title")]
        public string RomanisedTitle { get; private set; }
        [JsonProperty("titleU")]
        private string titleU;
        [JsonIgnore]
        public string Title => titleU ?? RomanisedTitle;
        [JsonProperty("artist")]
        public string RomanisedArtist { get; private set; }
        [JsonProperty("artistU")]
        private string artistU;
        [JsonIgnore]
        public string Artist => artistU ?? RomanisedArtist;
        [JsonProperty("creatorId")]
        public int CreatorId { get; private set; }
        [JsonProperty("creator")]
        public string Creator { get; private set; }
        [JsonProperty("rankedAt")]
        private DateTime? rankedAt;
        [JsonIgnore]
        public DateTimeOffset? ApprovedDateOffset => rankedAt.HasValue ? new DateTimeOffset(rankedAt.Value, Kst) : (DateTimeOffset?)null;
        [JsonProperty("tags")]
        public string Tags { get; private set; }
        [JsonProperty("source")]
        public string Source { get; private set; }
        [JsonProperty("genreId")]
        public Genre Genre { get; private set; }
        [JsonProperty("languageId")]
        public Language Language { get; private set; }
        [JsonProperty("id")]
        public int Id { get; private set; }
        [JsonProperty("beatmaps")]
        public BloodcatBeatmap[] Beatmaps { get; internal set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Source))
                return $"{Source} ({Artist}) - {Title}";
            return $"{Artist} - {Title}";
        }
#pragma warning restore CS0649
    }

    public class BloodcatBeatmap
    {
        [JsonProperty("id")]
        public int Id { get; private set; }
        [JsonProperty("name")]
        public string DifficultyName { get; private set; }
        [JsonProperty("mode")]
        public Mode Mode { get; private set; }
        [JsonProperty("hp")]
        public double HP { get; private set; }
        [JsonProperty("cs")]
        public double CS { get; private set; }
        [JsonProperty("od")]
        public double OD { get; private set; }
        [JsonProperty("ar")]
        public double AR { get; private set; }
        [JsonProperty("bpm")]
        public double Bpm { get; private set; }
        [JsonProperty("length")]
        public int TotalLength { get; private set; }
        [JsonProperty("star")]
        public double Stars { get; private set; }
        [JsonProperty("hash_md5")]
        public string FileMD5 { get; private set; }
        [JsonProperty("status")]
        public Approved Approved { get; private set; }
        [JsonProperty("author")]
        public string Creator { get; private set; }
    }
}
