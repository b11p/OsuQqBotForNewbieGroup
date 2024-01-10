using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Microsoft.Extensions.Caching.Memory;
using Sisters.WudiLib;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;
#nullable enable
[Component("ADHD")]
internal class Adhd : IMessageCommand
{
    private readonly IMemoryCache _cache;
    private bool _isInit;
    private int _answer;

    public Adhd(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        var asrsInfo = _cache.GetOrCreate($"ADHDTest.{context.UserId}", e =>
        {
            e.SlidingExpiration = TimeSpan.FromMinutes(10);
            return new AsrsInfo();
        });
        Debug.Assert(asrsInfo != null);
        if (_isInit)
        {
            var sb = new StringBuilder();
            sb.Append(asrsInfo.GetCurrentHint());
            await api.SendMessageAsync(context.Endpoint, sb.ToString());
            return;
        }

        if (!asrsInfo.Answer(_answer))
        {
            var sb = new StringBuilder();
            sb.AppendLine("回答有误，请重新回答~");
            sb.Append(asrsInfo.GetCurrentHint());
            await api.SendMessageAsync(context.Endpoint, sb.ToString());
            return;
        }
        else
        {
            var sb = new StringBuilder();
            sb.Append(asrsInfo.GetCurrentHint());
            await api.SendMessageAsync(context.Endpoint, sb.ToString());
            return;
        }
    }

    public bool ShouldResponse(MessageContext context)
    {
        if (context.Content.TryGetPlainText(out string text))
        {
            if (text.Equals("/adhd", StringComparison.OrdinalIgnoreCase))
            {
                _isInit = true;
                return true;
            }
            if (_cache.TryGetValue<AsrsInfo>($"ADHDTest.{context.UserId}", out var asrsInfo))
            {
                Debug.Assert(asrsInfo != null);
                return !asrsInfo.IsOver() && int.TryParse(text, out _answer);
            }
        }
        return false;
    }

    public sealed class AsrsInfo
    {
        private static readonly IReadOnlyList<(string question, int minOptionToScore)> s_questions = [
            ("1. 在完成其中最艰难的部分之后，您在处理某一项目的最后细节时是否常常有困难？", 3),
            ("2. 您在完成具有组织性质的任务时，是否时常有困难把事情整理安排好？", 3),
            ("3. 您是否时常有困难记住约会或应做的事？", 3),
            ("4. 如果一件事需要多动脑筋，您是否常常躲避或推延开始做它？", 4),
            ("5. 如果您不得不长时间坐下，您是否常常蠕动不安或者手脚动个不停？", 4),
            ("6. 您是否时常感到过度活跃，强迫自己做事，就像上了发条的机器?", 4),
        ];
        private static readonly string s_options = """
            1. 从不
            2. 很少
            3. 有时
            4. 经常
            5. 很经常
            """;

        private int _answeredQuestions = 0;
        private int _totalScore = 0;

        public string GetCurrentHint()
        {
            var sb = new StringBuilder();
            if (_answeredQuestions == 0)
            {
                sb.AppendLine("下面开始 ADHD 自评问卷，本问卷仅供智力正常的成年人使用（年龄>=18且IQ>=80），请根据个人情况回复相应数字~");
            }
            if (!IsOver())
            {
                sb.AppendLine(s_questions[_answeredQuestions].question);
                sb.Append(s_options);
            }
            else
            {
                sb.AppendLine($"您的测试分数为{_totalScore}");
                if (GetTotalScores() >= 4)
                {
                    sb.AppendLine("测试结果表明您可能患有 ADHD，如您生活中有相应困扰，可以考虑向合适的医生寻求帮助!");
                }
                else
                {
                    sb.AppendLine("测试结果并未表明您可能患有 ADHD，如您生活中有相应困扰，可以考虑向合适的医生寻求帮助!");
                }

                sb.AppendLine("本问卷的敏感性为 68.7%，特异性为 99.5%。仅供参考，不构成医疗建议~");
                sb.Append("参考资料：https://embrace-autism.com/asrs-v1-1/");
            }
            return sb.ToString();
        }

        public bool Answer(int directValue)
        {
            if (IsOver() || directValue > 5 || directValue < 1)
            {
                return false;
            }

            if (directValue >= s_questions[_answeredQuestions].minOptionToScore)
                _totalScore += 1;
            _answeredQuestions++;

            return true;
        }

        public bool IsOver() => _answeredQuestions >= s_questions.Count;

        public int GetTotalScores() => _totalScore;
    }
}
