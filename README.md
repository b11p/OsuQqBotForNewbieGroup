# osu! 新人群 Bot
为 osu! 新人群提供各种功能服务，在其他群也可以使用一些基本的功能。

## 环境及依赖 (Prerequisites)
- Ubuntu 18.04 / Docker

- .NET 5.0 (Required)

- [go-cqhttp](https://github.com/Mrs4s/go-cqhttp)

- MySQL / MariaDB (Required)

- Chromium-based browser (Recommended)

## 服务器准备 (Server Environments)
1. 运行好 [go-cqhttp](https://github.com/Mrs4s/go-cqhttp)，检查并修改生成的配置文件，确保你理解并正确配置好 `http_config` 部分。
2. 安装运行好 MySQL，确保可以连接，预留好要使用的数据库名，不要手动创建该数据库。
3. 安装好 .NET 5.0 Runtime。
4. (Optional, recommended) 安装一个 Chromium 内核浏览器。

## 本地准备 (Development Environments)
### 开发环境
1. 安装最新版本的 Visual Studio，确保选中 “.NET Core 跨平台开发”。
2. 运行以下命令
```Powershell
dotnet tool install --global dotnet-ef
```

### 克隆
1. 克隆本 repo 和 [Sisters.WudiLib](https://github.com/int-and-his-friends/Sisters.WudiLib)

2. 移动文件位置，保证 `OsuQqBotHttp.sln` 和 `Sisters.WudiLib.sln` 按下列规则放置

    ./OsuQqBotHttp.sln

    ../Sisters.WudiLib/Sisters.WudiLib.sln

### 补充缺失文件
1. 使用 Visual Studio 打开 `OsuQqBotHttp.sln`，在 `Bleatingsheep.OsuQqBot.Database/Models` 目录创建 `ServerInfo.cs` 文件，内容如下。

```C#
namespace Bleatingsheep.OsuQqBot.Database.Models
{
    internal class ServerInfo
    {
        public const string Server = "database.server.address";
        public const int Port = 12345;
        public const string Database = "database-name";
        public const string User = "your-database-user";
        public const string Password = "your-database-password";
    }
}
```

2. 在 `Bleatingsheep.NewHydrant.Bot` 项目创建 `HardcodedConfigure.cs` 文件，内容如下。

```C#
using System;

namespace Bleatingsheep.NewHydrant
{
    public sealed class HardcodedConfigure
    {
        public string ApiKey => "your-osu-api-key"; // osu! API key

        public long SuperAdmin => 123456789; // 你的 QQ 账号（非机器人账号）

        public string Listen => "http://+:8876"; // 与 go-cqhttp 配置中 `http_config.post_urls` 部分对应
        public string ApiAddress => "http://cq:5700"; // 与 go-cqhttp 配置中 `http_config.host` 和 `http_config.host.port` 字段对应。
        public string AccessToken => "your-access-token"; // 与 go-cqhttp 配置中 `access_token` 字段对应
        public string Secret => "your-secret"; // 与 go-cqhttp 配置中 `secret` 部分对应
    }
}
```
#### 参考
> [go-cqhttp 的配置文件](https://github.com/Mrs4s/go-cqhttp/blob/master/docs/config.md)

3. (Optional) 修改 `Bleatingsheep.NewHydrant.Bot/啥玩意儿啊/Chrome.cs` 文件，将 `ExecutablePath` 改为 Chromium 内核浏览器的可执行文件位置。如果未正确设置，部分功能可能无法使用。

4. 如果 MySQL 服务器未正确配置好 SSL 证书（会验证 CA 及域名是否匹配），在 `Bleatingsheep.OsuQqBot.Database/Models/NewbieContext.cs` 文件中删去 `SslMode=VerifyCA;`。

5. 尝试编译`Bleatingsheep.NewHydrant.Bot`项目，根据提示消除编译错误

6. **到这步还没完，先不要运行。**

### 创建数据库
打开 Powershell，在 `Bleatingsheep.OsuQqBot.Database` 目录运行
```Powershell
dotnet ef migrations script
```
检查将要运行的 SQL 脚本，确认后运行
```
dotnet ef database update
```
创建数据库。

## 编译 (Compilation)
1. 右键单击 `Bleatingsheep.NewHydrant.Bot` 项目，点“发布”。
2. 目标选“文件夹”，然后选择合适的文件夹。
3. 发布后，把该文件夹的文件全部复制到服务器上。

## 运行 (Run)
在服务器上打开 Powershell（或 bash 等任何 shell），运行
```Powershell
dotnet Bleatingsheep.NewHydrant.Bot.dll
```