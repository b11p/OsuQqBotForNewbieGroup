using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Addon
{
    public abstract class Addon
    {
        private HttpApiClient _qq;
        private Endpoint _endpoint;

        protected Addon() { }

        /// <exception cref="InvalidOperationException">Throws if access in constructor.</exception>
        public HttpApiClient Qq
        {
            get => _qq ?? throw new InvalidOperationException("This property is invalid in constructor.");
            internal set => _qq = value;
        }

        internal Endpoint Endpoint
        {
            get => _endpoint ?? throw new InvalidOperationException("This property is invalid in constructor.");
            set => _endpoint = value;
        }

        protected virtual async void ReplyAsync(string message) => await Qq.SendMessageAsync(Endpoint, message);

        protected virtual async void ReplyAsync(Sisters.WudiLib.Message message) => await Qq.SendMessageAsync(Endpoint, message);

        public static void Exit()
        {
#warning should throw
        }
    }
}
