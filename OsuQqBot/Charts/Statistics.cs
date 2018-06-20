using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database;
using Bleatingsheep.OsuQqBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuQqBot.Charts
{
    static class Statistics
    {
        public static async Task<(string title, IEnumerable<(int uid, double score)> rank)> RankAsync(int chartId)
        {
            var chart = await NewbieDatabase.GetChartWithCommitsAsync(chartId);
            if (chart == null) return (null, null);

            var results = from p in PlayerFirstPlaces(chart)
                          let sum = p.Sum(c => (double)c.Score)
                          orderby sum descending
                          select (p.Key, sum);

            return (chart.ChartName, results);
        }

        #region csv
        public static async Task<string> CsvResultAsync(int chartId)
        {
            var chart = await NewbieDatabase.GetChartWithCommitsAsync(chartId);
            if (chart == null) return null;

            var titles = new Dictionary<ChartBeatmap, int>();
            for (int i = 0; i < chart.Maps.Count; i++)
            {
                titles.Add(chart.Maps[i], i);
            }
            var firstPlaces = PlayerFirstPlaces(chart).ToArray();
            var results = new dynamic[firstPlaces.Length, chart.Maps.Count];
            var players = new int[firstPlaces.Length];
            for (int i = 0; i < firstPlaces.Length; i++)
            {
                IGrouping<int, dynamic> efforts = firstPlaces[i];
                players[i] = efforts.Key;
                foreach (var effort in efforts)
                {
                    results[i, titles[effort.Commit.Beatmap]] = effort;
                }
            }
            var completedCsv = Complete(titles, results, players);
            return completedCsv;
        }

        private static string Complete(IDictionary<ChartBeatmap, int> titles, dynamic[,] results, int[] players)
        {
            int playerCount = players.Length;
            int mapCount = titles.Count;
            var sb = new StringBuilder();
            var sortedTitles = titles.OrderBy(t => t.Value).Select(t => t.Key).ToArray();
            sb.AppendJoin("\r\n", sortedTitles as IEnumerable<ChartBeatmap>);
            sb.AppendLine();
            for (int i = 0; i < mapCount; i++)
            {
                sb.AppendLine(sortedTitles[i].BeatmapId.ToString());
            }
            sb.AppendLine(Title(mapCount));
            for (int i = 0; i < playerCount; i++)
            {
                sb.Append(players[i].ToString());
                for (int j = 0; j < mapCount; j++)
                {
                    sb.Append(AMap(results[i, j]));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string Title(int count)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(",date,pp,mods,combo,acc,score,rank,final");
            }
            return sb.ToString();
        }

        private static string AMap(dynamic single)
        {
            ChartTry effort = single?.Commit;
            double final = single?.Score ?? 0;

            if (effort == null) return ",,,,,,,,0";

            return $",{DateTimeOffset.FromUnixTimeSeconds(effort.Date).ToOffset(new TimeSpan(8, 0, 0))}" +
                $",{effort.PPWhenCommit},{effort.Mods.Display()},{effort.Combo},{effort.Accuracy:.##%}" +
                $",{effort.Score},{effort.Rank},{final}";
        }
        #endregion

        private static IEnumerable<IGrouping<int, dynamic>> PlayerFirstPlaces(Chart chart)
        {
            var maps = chart.Maps.Select(b => new { beatmap = b, score = ParseScorer(b.ScoreCalculation) });

            var commitsGroup =
                from m in maps
                from c in m.beatmap.Commits
                let calcRawScore = m.score(c)
                let score = double.IsNaN(calcRawScore) ? 0 : calcRawScore
                group new { commit = c, score } by new { c.Beatmap, c.UserId } into commits // Key 是 map 和 uid，Values 是 Commits。
                let first = commits.OrderByDescending(c => c.score).First()
                group new { Score = first.score, Commit = first.commit } by commits.Key.UserId;

            return commitsGroup;
        }

        /// <summary>
        /// 将分数表达式转换为计算委托。
        /// </summary>
        /// <param name="scoreCalculation"></param>
        /// <exception cref="FormatException"></exception>
        /// <returns></returns>
        public static Func<ChartTry, double> ParseScorer(string scoreCalculation)
        {
            if (string.IsNullOrWhiteSpace(scoreCalculation)) return s => s.Score;

            var exp = new Expression<ChartTry>(scoreCalculation, Operators, Values);
            return s => exp.Evaluate(s);
        }

        private static readonly IReadOnlyDictionary<string, (Func<double, double, double> function, int priority)> Operators
            = new Dictionary<string, (Func<double, double, double> function, int priority)>
            {
                { "+", ((x, y) => x + y, 1) },
                { "-", ((x, y) => x - y, 1) },
                { "*", ((x, y) => x * y, 2) },
                { "/", ((x, y) => x / y, 2) },
                { "^", ((x, y) => Math.Pow(x, y), 3) },
            };

        private static readonly IReadOnlyDictionary<string, Func<ChartTry, double>> Values = new Dictionary<string, Func<ChartTry, double>>
        {
            { "acc",  c => c.Accuracy },
            { "accuracy",  c => c.Accuracy },
            { "combo",  c => c.Combo },
            { "score",  c => c.Score },
            { "pp",  c => c.PPWhenCommit },
            { "performance",  c => c.PPWhenCommit },
        };
    }
}
