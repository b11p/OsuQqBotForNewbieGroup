﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Extentions;
using Bleatingsheep.SimpleBooru;
using Microsoft.Extensions.Caching.Memory;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Moebooru
{
    [Component("advanced_konachan")]
    class AdvancedKonachan : Service, IMessageCommand
    {
        private static readonly Regex s_regex = new Regex(
            @"^\s*(健康)?(?<website>konachan|yandere)\s*(?<count>\d*)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly IReadOnlyDictionary<string, IBooruClient> s_Booru = new Dictionary<string, IBooruClient>
        {
            { "KONACHAN", BooruFactory.Konachan },
            { "YANDERE", BooruFactory.Yandere },
        };

        private static readonly IReadOnlyDictionary<string, string> s_ReverseProxy = new Dictionary<string, string>
        {
            { "KONACHAN", "https://xfs-proxy-konachan.b11p.com/" },
            { "YANDERE", "https://xfs-proxy-yandere.b11p.com/" },
        };

        private static readonly IReadOnlyDictionary<string, IEnumerable<string>> s_dissTags = new Dictionary<string, IEnumerable<string>>
        {
            { "KONACHAN", Konachan.DissTags },
        };

        private string _proxyDomain;

        [Parameter("count")]
        public string WantedCount { get; set; }

        private int GetCount() => int.TryParse(WantedCount, out int result) ? result : 1;

        [Parameter("website")]
        public string Website { get; set; }

        [Parameter("1")]
        public string HealthyFlag { get; set; }

        private bool IsHealthy => !string.IsNullOrEmpty(HealthyFlag);

        private static readonly ICollection<long> EnhancedGroups = new HashSet<long>
        {
            308419061, // Steam
            601110599, // requested by 瓜皇
            514661057,
            851868928,
            661021255,
        };

        private static Task<bool> IsVipAsync(long qq) => Task.FromResult(false);

        private static readonly ISet<long> _promotedGroups = new HashSet<long>
        {
            851868928,
            661021255,
            338371278,
            184128198,
            750899485,
        };

        private static Task<IEnumerable<long>> GetPromotedGroupsAsync() => Task.FromResult(_promotedGroups as IEnumerable<long>);

        private static async Task<bool> AllowMulti(MessageContext context)
            => !(context.Endpoint is GroupEndpoint g)
            || (await GetPromotedGroupsAsync().ConfigureAwait(false)).Contains(g.GroupId);

        private static Task<bool> ShouldRandomize(Endpoint endpoint)
            => Task.FromResult(endpoint is PrivateEndpoint || endpoint is GroupEndpoint g && EnhancedGroups.Contains(g.GroupId));

        private readonly IMemoryCache _memoryCache;
        public AdvancedKonachan(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            if (s_Booru.TryGetValue(Website.ToUpperInvariant(), out IBooruClient booru))
            {
                _proxyDomain = s_ReverseProxy.GetValueOrDefault(Website.ToUpperInvariant());
                IEnumerable<string> diss;
                if (IsHealthy)
                {
                    if (!s_dissTags.TryGetValue(Website.ToUpperInvariant(), out diss))
                        throw new ExecutingException("这个网站不支持健康。");
                }
                else
                {
                    diss = Enumerable.Empty<string>();
                }

                var count = await AllowMulti(context).ConfigureAwait(false) ? GetCount() : 1;

                var result = (await booru.GetPopularRecentAsync())
                    .Where(p => p.Rating == Rating.Safe && p.Status == Status.Active && !p.Tags.Split().Intersect(diss).Any());
                if (await ShouldRandomize(context.Endpoint))
                {
                    result = result.Randomize();
                }
                result = result.Take(count).ToList();

                foreach (var post in result)
                {
                    await TrySend(post, context.Endpoint, api);
                }
                if (result.Count() != count)
                {
                    await api.SendMessageAsync(context.Endpoint, $"只有 {result.Count()} 张图。");
                }
                if (count != GetCount())
                {
                    await api.SendMessageAsync(context.Endpoint, $"私聊可以查看多张图片。");
                }
            }
            else
            {
                Logger.Error($"网站错误，原始消息是(RAW) {context.Content.Raw}，解析网站是 {Website}");
                throw new ExecutingException("网站错误，为什么会这样呢？");
            }
            //await api.SendMessageAsync(context.Endpoint, new Message($"(DEBUG) 健康：{IsHealthy}, Website: {Website}, 数量：{GetCount()}"));
        }

        private async Task TrySend(SimpleBooru.Post post, Endpoint endpoint, HttpApiClient api)
        {
            if (await TrySendImage(endpoint, api, post.Id, post.FileUrl, post.FileSize, post.Width, post.Height))
            {
                return;
            }
            else if (post.JpegUrl != post.FileUrl && await TrySendImage(endpoint, api, post.Id, post.JpegUrl, post.JpegFileSize, post.Width, post.Height))
            {
                return;
            }
            else if (await TrySendImage(endpoint, api, post.Id, post.SampleUrl, post.SampleFileSize, post.SampleWidth, post.SampleHeight))
            {
                return;
            }
            else if (await TrySendImage(endpoint, api, post.Id, post.PreviewUrl, 0, post.PreviewWidth, post.PreviewHeight))
            {
                return;
            }
            return;
        }

        private async Task<bool> TrySendImage(Endpoint endpoint, HttpApiClient api, int id, Uri uri, int length, int width, int height)
        {
            // string url = string.IsNullOrEmpty(_proxyDomain) ? uri.AbsoluteUri : Regex.Replace(uri.AbsoluteUri, @"^https?://.+?/", _proxyDomain);
            // bool success = await api.SendMessageAsync(endpoint, Message.NetImage(url)).ConfigureAwait(false) != default;
            using var httpClient = new HttpClient();
            bool success = false;
            try
            {
                var cacheKey = $"{nameof(AdvancedKonachan)}::{uri.AbsoluteUri}";
                if (_memoryCache.TryGetValue<ImageCacheEntry>(cacheKey, out var entry))
                {
                    success = entry.IsSuccess
                        && await api.SendMessageAsync(endpoint, Message.ByteArrayImage(entry.Data)).ConfigureAwait(false) != default;
                }
                else
                {
                    var data = await httpClient.GetByteArrayAsync(uri);
                    if (data.Length > 10 * 1024 * 1024)
                    {
                        _memoryCache.Set(cacheKey, new ImageCacheEntry(false, null), new MemoryCacheEntryOptions
                        {
                            Size = 10,
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                        });
                        success = false;
                    }
                    else
                    {
                        _memoryCache.Set(cacheKey, new ImageCacheEntry(true, data), new MemoryCacheEntryOptions
                        {
                            Size = data.Length,
                            SlidingExpiration = TimeSpan.FromHours(1),
                        });
                        success = await api.SendMessageAsync(endpoint, Message.ByteArrayImage(data)).ConfigureAwait(false) != default;
                    }
                }
            }
            catch
            {
                success = false;
            }
            Logger.Debug($"ID: {id}, length: {length}, {width}x{height}, success={success}, url: {uri} ");
            return success;
        }

        private record struct ImageCacheEntry(bool IsSuccess, byte[] Data);

        public bool ShouldResponse(MessageContext context)
            => RegexCommand(s_regex, context.Content) && (!(context is GroupMessage g) || !(g.GroupId == 595985887 || g.GroupId == 231094840));
    }
}
