重构：
Assembly/Service?
Service/Assembly?

ProcedureException

motd (message of the day)
将消防栓的群名片改为今日消息

一个想法：
利用 C# 里的插值字符串，实现类似 ASP .NET 的 View 功能。

用下面的方式处理命令：
```c#
[RegexCommand("here is regex")] // 或者 [Command.Regex("")] ？其中RegexAttribute是嵌套类。
public async Task<string/Message> SomeCommandX(string param1, int param2, ...) // 也许可以增加自定义类型转换功能。
{
    /*
     * Regex 处理流程：
     * 纯文本：正常处理
     * 单个 CQ 码：不处理（是否可以增加处理单个 CQ 码的机制？处理分享什么的，单个表情默认排除？某些情况下可以被识别为命令？）
     * 文本使用转义后的，记录下 CQ 码的下标起止，如果匹配会撕裂 CQ 码，则视为失败。
     * 如果想匹配 CQ 码，也许可以尝试在 CQ 码前面加某个符号（比如'$'？），加的数量比消息中连续'$'的数量多，然后每条消息分别生成一个正则。
     * （也许替换成单个字符进行匹配？表情/图片->私人使用区，设计一个表情到私人使用区的映射，每张不同的图片都用一个不同的码位）
     * 手机QQ就是用的这种方法实现的，但用电脑发表示表情的特殊字符，电脑上还是特殊字符，手机上显示表情（酷Q疑似收到特殊字符而不是表情/CQ码）
     */

    /*
     * 转换器想法：
     * 默认转换器：只转换纯文本，除非是 Message 类型，否则视为匹配失败
     * 自定义转换器：可接收 Message 或 string 类型，如果接收的是 string，且不是纯文本，则视为匹配失败。
     */
}

[Command("command {param1} {param2}")]
[Command.MultiLine(@"command
{param1}
{param2}")] // 还没想好多行的怎么实现
public async Task<string/Message> SomeCommandX(int param1, Message/[CQType("image")]MessageSegment param2) // 可以保留表情等等
{

}
```