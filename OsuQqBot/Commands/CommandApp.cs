using Sisters.WudiLib.Posts;

namespace OsuQqBot.Commands
{
    /// <summary>
    /// 用来控制上下文命令的启动和执行的类。
    /// </summary>
    abstract class CommandApp : CommandBase
    {
        protected CommandApp() { }

        protected virtual CommandBase Base { get; } = null;

        protected abstract MessageSource User { get; }

        public abstract string Introduce();

        public abstract string Help();

        public abstract string Help(string param);

        public abstract CommandApp Start(Endpoint endpoint, MessageSource user, string param);
    }
}
