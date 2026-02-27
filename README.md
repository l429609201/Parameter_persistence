# EMBY 参数持久化插件

一个为EMBY提供参数持久化API的插件，支持将参数存储在本地JSON文件中，提供统一的RESTful接口。

## 功能特性

- ✅ **5个统一设计的API接口** - 查询、创建、更新、删除、列表
- ✅ **自动识别单个/批量操作** - 通过请求体结构自动判断
- ✅ **命名空间分类管理** - 支持按命名空间组织参数
- ✅ **Web可视化管理界面** - 提供友好的管理界面
- ✅ **Token安全验证** - 仅管理员可访问
- ✅ **导入导出功能** - 支持JSON格式导入导出

## 安装方法

1. 下载插件DLL文件
2. 将DLL文件放入EMBY插件目录：`{EMBY安装目录}/plugins/`
3. 重启EMBY服务器
4. 在EMBY管理后台的"插件"页面中找到"参数持久化"

## 存储位置

参数数据存储在以下位置：

```
{EMBY配置目录}/config/plugins/configurations/ParameterPersistence/parameters.json
```

**示例路径**:
- Windows: `C:\ProgramData\Emby-Server\config\plugins\configurations\ParameterPersistence\parameters.json`
- Linux: `/var/lib/emby/config/plugins/configurations/ParameterPersistence/parameters.json`
- Docker: `/config/plugins/configurations/ParameterPersistence/parameters.json`

## Token验证

所有API接口都需要携带EMBY Token进行身份验证，仅管理员可访问。

### Token获取方法

1. 登录EMBY Web界面
2. 打开浏览器开发者工具（F12）
3. 在Network标签中查看任意API请求
4. 在请求头或URL中找到 X-Emby-Token 参数值

### Token传递方式

支持两种方式传递Token：

`ash
# 方式1: 查询参数
POST /emby/ParameterPersistence/Parameters/Query?X-Emby-Token=your_token_here

# 方式2: HTTP Header
POST /emby/ParameterPersistence/Parameters/Query
Header: X-Emby-Token: your_token_here
`

## API接口列表

本插件提供5个统一设计的API接口：

| 接口名称 | 路径 | 方法 | 功能 |
|---------|------|------|------|
| Query | /Parameters/Query | POST | 查询参数（单个/批量/命名空间） |
| Create | /Parameters/Create | POST | 创建参数（单个/批量） |
| Update | /Parameters/Update | POST | 更新参数（单个/批量） |
| Delete | /Parameters/Delete | POST | 删除参数（单个/批量） |
| List | /Parameters | GET | 列出所有参数（支持筛选） |

详细的API使用说明和代码示例请参考设计方案.md文档。

## 安全性

- 所有API接口都需要有效的EMBY Token
- 仅管理员用户可以访问API和管理界面
- 文件读写使用锁机制，防止并发冲突
- 建议敏感信息加密后再存储

## 许可证

MIT License
