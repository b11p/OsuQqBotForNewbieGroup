using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.啥玩意儿啊;
using PuppeteerSharp;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;
using Mode = Bleatingsheep.Osu.Mode;
using UserInfo = Bleatingsheep.OsuMixedApi.UserInfo;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    partial class NotifyOnJoinRequest
    {
        private async Task<TrustedUserInfo> ProcessApplicantReportAsync(List<Message> hints, string fallbackName, (bool networkSuccess, TrustedUserInfo) userTuple)
        {
            var (success, info) = userTuple;
            var userName = info?.Name ?? fallbackName;
            var basicInfo = (success, info) switch
            {
                (false, _) => "查询失败。",
                (true, null) => "不存在此用户。",
                (_, _) => $"PP: {info.Performance}, PC: {info.PlayCount}, TTH: {info.TotalHits}",
            };// 包含 PC 、PP等基础信息。
            var hint = $"{userName}: {basicInfo}";
            hints.Add(new Message(hint));
            if (info?.IsBanned == false)
                await ScreenShotsAsync(hints, info.Id).ConfigureAwait(false);
            return info;
        }

        private async Task ScreenShotsAsync(List<Message> hints, int uid)
        {
            async Task<byte[]> GetScreenshot(Page page, string selector)
            {
                ElementHandle detailElement = await page.WaitForSelectorAsync(selector);
                var data = await detailElement
                    .ScreenshotDataAsync(new ScreenshotOptions
                    {
                    });
                return data;
            }

            try
            {
                Message messageRankHistory, messageBest;
                using (var page = await Chrome.OpenNewPageAsync())
                {
                    await page.SetViewportAsync(new ViewPortOptions
                    {
                        DeviceScaleFactor = 2.5,
                        Width = 1440,
                        Height = 900,
                    });
                    var response = await page.GoToAsync($"https://osu.ppy.sh/users/{uid}/osu").ConfigureAwait(false);
                    if (response.Status == System.Net.HttpStatusCode.NotFound) return;
                    // draw history
                    const string chartSelector = "body > div.osu-layout__section.osu-layout__section--full.js-content.user_show > div > div > div > div.js-switchable-mode-page--scrollspy.js-switchable-mode-page--page > div.osu-page.osu-page--users > div > div.profile-detail > div:nth-child(2)";
                    var data = await GetScreenshot(page, chartSelector);
                    messageRankHistory = Message.ByteArrayImage(data);
                    hints.Add(messageRankHistory);

                    // draw ranks
                    const string bestSelector = "body > div.osu-layout__section.osu-layout__section--full.js-content.user_show > div > div > div > div.user-profile-pages.ui-sortable > div > div > div:nth-child(2) > div > div.play-detail-list";
                    const string bestFallbackSelector = "body > div.osu-layout__section.osu-layout__section--full.js-content.user_show > div > div > div > div.user-profile-pages.ui-sortable > div > div > div:nth-child(2) > h3 > span.title__count";
                    bool noBP = false;
                    ElementHandle bpElement = await page.QuerySelectorAsync(bestSelector).ConfigureAwait(false);
                    if (bpElement is null)
                    {
                        noBP = true;
                        bpElement = await page.QuerySelectorAsync(bestFallbackSelector).ConfigureAwait(false);
                    }
                    if (bpElement != null)
                    {
                        data = await bpElement.ScreenshotDataAsync();
                        //data = await GetScreenshot(page, bestSelector).ConfigureAwait(false);
                        messageBest = Message.ByteArrayImage(data);
                        hints.Add(noBP ? new Message("有") + messageBest + new Message("条BP") : messageBest);
                    }
                    else
                    {
                        hints.Add(new Message("查询 BP 失败。既没有找到 BP 数据，也没有找到未上传成绩的说明。"));
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                Logger.Warn(ex);
                hints.Add(new Message(ex.Message));
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
