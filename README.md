# treediagram

一個可讓客端透過API編輯樹狀圖(tree diagram)的網路服務

## Neo4j 資料庫安裝

1. 使用 **docker** 安裝資料儲存目錄設定為`/home/cefc/Data/neo4j`
    ```sh
    sudo docker run --name neo4j -p=7474:7474 -p=7687:7687 -v=/home/cefc/Data/neo4j:/data -d neo4j
    ```
1. 使用瀏覽器登入 http://localhost:7474
1. 預設帳密 **neo4j**/**neo4j**，變更密碼為：**12369874**
