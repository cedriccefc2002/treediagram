# treediagram

一個可讓客端透過API編輯樹狀圖(tree diagram)的網路服務

[![GUI功能測試影片 2018-6-27](https://img.youtube.com/vi/FOpUedw45ek/0.jpg)
](https://youtu.be/FOpUedw45ek)

## 簡報ppt

- git [https://github.com/cedriccefc2002/treediagram-website](https://github.com/cedriccefc2002/treediagram-website)
- 網址 [https://cedriccefc2002.github.io/treediagram-website/publish/ppt.html](https://cedriccefc2002.github.io/treediagram-website/publish/ppt.html)

## [軟體設計文件](./doc/SDD.md)

## [windows安裝文件](./script/Windows/Install.md)

參考範例：

- [朝陽科技大學資訊管理系-文件範例](http://www.im.cyut.edu.tw/html/project/download.htm)：[SDD](http://www.im.cyut.edu.tw/html/project/sdd.pdf)文件

- [IEEE Std 1016-1998《软件设计描述IEEE推荐实践》阅读摘要](https://blog.csdn.net/skydreamer01/article/details/2943333)

## Glacier2 執行

```sh
glacier2router --Ice.Config=src/glacier2.conf
```

Gui Client

```js
const Ice = require("ice").Ice;
const Glacier2 = require("ice").Glacier2;
// 設定Router
let ic = Ice.initialize([
    "--Ice.Default.Router=PublicRouter/router:tcp -h 127.0.0.1 -p 10001",
    "--Ice.RetryIntervals=-1",
    "--Ice.ACM.Client=0"
]);
let router = await Glacier2.RouterPrx.checkedCast(ic.getDefaultRouter());
// 設定帳號密碼
let session = await router.createSession("username", "password");
let proxy = ic.stringToProxy("Server:tcp -h 127.0.0.1 -p 10000");
```

[Client] <-> [Glacier2:10001] <-> [Server:10000]

## Neo4j 資料庫安裝

1. 使用 **docker** 安裝資料儲存目錄設定為`/home/cefc/Data/neo4j`
    ```sh
    sudo docker run --name neo4j -p=7474:7474 -p=7687:7687 -v=/home/cefc/Data/neo4j:/data -d neo4j
    ```
1. 使用瀏覽器登入 http://localhost:7474
1. 預設帳密 **neo4j**/**neo4j**，變更密碼為：**12369874**

## 安裝 [Slice Compiler](https://zeroc.com/downloads/ice#dotnet-core) 與 runtime 程式

```sh
sudo apt-key adv --keyserver keyserver.ubuntu.com --recv B6391CB2CFBA643D
sudo apt-add-repository "deb http://zeroc.com/download/Ice/3.7/ubuntu18.04 stable main"
sudo apt-get update
sudo apt-get install zeroc-ice-compilers
sudo apt-get install zeroc-ice-all-runtime
```

## Server 專案

### Server分層架構

- `lib/Config`： 設定參數
- `lib/Repository`： 提供資料庫存取
- `lib/Repository/Domain`：**Repository**產生的資料或呼叫的變數型態
- `lib/Service`： 提供邏輯判斷程序
- `lib/Service/Model`：**Service**產生的資料或呼叫的變數型態
- `lib/IceBridge`：提供Ice產生Api的橋接
- `lib/Provider.cs`： 層與層之間的相依性注入
- `IceService.cs`：可獨立執行或嵌入`ICEbox`

### Dependency Injection

- 建立 IRepository 介面
- Neo4jRepository 實做 IRepository
- `lib/Provider.cs` 設定相依性

    ```cs
    services.AddSingleton<Repository.IRepository, Repository.Neo4jRepository>();
    ```
- `lib/Service/ServerService.cs` 使用 **IRepository** 介面

    ```cs
    private readonly IRepository repository;
    public ServerService()
    {
        repository = lib.Provider.serviceProvider.GetRequiredService<IRepository>();
    }
    public async Task<bool> Status()
    {
        return await repository.Status();
    }
    ```

### 加入protobuf

```sh
cd ./src
git submodule add https://github.com/cedriccefc2002/protobuf
mkdir Protos
cd Protos
dotnet new classlib
mkdir generated
/home/cefc/.nuget/packages/grpc.tools/1.12.0/tools/linux_x64/protoc --plugin=protoc-gen-grpc=/home/cefc/.nuget/packages/grpc.tools/1.12.0/tools/linux_x64/grpc_csharp_plugin --csharp_out=generated --grpc_out=generated Server.proto
```

```xml
<ItemGroup>
    <PackageReference Include="Grpc" Version="1.12.0" />
    <ProjectReference Include="..\protobuf\csharp\src\Google.Protobuf\Google.Protobuf.csproj" />
  </ItemGroup>
```


### Server 專案建置

- 使用 dotnet 還原

```sh
dotnet restore
```

- 手動新增

```sh
dotnet new console -o src/Server
dotnet add src/Server package zeroc.icebuilder.msbuild
dotnet add src/Server package zeroc.ice.net
dotnet add src/Server package Neo4j.Driver
# NLog
dotnet add src/Server package Microsoft.Extensions.DependencyInjection
dotnet add src/Server package NLog
dotnet add src/Server package NLog.Extensions.Logging
# Configuration
dotnet add src/Server package Microsoft.Extensions.Configuration
dotnet add src/Server package Microsoft.Extensions.Configuration.FileExtensions
dotnet add src/Server package Microsoft.Extensions.Configuration.Json
# AutoMapper
dotnet add src/Server package AutoMapper
```

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
    <ItemGroup>
        <SliceCompile Include="../Slice/*.ice" />
    </ItemGroup>
    <Target Name="CopyCustomContent" AfterTargets="AfterBuild">
        <!--複製NLog設定檔案-->
        <Copy SourceFiles="nlog.config" DestinationFolder="$(OutDir)" />
        <!--複製appsettings.json設定檔案-->
        <Copy SourceFiles="appsettings.json" DestinationFolder="$(OutDir)" />
    </Target>
    <Target Name="CopyCustomContentOnPublish" AfterTargets="Publish">
        <Copy SourceFiles="nlog.config" DestinationFolder="$(PublishDir)" />
        <Copy SourceFiles="appsettings.json" DestinationFolder="$(PublishDir)" />
     </Target>
</Project>
```

### Server 專案編譯

```sh
dotnet build src/Server
```

### Server 專案發布：win-x64，win-x8，linux-x64

```sh
# 輸出目錄到 src/Server/bin/Release/netcoreapp2.1/win-x64
dotnet publish --self-contained -c Release -r win-x64 src/Server
# 輸出目錄到 src/Server/bin/Release/netcoreapp2.1/win-x86
dotnet publish --self-contained -c Release -r win-x86 src/Server
# 輸出目錄到 src/Server/bin/Release/netcoreapp2.1/linux-x64
dotnet publish --self-contained -c Release -r linux-x64 src/Server
```

### Server 專案執行：

```sh
# 執行 linux-x64
cd ./src/Server/bin/Release/netcoreapp2.1/linux-x64/publish/
./Server
```

```bat
# 執行 win-x64
cd .\src\Server\bin\Release\netcoreapp2.1\win-x64/publish\
.\Server.exe
# 執行 win-x86
cd .\src\Server\bin\Release\netcoreapp2.1\win-x86/publish\
.\Server.exe
```

### Server 專案 佈署成 windows 服務：

- 使用 IceGrid + IceBox

- 使用 [nssm](http://nssm.cc/usage) （Licence：[公有領域](https://git.nssm.cc/?p=nssm.git;a=blob_plain;f=README.txt)）

```bat
nssm install treediagram_server
```

![連線測試](http://nssm.cc/images/install_application.png)

## Server.Tests 測試專案

### Server.Tests 專案建置

```sh
dotnet new mstest -o src/Server.Tests -lang C#
dotnet add src/Server.Tests reference src/Server/Server.csproj
dotnet add src/Server.Tests package Microsoft.Extensions.DependencyInjection
```

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
    <Target Name="CopyCustomContent" AfterTargets="AfterBuild">
        <!--複製NLog設定檔案-->
        <Copy SourceFiles="nlog.config" DestinationFolder="$(OutDir)" />
        <!--複製appsettings.json設定檔案-->
        <Copy SourceFiles="appsettings.json" DestinationFolder="$(OutDir)" />
  </Target>
</Project>
```

### Server.Tests 專案執行：

```sh
cd src/Server.Tests
dotnet test
```

## ConsoleClient 專案

```sh
dotnet new console -o src/ConsoleClient
dotnet add src/ConsoleClient package zeroc.icebuilder.msbuild
dotnet add src/ConsoleClient package zeroc.ice.net
```

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
    <ItemGroup>
        <SliceCompile Include="../Slice/*.ice" />
    </ItemGroup>
</Pro
```

### ConsoleClient 專案編譯

```sh
dotnet build src/ConsoleClient
```

## GuiClient 專案

### GuiClient 專案建置

- 使用 dotnet 還原 

```sh
npm install
```

- 手動新增


```sh
cd ./src/GuiClient
npm init
npm install ice --save
npm install uuid --save
npm install typescript --save-dev
npm install tslint --save-dev
npm install electron --save-dev --save-exact
npm install @types/electron --save-dev
npm install @types/uuid --save-dev
# 初始化 typescript 設定
./node_modules/.bin/tsc --init
# 初始化 tslint 設定
./node_modules/.bin/tslint --init
```

### 編譯 Slice

```sh
slice2js -I./src/Slice/ --output-dir ./src/GuiClient/Ice ./src/Slice/*.ice
```

當有使用 `#include` 指令時需要增加匯入路徑

- 方法一 適用於 `electron renderer process` 環境下

    ```js
    const path = require("path");
    require('module').globalPaths.push(path.join(process.cwd(), "Ice"));
    ```
- 方法二 適用於 `nodejs` 與 `electron renderer process` 環境下 在每一個**Ice**產生的檔案開頭加入
    ```js
    module.paths.push(__dirname);
    ```

### GuiClient 專案編譯

使用 electron-builder

```sh
cd ./src/GuiClient
npm run build
```

### GuiClient 建置與發布到github

- 設定[personal-access-token](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/)後執行

```sh
#產生的token
export GH_TOKEN=""
cd ./src/GuiClient
npm run publish
```

- 產出位置
  - windows 安裝檔於 /src/GuiClient/dist/treediagram-client 1.0.0.exe
  - linux 壓縮產出於 /src/GuiClient/dist/treediagram-client-1.0.0.7z

### GuiClient 專案執行與測試：

```sh
cd ./src/GuiClient
npm run gui
```

### ice 模組在 `electron renderer process` 執行時發生的問題

由於 `./src/GuiClient/node_modules/ice/src/Ice/Timer.js` 有[非法調用](https://stackoverflow.com/questions/9677985/uncaught-typeerror-illegal-invocation-in-chrome)的問題
所以必須修改所有bind的原生函數進行修改以相容

```js
//schedule(callback, delay) { ...
const id = Timer.setTimeout(() => this.handleTimeout(token), delay);
// scheduleRepeated(callback, period) { ...
const id = Timer.setInterval(() => this.handleInterval(token), period);
//  cancel(id) { ...
if(token.isInterval)
{
    Timer.clearInterval(token.id);
}
else
{
    Timer.clearTimeout(token.id);
}
```

變成

```js
// schedule(callback, delay) {
let id
if (typeof window === "undefined") {
    id = Timer.setTimeout(() => this.handleTimeout(token), delay);
} else {
    id = Timer.setTimeout.call(window, () => this.handleTimeout(token), delay);
}
// scheduleRepeated(callback, period) {
let id
if (typeof window === "undefined") {
    id = Timer.setInterval(() => this.handleInterval(token), period);
} else {
    id = Timer.setInterval.call(window, () => this.handleInterval(token), period);
}
//  cancel(id) { ...
if (typeof window === "undefined") {
    if (token.isInterval) {
        Timer.clearInterval(token.id);
    }
    else {
        Timer.clearTimeout(token.id);
    }
} else {
    if (token.isInterval) {
        Timer.clearInterval.call(window, token.id);
    }
    else {
        Timer.clearTimeout.call(window, token.id);
    }
}
```

---

## 已知問題：

1. `nodejs` 的 **Ice** 套件無法相容webpack

    ```sh
    Critical dependency: require function is used in a way in which dependencies cannot be statically extracted
    ```

1. **Ice**框架讓Server主動推播資料到Client?

    使用 `Glacier2`

1. 複寫呼叫**Ice**產生物件的方法是非同步的，如果裡面有用到`Neo4j.Driver.V1.ISession`的`WriteTransactionAsync`的**async**方法，會導致**Block**無回應

    使用`Task.Run`+`WriteTransaction`可以運作
    ```cs
    public override string saveData(string message, Ice.Current current)
    {
        Task<string> task = Task<string>.Run(async () =>
        {
            await Task.Delay(0);
            using (var session = _driver.Session())
            {
                var result = session.WriteTransaction((tx) =>
                {
                    var result1 = tx.Run(@"
                        CREATE (a:Greeting) SET a.message = $message 
                        RETURN a.message + ', from node ' + id(a)
                    ", new { message });
                    return (result1.Single())[0].As<string>();
                });
                return result;
            }
        });
        task.Wait();
        return task.Result;
    }
    ```

    使用`Task.Run`+`WriteTransactionAsync`可以運作
    ```cs
    public override string saveData(string message, Ice.Current current)
    {
        Task<string> task = Task<string>.Run(async () =>
        {
            await Task.Delay(0);
            using (var session = _driver.Session())
            {
                var result = await session.WriteTransactionAsync(async (tx) =>
                {
                    await Task.Delay(0);
                    var result1 = await tx.RunAsync(@"
                        CREATE (a:Greeting) SET a.message = $message 
                        RETURN a.message + ', from node ' + id(a)
                    ", new { message });
                    return (await result1.SingleAsync())[0].As<string>();
                });
                return result;
            }
        });
        task.Wait();
        return task.Result;
    }
    ```
    使用`async function`+`WriteTransaction`可以運作
    ```cs
    private async Task<string> SaveDataAsync(string message)
    {
        await Task.Delay(0);
        using (var session = _driver.Session())
        {
            var result = session.WriteTransaction((tx) =>
            {
                var result1 = tx.Run(@"
                        CREATE (a:Greeting) SET a.message = $message 
                        RETURN a.message + ', from node ' + id(a)
                    ", new { message });
                return (result1.Single())[0].As<string>();
            });
            return result;
        }
    }

    public override string saveData(string message, Ice.Current current)
    {
        var task = SaveDataAsync(message);
        task.Wait();
        return task.Result;
    }
    ```

    使用`async function`+`WriteTransactionAsync`會導致**Block**無回應
    ```cs
    private async Task<string> SaveDataAsync(string message)
    {
        await Task.Delay(0);
        using (var session = _driver.Session())
        {
            // 無回應
            var result = await session.WriteTransactionAsync(async (tx) =>
                    {
                        await Task.Delay(0);
                        var result1 = await tx.RunAsync(@"
                        CREATE (a:Greeting) SET a.message = $message 
                        RETURN a.message + ', from node ' + id(a)
                    ", new { message });
                        return (await result1.SingleAsync())[0].As<string>();
                    });
            return result;
        }
    }

    public override string saveData(string message, Ice.Current current)
    {
        var task = SaveDataAsync(message);
        task.Wait();
        return task.Result;
    }
    ```