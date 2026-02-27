 # EMBY 参数持久化插件

 为 EMBY 提供通用的键值对参数持久化存储 API，数据以 JSON 文件保存在本地，供其他插件或外部程序调用。

 ## 功能特性

 - 4 个 RESTful API — 查询(GET)、创建(POST)、更新(POST)、删除(POST)
 - 支持单个和批量操作 — 通过请求体结构自动判断
 - 命名空间管理 — 不同插件使用不同命名空间，互不干扰
 - Upsert 模式 — Create 接口参数已存在时自动更新
 - Web 管理界面 — 在 EMBY 后台可视化管理所有参数
 - 导入导出 — 支持 JSON 格式导入导出

 ## 安装

 1. 从 [Releases](https://github.com/l429609201/Parameter_persistence/releases) 下载 `Emby.ParameterPersistence.dll`
 2. 放入 EMBY 插件目录 `{EMBY安装目录}/plugins/`
 3. 重启 EMBY 服务器
 4. 在管理后台 - 插件 中找到「参数持久化」

 ## 存储位置

 ```
 {EMBY配置目录}/plugins/configurations/ParameterPersistence/parameters.json
 ```

 ## Token 验证

 所有 API 接口都需要携带 EMBY Token 进行身份验证，支持两种方式：

 ```
 # 方式1：URL 查询参数
 GET /emby/ParameterPersistence/Query?api_key=your_token

 # 方式2：HTTP Header
 X-Emby-Token: your_token
 ```

 ---

 ## API 接口总览

 > **重要**：EMBY 的 JSON 序列化器返回的字段名均为 **PascalCase** 格式（如 `Success`、`DataList`、`Key`、`Value`），**不是** camelCase。

 | 接口 | 方法 | 路径 | 说明 |
 |------|------|------|------|
 | 查询 | GET | `/emby/ParameterPersistence/Query` | 通过 query string 传参 |
 | 创建 | POST | `/emby/ParameterPersistence/Create` | 支持 upsert，已存在自动更新 |
 | 更新 | POST | `/emby/ParameterPersistence/Update` | 更新已有参数的值 |
 | 删除 | POST | `/emby/ParameterPersistence/Delete` | 删除指定参数 |

 ---

 ## 数据模型

 ### 参数对象 (ParameterModel)

 | 字段 | 类型 | 说明 |
 |------|------|------|
 | `Id` | string | 参数唯一标识（自动生成） |
 | `Namespace` | string | 命名空间，用于分类管理（默认 `default`） |
 | `Key` | string | 参数键名 |
 | `Value` | string | 参数值 |
 | `Type` | string | 参数类型：`string` / `number` / `boolean` / `json` |
 | `Description` | string | 参数描述（可选） |
 | `CreatedAt` | datetime | 创建时间 |
 | `UpdatedAt` | datetime | 最后更新时间 |

 ### 响应对象 (ParameterResponse)

 | 字段 | 类型 | 说明 |
 |------|------|------|
 | `Success` | bool | 是否成功 |
 | `Message` | string | 提示信息 |
 | `Data` | ParameterModel | 单个查询/创建/更新时返回 |
 | `DataList` | ParameterModel[] | 列表查询时返回 |
 | `Total` | int | 列表结果总数 |

 ---

 ## 1. 查询参数

 ```
 GET /emby/ParameterPersistence/Query
 ```

 通过 URL query string 传递查询条件，支持以下组合：

 | 参数组合 | 说明 |
 |---------|------|
 | 无参数 | 返回所有参数 |
 | `?Namespace=xx` | 返回指定命名空间下所有参数 |
 | `?Namespace=xx&Key=xx` | 返回单个参数 |

### curl 示例

```bash
# 查询全部参数
curl "http://your-emby:8096/emby/ParameterPersistence/Query?api_key=YOUR_TOKEN"

# 查询指定命名空间
curl "http://your-emby:8096/emby/ParameterPersistence/Query?Namespace=dd-danmaku&api_key=YOUR_TOKEN"

# 查询单个参数
curl "http://your-emby:8096/emby/ParameterPersistence/Query?Namespace=dd-danmaku&Key=danmakuSwitch&api_key=YOUR_TOKEN"

# 关键词搜索
curl "http://your-emby:8096/emby/ParameterPersistence/Query?Keyword=danmaku&api_key=YOUR_TOKEN"
```

### JavaScript 示例（EMBY 插件内部调用）

```javascript
// EMBY 插件内使用 ApiClient.ajax
ApiClient.ajax({
    type: 'GET',
    url: ApiClient.getUrl('/ParameterPersistence/Query', {
        Namespace: 'dd-danmaku'
    }),
    dataType: 'json'
}).then(function (response) {
    if (response.Success) {
        response.DataList.forEach(function (param) {
            console.log(param.Key + ' = ' + param.Value);
        });
    }
});
```

### JavaScript 示例（外部 fetch 调用）

```javascript
const response = await fetch(
    'http://your-emby:8096/emby/ParameterPersistence/Query?Namespace=dd-danmaku&api_key=YOUR_TOKEN'
);
const result = await response.json();
if (result.Success) {
    result.DataList.forEach(param => {
        console.log(param.Key, param.Value);
    });
}
```

### 响应示例

**列表查询**（无参数 / Namespace / Keyword）：
```json
{
    "Success": true,
    "DataList": [
        {
            "Id": "a1b2c3d4-...",
            "Namespace": "dd-danmaku",
            "Key": "danmakuSwitch",
            "Value": "1",
            "Type": "string",
            "Description": "弹幕开关",
            "CreatedAt": "2026-02-27T22:59:21Z",
            "UpdatedAt": "2026-02-27T22:59:21Z"
        }
    ],
    "Total": 1,
    "Message": null
}
```

**单个查询**（Namespace + Key）：
```json
{
    "Success": true,
    "Data": {
        "Id": "a1b2c3d4-...",
        "Namespace": "dd-danmaku",
        "Key": "danmakuSwitch",
        "Value": "1",
        "Type": "string"
    },
    "Message": null
}
```

**查询失败**：
```json
{
    "Success": false,
    "Message": "参数不存在"
}
```

---

## 2. 创建参数

```
POST /emby/ParameterPersistence/Create
Content-Type: application/json
```

支持 **upsert 模式**：如果 Namespace + Key 已存在，自动更新值，不会报错。

### 请求体字段

**单个创建**：直接传字段

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `Namespace` | string | 否 | 命名空间（默认 `default`） |
| `Key` | string | 是 | 参数键名 |
| `Value` | string | 否 | 参数值（默认空字符串） |
| `Type` | string | 否 | 参数类型（默认 `string`） |
| `Description` | string | 否 | 参数描述 |

**批量创建**：传 `Parameters` 数组

| 字段 | 类型 | 说明 |
|------|------|------|
| `Parameters` | array | 每个元素包含 Namespace、Key、Value、Type、Description |

### curl 示例

```bash
# 单个创建
curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Create?api_key=YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Namespace":"my-plugin","Key":"theme","Value":"dark","Type":"string","Description":"主题设置"}'

# 批量创建
curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Create?api_key=YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Parameters":[{"Namespace":"my-plugin","Key":"theme","Value":"dark"},{"Namespace":"my-plugin","Key":"fontSize","Value":"16"}]}'
```

### JavaScript 示例

```javascript
// 单个创建（EMBY 插件内部）
ApiClient.ajax({
    type: 'POST',
    url: ApiClient.getUrl('/ParameterPersistence/Create'),
    data: JSON.stringify({
        Namespace: 'my-plugin',
        Key: 'theme',
        Value: 'dark',
        Type: 'string'
    }),
    contentType: 'application/json',
    dataType: 'json'
}).then(function (r) {
    if (r.Success) console.log('创建成功', r.Data);
});

// 批量创建
ApiClient.ajax({
    type: 'POST',
    url: ApiClient.getUrl('/ParameterPersistence/Create'),
    data: JSON.stringify({
        Parameters: [
            { Namespace: 'my-plugin', Key: 'a', Value: '1', Type: 'string' },
            { Namespace: 'my-plugin', Key: 'b', Value: '2', Type: 'number' }
        ]
    }),
    contentType: 'application/json',
    dataType: 'json'
}).then(function (r) {
    console.log('成功:', r.Total, '个');
});
```

### 响应示例

**单个创建成功**：
```json
{
    "Success": true,
    "Data": {
        "Id": "a1b2c3d4-...",
        "Namespace": "my-plugin",
        "Key": "theme",
        "Value": "dark",
        "Type": "string",
        "Description": "主题设置",
        "CreatedAt": "2026-02-28T00:00:00Z",
        "UpdatedAt": "2026-02-28T00:00:00Z"
    },
    "Message": "参数创建成功"
}
```

**批量创建成功**：
```json
{
    "Success": true,
    "DataList": [ ... ],
    "Total": 2,
    "Message": "成功创建 2 个参数"
}
```

---

## 3. 更新参数

```
POST /emby/ParameterPersistence/Update
Content-Type: application/json
```

### 请求体字段

**单个更新**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `Namespace` | string | 否 | 命名空间（默认 `default`） |
| `Key` | string | 是 | 要更新的参数键名 |
| `Value` | string | 否 | 新的参数值 |
| `Description` | string | 否 | 新的描述 |

**批量更新**：传 `Parameters` 数组，每个元素包含 Namespace、Key、Value、Description。

### curl 示例

```bash
# 单个更新
curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Update?api_key=YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Namespace":"my-plugin","Key":"theme","Value":"light"}'

# 批量更新
curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Update?api_key=YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Parameters":[{"Namespace":"my-plugin","Key":"theme","Value":"light"},{"Namespace":"my-plugin","Key":"fontSize","Value":"18"}]}'
```

### JavaScript 示例

```javascript
ApiClient.ajax({
    type: 'POST',
    url: ApiClient.getUrl('/ParameterPersistence/Update'),
    data: JSON.stringify({
        Namespace: 'my-plugin',
        Key: 'theme',
        Value: 'light'
    }),
    contentType: 'application/json',
    dataType: 'json'
}).then(function (r) {
    if (r.Success) console.log('更新成功', r.Data);
});
```

### 响应示例

```json
{
    "Success": true,
    "Data": {
        "Id": "a1b2c3d4-...",
        "Namespace": "my-plugin",
        "Key": "theme",
        "Value": "light",
        "Type": "string",
        "UpdatedAt": "2026-02-28T01:00:00Z"
    },
    "Message": "参数更新成功"
}
```

---

## 4. 删除参数

```
POST /emby/ParameterPersistence/Delete
Content-Type: application/json
```

支持三种删除方式：

### 请求体字段

**方式1 — 单个删除**：

| 字段 | 类型 | 说明 |
|------|------|------|
| `Namespace` | string | 命名空间（默认 `default`） |
| `Key` | string | 要删除的参数键名 |

**方式2 — 批量删除（同一命名空间）**：

| 字段 | 类型 | 说明 |
|------|------|------|
| `Namespace` | string | 命名空间 |
| `Keys` | string[] | 要删除的键名数组 |

**方式3 — 批量删除（不同命名空间）**：

| 字段 | 类型 | 说明 |
|------|------|------|
| `Parameters` | array | 每个元素包含 Namespace 和 Key |

### curl 示例

```bash
# 单个删除
curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Delete?api_key=YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Namespace":"my-plugin","Key":"theme"}'

# 批量删除（同一命名空间下多个 Key）
curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Delete?api_key=YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Namespace":"my-plugin","Keys":["theme","fontSize","autoPlay"]}'

# 批量删除（不同命名空间）
curl -X POST "http://your-emby:8096/emby/ParameterPersistence/Delete?api_key=YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Parameters":[{"Namespace":"plugin-a","Key":"key1"},{"Namespace":"plugin-b","Key":"key2"}]}'
```

### JavaScript 示例

```javascript
ApiClient.ajax({
    type: 'POST',
    url: ApiClient.getUrl('/ParameterPersistence/Delete'),
    data: JSON.stringify({
        Namespace: 'my-plugin',
        Key: 'theme'
    }),
    contentType: 'application/json',
    dataType: 'json'
}).then(function (r) {
    if (r.Success) console.log('删除成功');
});
```

### 响应示例

```json
{
    "Success": true,
    "Message": "参数删除成功"
}
```

---

## 许可证

MIT License