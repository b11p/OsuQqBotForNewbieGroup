using System;
using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Bleatingsheep.OsuQqBot.Database
{
    public static class NewbieDatabase
    {
        /// <exception cref="NewbieDbException"></exception>
        private static T TryUsingContext<T>(Func<NewbieContext, T> func)
        {
            try
            {
                using (var context = new NewbieContext())
                {
                    return func(context);
                }
            }
            catch (Exception e)
            {
                throw new NewbieDbException(e.Message, e);
            }
        }

        /// <exception cref="NewbieDbException"></exception>
        private static async Task<T> TryUsingContextAsync<T>(Func<NewbieContext, Task<T>> func)
        {
            try
            {
                using (var context = new NewbieContext())
                {
                    return await func(context);
                }
            }
            catch (Exception e)
            {
                throw new NewbieDbException(e.Message, e);
            }
        }

        /// <summary>
        /// 绑定 QQ 号和 osu! 账号。
        /// </summary>
        /// <param name="qq">要绑定的 QQ 号。</param>
        /// <param name="osuId">要绑定的 osu! UID。</param>
        /// <param name="osuName">记录在日志里的 osu! 用户名。</param>
        /// <param name="source">绑定来源。记录在日志和绑定信息里。</param>
        /// <param name="operatorId">操作者 QQ 号。记录在日志里。</param>
        /// <param name="operatorName">操作者名字。记录在日志里。</param>
        /// <exception cref="NewbieDbException">绑定过程出现异常。</exception>
        /// <returns>以前的 osu! UID。</returns>
        public static async Task<int?> BindAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName)
        {
            try
            {
                using (var context = new NewbieContext())
                {
                    var bindingInfo = await context.Bindings.SingleOrDefaultAsync(b => b.UserId == qq);
                    var oldOsuId = bindingInfo?.OsuId;
                    if (bindingInfo is BindingInfo)
                    {
                        if (bindingInfo.OsuId == osuId)
                        {
                            return osuId;
                        }
                        bindingInfo.OsuId = osuId;
                        bindingInfo.Source = source;
                    }
                    else
                    {
                        bindingInfo = new BindingInfo { OsuId = osuId, UserId = qq, Source = source };
                        await context.Bindings.AddAsync(bindingInfo);
                    }
                    await context.Histories.AddAsync(new OperationHistory
                    {
                        Operation = Operation.Binding,
                        UserId = qq,
                        User = osuName,
                        OperatorId = operatorId,
                        Operator = operatorName,
                        Remark = $"osu! ID: {osuId}; source: {source}",
                    });
                    await context.SaveChangesAsync();
                    return oldOsuId;
                }
            }
            catch (Exception e)
            {
                throw new NewbieDbException(e.Message, e);
            }
        }

        /// <summary>
        /// 获取绑定信息。
        /// </summary>
        /// <param name="qq">QQ 号。</param>
        /// <exception cref="NewbieDbException"></exception>
        /// <returns>绑定信息。如果没绑定，则为 <c>null</c>。</returns>
        public static async Task<BindingInfo> GetBindingInfoAsync(long qq)
        {
            return await TryUsingContextAsync(async context =>
            {
                return await context.Bindings.SingleOrDefaultAsync(b => b.UserId == qq);
            });
        }

        /// <exception cref="NewbieDbException"></exception>
        public static async Task<int?> GetBindingIdAsync(long qq)
        {
            return (await GetBindingInfoAsync(qq))?.OsuId;
        }
    }
}
