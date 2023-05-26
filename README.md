# osu! 新人群 Bot
为 osu! 新人群提供各种功能服务，在其他群也可以使用一些基本的功能。

> **Warning**
> 如果你只是想使用消防栓，可以用自己的账号创建一个分身，参考[消防栓分身](https://xfs.b11p.com/fenshen/)。
>
> 本页内容仅在贡献代码或二次开发时才需要。请注意遵守开源协议。

## 环境及依赖 (Prerequisites)
- .NET 7.0 (Required)
- [go-cqhttp](https://github.com/Mrs4s/go-cqhttp) 或任何 OneBot v11 实现
- PostgreSQL (Required)
- Chromium-based browser (Optional)

## 本地开发及调度 (Development)
### 开发环境 (Environments)
1. 配置 Prerequisites 中所需的工具。注意 .NET 必须安装 SDK。
2. 安装最新版本的 Visual Studio，确保选中 “.NET Core 跨平台开发”。

### 克隆 (Clone)
1. 克隆本 repo。

### 修改配置文件 (Configuration)
1. 将“Bleatingsheep.NewHydrant.Bot/appsettings.json.template”文件复制在相同目录下，重命名为“appsettings.json”。
2. 编辑“appsettings.json”，将 `NewbieDatabase_Postgres` 修改为 PostgreSQL 的[连接字符串](https://www.connectionstrings.com/npgsql/)。
3. 将 `ApiKey` 修改为 osu! API v1 key。
4. 将 `SuperAdmin` 修改为你的 QQ 号（非 bot 账号，该账号具有最高权限）。
5. 将 `ServerPort` 修改为反向 ws 监听端口。
6. (Optional) `Chrome` 下的 `Path` 修改为 Chrome 浏览器的路径，如果未正确设置，部分功能可能无法使用。

#### 参考 (Reference)
> [go-cqhttp 的配置文件](https://github.com/Mrs4s/go-cqhttp/blob/master/docs/config.md)

### 本地运行及测试 (Test)
尝试编译`Bleatingsheep.NewHydrant.Bot`项目，根据提示消除编译错误。

使用 go-cqhttp 的“反向 WebSocket”模式连接 `ServerPort` 中配置的端口。

## 部署 (Deployment)
接下来将说明如果部署至生产环境。请注意本文只提供基本方法，关于你服务器上的

### 服务器准备 (Server Environments)
1. 首先安装 Prerequisites 环境，.NET 安装 runtime 即可。

### 编译 (Compilation)
接下来将编译

1. 右键单击 `Bleatingsheep.NewHydrant.Bot` 项目，点“发布”。
2. 目标选“文件夹”，然后选择合适的文件夹。
3. 发布后，把该文件夹的文件全部复制到服务器上。

如果使用命令行，则运行以下命令（和上面的步骤二选一）：

```sh
dotnet publish Bleatingsheep.NewHydrant.Bot -c Release -o <output_dir>
```

### 配置
1. 创建“appsettings.json”文件，并按上方相同方法配置。<br/>此文件应该与“Bleatingsheep.NewHydrant.Bot.dll”放在同一目录。
2. 如果希望提供公共服务，将 `ServerAccessToken` 修改为要求客户端（OneBot 实现）设置的 Token。

### 添加账号权限
先在服务器上运行一次，创建数据库结构，然后连接数据库，在 `DuplicateAuthentication` 表中添加 Bot 账号以及 AccessToken，请注意务必与公开 Token 不同。该账号使用此 Token 将具有高权限。

### 运行 (Run)
在服务器上打开 Powershell（或 bash 等任何 shell），运行
```Powershell
dotnet Bleatingsheep.NewHydrant.Bot.dll
```

要连接 go-cqhttp，请使用“反向 WebSocket”模式，并按照数据库中的 Token 进行配置。

## 高权限和低权限有什么区别？
高权限的 Bot 账号可以使用绑定功能，低权限没有，这是为了防止伪造绑定和管理请求。

## 开源协议 (LICENSE)
项目主体部分使用 AGPL 协议授权，框架部分（Bleatingsheep.NewHydrant 文件夹）使用 MIT 协议授权。

### 其他项目
本项目可能用到了其他项目的代码，遵守其协议，在此列出。

|链接|协议|
|-|-|
|https://github.com/dotnet/runtime|MIT|