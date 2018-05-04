# osu! 新人群 Bot
为 osu! 新人群提供各种功能服务，在其他群也可以使用一些基本的功能。

## 运行环境
- Windows Server 2008 R2（可选）

- .net core 2.0（必须）

- 酷Q Pro 以及 HTTP API 插件（必须）

## 配置文件
配置和数据存储在`桌面/Sheep Bot Data`文件夹下。

## 编译
1. 克隆本repo和[Sisters.WudiLib](https://github.com/int-and-his-friends/Sisters.WudiLib)

2. 移动文件位置，保证`OsuQqBotHttp.sln`和`Sisters.WudiLib.sln`按下列规则放置

    ./OsuQqBotHttp.sln

    ../Sisters.WudiLib/Sisters.WudiLib.sln

3. 使用 Visual Studio 打开`OsuQqBotHttp.sln`，编译`OsuQqBotHttp`项目

4. 运行

## 运行

1. 进入`Bleatingsheep.OsuQqBot.Database`目录，打开Powershell（能执行`dotnet`命令即可）

2. 执行`dotnet ef migrations add Init`

3. 执行`dotnet ef database update`建立数据库

4. 编译运行