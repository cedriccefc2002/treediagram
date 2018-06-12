# treediagram

一個可讓客端透過API編輯樹狀圖(tree diagram)的網路服務

## Neo4j 資料庫安裝

1. 使用 **docker** 安裝資料儲存目錄設定為`/home/cefc/Data/neo4j`
    ```sh
    sudo docker run --name neo4j -p=7474:7474 -p=7687:7687 -v=/home/cefc/Data/neo4j:/data -d neo4j
    ```
1. 使用瀏覽器登入 http://localhost:7474
1. 預設帳密 **neo4j**/**neo4j**，變更密碼為：**12369874**

## 安裝 [Slice Compiler](https://zeroc.com/downloads/ice#dotnet-core)

```sh
sudo apt-key adv --keyserver keyserver.ubuntu.com --recv B6391CB2CFBA643D
sudo apt-add-repository "deb http://zeroc.com/download/Ice/3.7/ubuntu18.04 stable main"
sudo apt-get update
sudo apt-get install zeroc-ice-compilers
```


## Server 專案

### Server 專案建置

```sh
dotnet new console -o src/Server
dotnet add src/Server package zeroc.icebuilder.msbuild
dotnet add src/Server package zeroc.ice.net
dotnet add src/Server package Neo4j.Driver
```

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
    <ItemGroup>
        <SliceCompile Include="../Slice/*.ice" />
    </ItemGroup>
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
./src/Server/bin/Release/netcoreapp2.1/linux-x64/Server
# 執行 win-x64
./src/Server/bin/Release/netcoreapp2.1/win-x64/Server.exe
# 執行 win-x86
./src/Server/bin/Release/netcoreapp2.1/win-x86/Server.exe
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

```sh
cd ./src/GuiClient
npm init
npm install ice --save
npm install typescript --save-dev
npm install tslint --save-dev
npm install electron --save-dev --save-exact
npm install @types/electron --save-dev
# 初始化 typescript 設定
./node_modules/.bin/tsc --init
# 初始化 tslint 設定
./node_modules/.bin/tslint --init
```

### 編譯 Slice

```sh
slice2js -I./src/Slice/ --output-dir ./src/GuiClient/Ice ./src/GuiClient/Node.ice
```

### GuiClient 專案編譯

```sh
cd ./src/GuiClient
npm run build
```

### GuiClient 專案執行：

```sh
cd ./src/GuiClient
npm run gui
```

### ice 模組在 `electron renderer process` 執行時發生的問題

由於 `./node_modules/ice/src/Ice/Timer.js` 有[非法調用](https://stackoverflow.com/questions/9677985/uncaught-typeerror-illegal-invocation-in-chrome)的問題
所以必須修改所有bind的原生函數進行修改

```js
const id = Timer.setTimeout(() => this.handleTimeout(token), delay);

const id = Timer.setInterval(() => this.handleInterval(token), period);

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
const id = Timer.setTimeout.call(window,() => this.handleTimeout(token), delay);

const id = Timer.setInterval.call(window,() => this.handleInterval(token), period);

if(token.isInterval)
{
    Timer.clearInterval.call(window, token.id);
}
else
{
    Timer.clearTimeout.call(window, token.id);
}
```