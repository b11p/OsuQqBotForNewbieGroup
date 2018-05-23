using Sisters.WudiLib.Posts;
using System.Threading.Tasks;

namespace OsuQqBot.Commands
{
    /// <summary>
    /// 表示可以接收命令，输出结果的类。
    /// </summary>
    abstract class CommandBase // 改名为“可以输入输出的东西”是不是更好？
    {
        protected CommandBase() { }

        private Sisters.WudiLib.HttpApiClient HttpApi => OsuQqBot.ApiV2;

        /// <summary>
        /// 输出结果的目标路径。
        /// </summary>
        protected abstract Endpoint Endpoint { get; }

        protected virtual async Task OutputAsync(string output) => await HttpApi.SendMessageAsync(Endpoint, output);

        protected virtual async Task OutputAsync(Sisters.WudiLib.Message message) => await HttpApi.SendMessageAsync(Endpoint, message);

        public abstract CommandBase Input(string message);
    }
}
