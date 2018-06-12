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

### 專案建置

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

### 專案編譯

```sh
dotnet build src/Server
```

### 專案發布：win-x64，win-x8，linux-x64

```sh
# 輸出目錄到 src/Server/bin/Release/netcoreapp2.1/win-x64
dotnet publish --self-contained -c Release -r win-x64 src/Server
# 輸出目錄到 src/Server/bin/Release/netcoreapp2.1/win-x86
dotnet publish --self-contained -c Release -r win-x86 src/Server
# 輸出目錄到 src/Server/bin/Release/netcoreapp2.1/linux-x64
dotnet publish --self-contained -c Release -r linux-x64 src/Server
```

### 專案執行：

```sh
# 執行 linux-x64
./src/Server/bin/Release/netcoreapp2.1/linux-x64/Server
# 執行 win-x64
./src/Server/bin/Release/netcoreapp2.1/win-x64/Server.exe
# 執行 win-x86
./src/Server/bin/Release/netcoreapp2.1/win-x86/Server.exe
```