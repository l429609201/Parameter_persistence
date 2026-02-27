# 版本管理说明

本项目使用 `version.json` 文件进行版本管理，推送到main分支时会自动创建标签并发布Release。

## version.json 文件格式

```json
{
  "version": "1.0.0",
  "name": "EMBY 参数持久化插件",
  "description": "版本简短描述",
  "changelog": [
    "✨ 新功能说明",
    "🐛 Bug修复说明",
    "📝 文档更新说明"
  ],
  "breaking_changes": [
    "⚠️ 破坏性变更说明（如果有）"
  ],
  "notes": "版本备注信息"
}
```

## 字段说明

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| version | string | ✅ | 版本号，格式：`主版本.次版本.修订号` |
| name | string | ✅ | 插件名称 |
| description | string | ✅ | 版本简短描述 |
| changelog | array | ✅ | 更新日志列表 |
| breaking_changes | array | ❌ | 破坏性变更列表（可为空数组） |
| notes | string | ❌ | 版本备注信息 |

## 版本号规范

遵循语义化版本规范（Semantic Versioning）：

- **主版本号（Major）**: 不兼容的API修改
- **次版本号（Minor）**: 向下兼容的功能性新增
- **修订号（Patch）**: 向下兼容的问题修正

示例：
- `1.0.0` - 首次正式发布
- `1.1.0` - 新增功能
- `1.1.1` - Bug修复
- `2.0.0` - 重大更新（不兼容旧版本）

## 更新日志图标建议

使用Emoji让更新日志更直观：

- ✨ `:sparkles:` - 新功能
- 🐛 `:bug:` - Bug修复
- 📝 `:memo:` - 文档更新
- 🔧 `:wrench:` - 配置修改
- ♻️ `:recycle:` - 代码重构
- 🎨 `:art:` - 改进代码结构/格式
- ⚡ `:zap:` - 性能优化
- 🔒 `:lock:` - 安全性修复
- 🚀 `:rocket:` - 部署相关
- 🔥 `:fire:` - 移除代码或文件

## 发布流程

### 1. 修改 version.json

编辑 `version.json` 文件，更新版本信息：

```json
{
  "version": "1.1.0",
  "name": "EMBY 参数持久化插件",
  "description": "新增批量操作优化功能",
  "changelog": [
    "✨ 新增批量操作性能优化",
    "🐛 修复命名空间查询bug",
    "📝 更新API使用文档"
  ],
  "breaking_changes": [],
  "notes": "本版本优化了批量操作的性能，建议所有用户升级。"
}
```

### 2. 提交并推送到main分支

```bash
# 添加修改
git add version.json
git add .  # 添加其他修改的文件

# 提交
git commit -m "chore: 发布 v1.1.0 版本"

# 推送到main分支
git push origin main
```

### 3. 自动化流程

推送后，GitHub Actions会自动执行以下操作：

1. ✅ 读取 `version.json` 文件
2. ✅ 检查标签 `v1.1.0` 是否已存在
3. ✅ 如果不存在，创建并推送标签
4. ✅ 编译DLL文件
5. ✅ 创建ZIP压缩包
6. ✅ 生成版本信息文件
7. ✅ 根据 `version.json` 生成Release说明
8. ✅ 创建GitHub Release并上传文件

### 4. 查看发布结果

- 访问仓库的 **Releases** 页面查看新发布的版本
- 在 **Actions** 页面查看构建日志

## 示例场景

### 场景1: 发布新功能版本

```json
{
  "version": "1.2.0",
  "name": "EMBY 参数持久化插件",
  "description": "新增导出导入功能",
  "changelog": [
    "✨ 新增参数导出为JSON功能",
    "✨ 新增从JSON导入参数功能",
    "🎨 优化Web管理界面布局",
    "📝 更新使用文档"
  ],
  "breaking_changes": [],
  "notes": "本版本新增了导出导入功能，方便参数备份和迁移。"
}
```

### 场景2: 发布Bug修复版本

```json
{
  "version": "1.2.1",
  "name": "EMBY 参数持久化插件",
  "description": "修复已知问题",
  "changelog": [
    "🐛 修复批量删除时的空指针异常",
    "🐛 修复命名空间筛选不生效的问题",
    "🔒 增强Token验证安全性"
  ],
  "breaking_changes": [],
  "notes": "本版本修复了几个重要bug，建议尽快升级。"
}
```

### 场景3: 发布重大更新版本

```json
{
  "version": "2.0.0",
  "name": "EMBY 参数持久化插件",
  "description": "API接口重构",
  "changelog": [
    "♻️ 重构API接口，采用统一设计模式",
    "✨ 接口数量从10个精简为5个",
    "✨ 支持自动识别单个/批量操作",
    "📝 更新所有文档"
  ],
  "breaking_changes": [
    "⚠️ 删除了旧的简化接口（Set, Get, GetMultiple, SetMultiple）",
    "⚠️ 所有操作接口改为POST方法",
    "⚠️ 不再支持路径参数方式"
  ],
  "notes": "本版本为重大更新，API接口不兼容旧版本，升级前请查看迁移指南。"
}
```

## 注意事项

1. **版本号不能重复**: 如果标签已存在，工作流会跳过发布
2. **必须推送到main分支**: 只有main分支的推送会触发自动发布
3. **确保代码已编译通过**: 推送前建议本地编译测试
4. **changelog不能为空**: 至少要有一条更新说明
5. **版本号格式要正确**: 必须是 `x.y.z` 格式

## 手动触发

如果需要手动触发发布流程：

1. 进入GitHub仓库的 **Actions** 页面
2. 选择 **Auto Release from Version File** 工作流
3. 点击 **Run workflow** 按钮
4. 选择分支（通常是main）
5. 点击 **Run workflow** 开始执行

## 回滚版本

如果需要回滚到旧版本：

1. 修改 `version.json` 为旧版本号（如 `1.0.0`）
2. 在changelog中说明这是回滚版本
3. 推送到main分支
4. 系统会创建新的Release

## 常见问题

### Q: 推送后没有自动创建Release？
A: 检查以下几点：
- 是否推送到了main分支
- version.json格式是否正确
- 该版本的标签是否已存在
- 查看Actions页面的构建日志

### Q: 如何删除错误的Release？
A: 
1. 在Releases页面删除对应的Release
2. 删除对应的Git标签：`git push origin :refs/tags/v1.0.0`
3. 修改version.json后重新推送

### Q: 可以跳过某些版本号吗？
A: 可以，版本号不需要连续，但建议遵循语义化版本规范。

## 相关文件

- `version.json` - 版本配置文件
- `.github/workflows/ci-cd.yml` - 自动化工作流
- `.github/README.md` - GitHub Actions使用说明

