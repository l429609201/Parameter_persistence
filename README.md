 # EMBY 参数持久化插件
 
 为 EMBY 提供通用的键值对参数持久化存储 API，数据以 JSON 文件保存在本地，供其他插件或外部程序调用。
 
 ## 功能特性
 
 - ✅ **4 个 RESTful API** — 查询(GET)、创建(POST)、更新(POST)、删除(POST)
 - ✅ **支持单个和批量操作** — 通过请求体结构自动判断
 - ✅ **命名空间管理** — 不同插件使用不同命名空间，互不干扰
 - ✅ **Upsert 模式** — Create 接口参数已存在时自动更新
 - ✅ **Web 管理界面** — 在 EMBY 后台可视化管理所有参数
 - ✅ **导入导出** — 支持 JSON 格式导入导出
 
 ## 安装
 
 1. 从 [Releases](https://github.com/l429609201/Parameter_persistence/releases) 下载 `Emby.ParameterPersistence.dll`
 2. 放入 EMBY 插件目录：`{EMBY安装目录}/plugins/`
 3. 重启 EMBY 服务器
 4. 在管理后台 → 插件 → 找到「参数持久化」
 
 ## 存储位置
 
 ```
 {EMBY配置目录}/plugins/configurations/ParameterPersistence/parameters.json
 ```
 
 - Windows: `C:\ProgramData\Emby-Server\config\plugins\configurations\ParameterPersistence\parameters.json`
 - Linux: `/var/lib/emby/config/plugins/configurations/ParameterPersistence/parameters.json`
 - Docker: `/config/plugins/configurations/ParameterPersistence/parameters.json`
 
 ## API 接口
 
 所有接口需携带 EMBY Token（通过 `?api_key=xxx` 或 Header `X-Emby-Token`）。
 
 > **注意**：响应字段均为 **PascalCase**（如 `Success`、`DataList`、`Key`、`Value`）。
 
 | 接口 | 方法 | 路径 | 说明 |
 |------|------|------|------|
 | 查询 | GET | `/emby/ParameterPersistence/Query` | 支持 query string 参数 |
 | 创建 | POST | `/emby/ParameterPersistence/Create` | 支持 upsert，已存在自动更新 |
 | 更新 | POST | `/emby/ParameterPersistence/Update` | 支持单个和批量 |
 | 删除 | POST | `/emby/ParameterPersistence/Delete` | 支持单个和批量 |
 
 ---
 
 ### 1. 查询参数 — `GET /emby/ParameterPersistence/Query`
 
 通过 query string 传参，支持以下组合：
 
 | 参数 | 说明 |
 |------|------|
 | 无参数 | 返回所有参数 |
 | `?Namespace=xx` | 返回指定命名空间下所有参数 |
 | `?Namespace=xx&Key=xx` | 返回单个参数 |
 | `?Keyword=xx` | 搜索关键词（匹配 Key、Value、Description） |
 
 **示例：**
 
 ```bash
 # 查询全部
 curl "http://your-emby:8096/emby/ParameterPersistence/Query?api_key=YOUR_TOKEN"
 
 # 按命名空间查询
 curl "http://your-emby:8096/emby/ParameterPersistence/Query?Namespace=dd-danmaku&api_key=YOUR_TOKEN"
 
 # 查询单个参数
 curl "http://your-emby:8096/emby/ParameterPersistence/Query?Namespace=dd-danmaku&Key=danmakuSwitch&api_key=YOUR_TOKEN"
 ```
 
 **响应示例（列表）：**
 ```json
 {
   "Success": true,
   "DataList": [
     {
       "Namespace": "dd-danmaku",
       "Key": "danmakuSwitch",
       "Value": "1",
       "Type": "string",
       "Description": "弹幕开关"
     }
   ],
   "Total": 1
 }
 ```
 
 **响应示例（单个）：**
 ```json
 {
   "Success": true,
   "Data": {
     "Namespace": "dd-danmaku",
     "Key": "danmakuSwitch",
     "Value": "1",
     "Type": "string"
   }
 }
 ```
 
 ---
 
 ### 2. 创建参数 — `POST /emby/ParameterPersistence/Create`
 
 支持 upsert 模式：参数已存在时自动更新。
 
 **单个创建：**
 ```bash
 curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Create?api_key=YOUR_TOKEN" \
   -H "Content-Type: application/json" \
   -d '{"Namespace":"my-plugin","Key":"setting1","Value":"hello","Type":"string"}'
 ```
 
 **批量创建：**
 ```bash
 curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Create?api_key=YOUR_TOKEN" \
   -H "Content-Type: application/json" \
   -d '{"Parameters":[{"Namespace":"my-plugin","Key":"a","Value":"1"},{"Namespace":"my-plugin","Key":"b","Value":"2"}]}'
 ```
 
 ---
 
 ### 3. 更新参数 — `POST /emby/ParameterPersistence/Update`
 
 **单个更新：**
 ```bash
 curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Update?api_key=YOUR_TOKEN" \
   -H "Content-Type: application/json" \
   -d '{"Namespace":"my-plugin","Key":"setting1","Value":"new-value"}'
 ```
 
 **批量更新：**
 ```bash
 curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Update?api_key=YOUR_TOKEN" \
   -H "Content-Type: application/json" \
   -d '{"Parameters":[{"Namespace":"my-plugin","Key":"a","Value":"100"},{"Namespace":"my-plugin","Key":"b","Value":"200"}]}'
 ```
 
 ---
 
 ### 4. 删除参数 — `POST /emby/ParameterPersistence/Delete`
 
 **单个删除：**
 ```bash
 curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Delete?api_key=YOUR_TOKEN" \
   -H "Content-Type: application/json" \
   -d '{"Namespace":"my-plugin","Key":"setting1"}'
 ```
 
 **批量删除（同一命名空间）：**
 ```bash
 curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Delete?api_key=YOUR_TOKEN" \
   -H "Content-Type: application/json" \
   -d '{"Namespace":"my-plugin","Keys":["a","b","c"]}'
 ```
 
 ## 许可证
 
 MIT License