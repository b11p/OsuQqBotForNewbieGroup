using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using PuppeteerSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    public partial class NotifyOnJoinRequest
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
            static async Task<byte[]> GetScreenshot(IPage page, string selector)
            {
                var detailElement = await page.WaitForSelectorAsync(selector);
                var data = await detailElement
                    .ScreenshotDataAsync(new ElementScreenshotOptions
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
                    // const string chartSelector = "div.profile-detail__stats";
                    // const string chartSelector = "div.profile-detail";
                    const string chartSelector = "div[data-page-id=\"main\"]";
                    var data = await GetScreenshot(page, chartSelector);
                    messageRankHistory = Message.ByteArrayImage(data);
                    hints.Add(messageRankHistory);

                    // draw ranks
                    const string bestSelector = "div[data-page-id=\"top_ranks\"]";
                    bool noBP = false;
                    var bpElement = await page.QuerySelectorAsync(bestSelector).ConfigureAwait(false);
                    if (bpElement != null)
                    {
                        if (noBP)
                        {
                            var bpCount = await bpElement.EvaluateFunctionAsync<string>("e => e.innerText").ConfigureAwait(false);
                            bpCount = bpCount.Trim();
                            hints.Add(new Message($"有{bpCount}条BP"));
                        }
                        else
                        {
                            // remove banner
                            const string bannerSelector = "body > div.osu-layout__section.osu-layout__section--full.js-content.user_show > div > div > div > div.hidden-xs.page-extra-tabs.page-extra-tabs--profile-page.js-switchable-mode-page--scrollspy-offset";
                            var banner = await page.QuerySelectorAsync(bannerSelector).ConfigureAwait(false);
                            if (banner is not null)
                            {
                                _ = await banner.EvaluateFunctionAsync("b => b.remove()").ConfigureAwait(false);
                            }

                            // scroll to bp and wait lazy load
                            await bpElement.HoverAsync();
                            const string bpLoadDetectionSelector = """
                                div[data-page-id="top_ranks"] > div > div.lazy-load > h3
                                """;
                            await page.WaitForSelectorAsync(bpLoadDetectionSelector);

                            // screenshot bests
                            data = await bpElement.ScreenshotDataAsync();
                            //data = await GetScreenshot(page, bestSelector).ConfigureAwait(false);
                            messageBest = Message.ByteArrayImage(data);
                            hints.Add(messageBest);
                        }
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

        private async Task GetForPPM(string userName, Action<Message> send) 
        {
            try
            {
                // 尝试查询 yumu ppm
                // https://bot.365246692.xyz/pub/ppm?name=-spring%20night-&mode=o
                var yumuUri = new UriBuilder("https://bot.365246692.xyz/pub/ppm");
                var query = HttpUtility.ParseQueryString(yumuUri.Query);
                query.Add("name", userName);
                query.Add("mode", "o");
                yumuUri.Query = query.ToString();
                using(HttpClient client = new HttpClient())
                {
                    try
                    {
                        client.Timeout = TimeSpan.FromSeconds(60);
                        byte[] data = await client.GetByteArrayAsync(yumuUri.Uri);
                        send(Message.ByteArrayImage(data));
                    }
                    catch (HttpRequestException ex)
                    {
                        send("yumu 服务器连接异常, 查询 ppm 失败");
                        Logger.Warn(ex);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                send("查询 ppm 失败,请参阅日志");
            }
        }
    }
}
