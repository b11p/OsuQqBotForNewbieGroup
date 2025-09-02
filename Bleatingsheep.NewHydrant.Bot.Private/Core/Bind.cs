using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu.ApiClient;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Core;
[Component("bind")]
public class Bind : Service, IMessageCommand
{
    public Bind(OsuMixedApi.OsuApiClient osuApi, ILogger<Bind> logger, IOsuApiClient osuApiClient, IOsuDataUpdator osuDataUpdator)
    {
        OsuApi = osuApi;
        _logger = logger;
        _osuApiClient = osuApiClient;
        _osuDataUpdator = osuDataUpdator;
    }
    private readonly ILogger<Bind> _logger;
    private readonly IOsuApiClient _osuApiClient;
    private readonly IOsuDataUpdator _osuDataUpdator;

    public OsuMixedApi.OsuApiClient OsuApi { get; }

    public async Task ProcessAsync(MessageContext message, HttpApiClient api)
    {
        UserInfo userInfo;
        try
        {
            userInfo = await _osuApiClient.GetUser(_userName).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            if (!e.Message.Contains("429"))
            {
                _logger.LogError(e, "绑定时出现错误，目标用户：{_userName}", _userName);
            }
            await api.SendMessageAsync(message.Endpoint, "网络错误。").ConfigureAwait(false);
            return;
        }
        if (userInfo == null)
        {
            await api.SendMessageAsync(message.Endpoint, "没有此用户。").ConfigureAwait(false);
            return;
        }
        if (userInfo.Id > int.MaxValue)
        {
            await api.SendMessageAsync(message.Endpoint, "osu! id 大于 32 位整型最大值。这游戏真有这么多人玩儿？请加群 563180497 联系开发者处理。").ConfigureAwait(false);
            return;
        }
        var (isChanged, oldOsuId, _) = await _osuDataUpdator.AddOrUpdateBindingInfoAsync(message.UserId, (int)userInfo.Id, userInfo.Name, "自己绑定", message.UserId, userInfo.Name).ConfigureAwait(false);
        if (isChanged)
        {
            await api.SendMessageAsync(message.Endpoint, $"成功绑定为{userInfo.Name}。").ConfigureAwait(false);
        }
        else if (oldOsuId == userInfo.Id)
        {
            await api.SendMessageAsync(message.Endpoint, "你已经绑定了这个账号。").ConfigureAwait(false);
        }
        else
        {
            await api.SendMessageAsync(message.Endpoint, "在已绑定的情况下不允许修改，如需修改请联系新人群管理（新人群相关）或加群 563180497（新人群以外）。").ConfigureAwait(false);
        }
    }

    private const string StartCommand = "绑定";
    private string _trimed;
    private string _userName;

    public bool ShouldResponse(MessageContext message)
    {
        if (message.Content.IsPlaintext)
        {
            _trimed = message.Content.Text.TrimStart();
            if (_trimed.StartsWith(StartCommand, StringComparison.InvariantCultureIgnoreCase))
            {
                _userName = _trimed.Substring(StartCommand.Length).Trim();
                return Bleatingsheep.Osu.Helper.UserNameHelper.IsUserName(_userName);
            }
        }
        return false;
    }
}
