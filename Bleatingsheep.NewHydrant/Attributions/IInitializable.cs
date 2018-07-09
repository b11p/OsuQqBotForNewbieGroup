namespace Bleatingsheep.NewHydrant.Attributions
{
    /// <summary>
    /// 实现此接口的功能会自动调用<see cref="Initialize"/>方法。并可能在需要的时候多次调用<see cref="Initialize"/>方法重新初始化。
    /// </summary>
    internal interface IInitializable
    {
        /// <summary>
        /// 名称，用于指令中识别。
        /// </summary>
        string Name { get; }

        bool Initialize();
    }
}
