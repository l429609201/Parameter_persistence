# GitHub Actions 自动化编译说明

本项目包含两个GitHub Actions工作流文件，用于自动编译EMBY参数持久化插件。

## 工作流文件

### 1. build.yml - 基础编译工作流

**触发条件**:
- 推送到 `main` 或 `master` 分支
- 创建以 `v` 开头的标签（如 `v1.0.0`）
- Pull Request 到 `main` 或 `master` 分支
- 手动触发

**功能**:
- 自动编译DLL文件
- 上传编译产物到Artifacts
- 创建标签时自动发布Release

### 2. ci-cd.yml - 完整CI/CD工作流（推荐）

**触发条件**:
- 推送到 `main`、`master` 或 `develop` 分支
- 创建版本标签（如 `v1.0.0`）
- Pull Request
- 手动触发（可指定版本号）

**功能**:
- 编译Debug和Release版本
- 生成版本信息文件
- 创建ZIP压缩包
- 上传编译产物（保留30天）
- 上传ZIP压缩包（保留90天）
- 自动创建GitHub Release（带详细说明）

## 使用方法

### 方式1: 推送代码自动编译

```bash
git add .
git commit -m "更新代码"
git push origin main
```

工作流会自动触发，编译完成后可在Actions页面下载编译产物。

### 方式2: 创建版本标签发布

```bash
# 创建标签
git tag -a v1.0.0 -m "Release version 1.0.0"

# 推送标签
git push origin v1.0.0
```

工作流会自动编译并创建GitHub Release，包含：
- DLL文件
- README.md
- 设计方案.md
- VERSION.txt（版本信息）
- ZIP压缩包

### 方式3: 手动触发编译

1. 进入GitHub仓库的 **Actions** 页面
2. 选择 **CI/CD Pipeline** 工作流
3. 点击 **Run workflow** 按钮
4. 可选：输入自定义版本号
5. 点击 **Run workflow** 开始编译

## 编译产物

编译完成后，可以在以下位置获取文件：

### Artifacts（所有构建）
- 进入Actions页面
- 点击具体的工作流运行记录
- 在页面底部的Artifacts区域下载

### Releases（仅标签触发）
- 进入仓库的 **Releases** 页面
- 下载对应版本的文件

## 版本号规则

- **标签版本**: `v1.0.0`、`v1.2.3` 等
- **开发版本**: `1.0.0-20260227-abc1234`（自动生成）
- **手动版本**: 手动触发时可自定义

## 注意事项

1. 首次使用需要确保GitHub仓库已启用Actions
2. 创建Release需要有仓库的写入权限
3. 编译需要.NET SDK 6.0或更高版本
4. 确保项目文件 `Emby.ParameterPersistence.csproj` 存在

## 故障排查

### 编译失败
- 检查项目文件是否正确
- 查看Actions日志中的错误信息
- 确认依赖包版本是否兼容

### Release创建失败
- 确认标签格式正确（以 `v` 开头）
- 检查是否有足够的权限
- 查看GITHUB_TOKEN是否有效

## 自定义配置

如需修改工作流配置，编辑以下文件：
- `.github/workflows/build.yml` - 基础工作流
- `.github/workflows/ci-cd.yml` - 完整工作流

常见修改项：
- 修改触发分支
- 调整.NET版本
- 更改编译配置
- 自定义Release说明

