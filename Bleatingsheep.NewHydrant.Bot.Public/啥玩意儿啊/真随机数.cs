using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Sisters.WudiLib;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;
#nullable enable
[Component(nameof(真随机数))]
internal partial class 真随机数 : IMessageCommand
{
    private Match _match = default!;

    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        int start = 0;
        int end = 100;
        var parameterString = _match.Groups[1].Value;
        if (!string.IsNullOrWhiteSpace(parameterString))
        {
            var splits = parameterString.Split();
            if (splits.Length >= 3)
            {
                goto failed;
            }
            if (!int.TryParse(splits[0], out var num1))
            {
                goto failed;
            }
            int num2 = default;
            if (splits.Length == 1 && num1 <= 1)
            {
                goto failed;
            }
            if (splits.Length >= 2 && (!int.TryParse(splits[1], out num2) || num2 <= num1 || num2 - num1 <= 1))
            {
                goto failed;
            }
            goto success;
        failed:
            await api.SendMessageAsync(context.Endpoint, "/roll min max 或 /roll max 或 /roll");
            return;
        success:
            if (splits.Length == 1)
            {
                end = num1;
            }
            if (splits.Length == 2)
            {
                start = num1;
                end = num2;
            }
        }
        var val = RandomNumberGenerator.GetInt32(start, end);
        await api.SendMessageAsync(context.Endpoint, val.ToString());
    }

    public bool ShouldResponse(MessageContext context)
    {
        if (!context.Content.TryGetPlainText(out var text))
        {
            return false;
        }

        var regex = CommandRegex();
        var match = regex.Match(text);
        if (match.Success)
        {
            _match = match;
        }
        return match.Success;
    }

    [GeneratedRegex(@"^\s*/\s*roll\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled)]
    private static partial Regex CommandRegex();
}