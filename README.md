# osu! 新人群 Bot
为 osu! 新人群提供各种功能服务，在其他群也可以使用一些基本的功能。

## 环境及依赖 (Prerequisites)
- Ubuntu 22.04 / Docker

- .NET 6.0 (Required)

- [go-cqhttp](https://github.com/Mrs4s/go-cqhttp)

- PostgreSQL (Required)

- Chromium-based browser (Optional)

## 服务器准备 (Server Environments)
1. 运行好 [go-cqhttp](https://github.com/Mrs4s/go-cqhttp)，检查并修改生成的配置文件，确保你理解并正确配置好 `http_config` 部分。
2. 安装运行好 PostgreSQL，确保可以连接，预留好要使用的数据库名，不要手动创建该数据库。
3. 安装好 .NET 6.0 Runtime。
4. (Optional) 安装一个 Chromium 内核浏览器。

## 本地准备 (Development Environments)
### 开发环境
1. 安装最新版本的 Visual Studio，确保选中 “.NET Core 跨平台开发”。
2. 运行以下命令
```Powershell
dotnet tool install --global dotnet-ef
```

### 克隆
1. 克隆本 repo。

### 修改配置文件
待更新。

#### 参考
> [go-cqhttp 的配置文件](https://github.com/Mrs4s/go-cqhttp/blob/master/docs/config.md)

(Optional) 修改 `Bleatingsheep.NewHydrant.Bot/啥玩意儿啊/Chrome.cs` 文件，将 `ExecutablePath` 改为 Chromium 内核浏览器的可执行文件位置。如果未正确设置，部分功能可能无法使用。

尝试编译`Bleatingsheep.NewHydrant.Bot`项目，根据提示消除编译错误

待更新。

### 创建数据库
待更新。

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

## 开源协议 (LICENSE)
项目主体部分使用 AGPL 协议授权，框架部分（Bleatingsheep.NewHydrant 文件夹）使用 MIT 协议授权。

项目使用到的其他开源软件列表待更新。