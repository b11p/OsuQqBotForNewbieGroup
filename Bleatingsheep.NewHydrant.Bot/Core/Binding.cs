using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Osu;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Core
{
    [Component("bind")]
    internal class Bind : OsuFunction, IMessageCommand
    {
        public IMessageCommand Create() => new Bind();
        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            // TODO 验证用户名是否合法
            var (success, userInfo) = await OsuApi.GetUserInfoAsync(_userName, OsuMixedApi.Mode.Standard);
            if (!success)
            {
                await api.SendMessageAsync(message.Endpoint, "网络错误。");
                return;
            }
            if (userInfo == null)
            {
                await api.SendMessageAsync(message.Endpoint, "没有此用户。");
                return;
            }
            var dbResult = await Database.AddNewBindAsync(message.UserId, userInfo.Id, userInfo.Name, "自己绑定", message.UserId, userInfo.Name);
            if (dbResult.Success)
            {
                await api.SendMessageAsync(message.Endpoint, $"成功绑定为{userInfo.Name}。");
            }
            else if (dbResult.Exception is DbUpdateException && dbResult.Exception.InnerException?.Message.Contains("Duplicate", StringComparison.Ordinal) == true)
            {
                await api.SendMessageAsync(message.Endpoint, "在已绑定的情况下不允许修改，如需修改请联系 bleatingsheep。");
            }
            else
            {
                await api.SendMessageAsync(message.Endpoint, "数据库访问错误。");
                FLogger.LogException(dbResult.Exception);
            }
        }

        private const string StartCommand = "绑定";
        private string _trimed;
        private string _userName;
        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
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
}
