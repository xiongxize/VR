# 美术资源规范管理技术文档

## 概述

美术资源规范管理是一套用于 Unity 项目的资源规范检查和管理系统。该系统通过自动化工具帮助团队建立统一的美术资源命名规范、路径规范和质量标准，确保项目资源的一致性和可维护性。

## 功能特性

### 1. 自动资源检查
- 资源导入时自动验证命名规范
- 自动检查资源存放路径
- 实时警告不符合规范的资源

### 2. 命名规范管理
- 支持自定义各类资源的前缀
- 纹理资源前缀（默认：tex_）
- 材质资源前缀（默认：mat_）
- 预制体前缀（默认：prefab_）
- 模型资源前缀（默认：model_）
- 动画资源前缀（默认：anim_）
- 音频资源前缀（默认：audio_）

### 3. 路径规范管理
- 支持自定义各类资源的存放路径
- 纹理路径（默认：Art/Textures）
- 材质路径（默认：Art/Materials）
- 模型路径（默认：Art/Models）
- 预制体路径（默认：Art/Prefabs）
- 动画路径（默认：Art/Animations）
- 音频路径（默认：Art/Audio）

### 4. 资源限制设置
- 强制纹理尺寸为2的幂次方
- 最大纹理尺寸限制
- 最大三角面数限制
- 最大材质通道数限制

### 5. 批量操作功能
- 一键验证所有资源
- 生成详细的资源规范报告
- 自动修复命名规范问题

## 系统架构

### 核心组件

```
AssetRegulation/
├── AssetRegulationSettings.cs      # 规范配置类
├── AssetRegulationProcessor.cs     # 资源导入处理器
└── AssetRegulationWindow.cs        # 编辑器窗口界面
```

### 类关系图

```
AssetRegulationWindow
    ├── AssetRegulationSettings (配置数据)
    └── AssetRegulationProcessor (处理逻辑)
        └── AssetPostprocessor (Unity内置)
```

## 使用说明

### 1. 打开编辑器窗口

菜单路径：`Window → Asset Regulation`

### 2. 配置规范设置

在编辑器窗口中可以配置：
- **基本设置**：启用/禁用验证、显示警告、自动修复
- **命名规范**：各类资源的前缀设置
- **路径规范**：各类资源的存放路径
- **资源限制**：纹理、模型、材质的限制参数

### 3. 资源导入自动检查

当美术资源导入项目时，系统会自动：
1. 检查资源命名是否符合规范
2. 检查资源路径是否正确
3. 在 Console 窗口输出警告信息（如果启用）

### 4. 批量验证功能

点击 "验证所有资源" 按钮，系统会：
- 扫描项目中所有 Art 目录下的资源
- 检查每个资源的命名和路径规范
- 在 Console 窗口输出所有违规信息

### 5. 生成报告

点击 "生成报告" 按钮，系统会：
- 统计各类资源的数量和错误数
- 列出所有违规的资源
- 生成 `AssetRegulationReport.txt` 报告文件
- 自动打开报告所在文件夹

### 6. 修复命名

点击 "修复命名" 按钮，系统会：
- 自动为不符合命名规范的资源添加正确的前缀
- 保存所有修改
- 刷新 AssetDatabase

## 技术实现

### AssetRegulationSettings

ScriptableObject 类型，用于存储所有规范配置数据。

**主要属性**：
- `enableValidation`：是否启用验证
- `showWarnings`：是否显示警告
- `texturePrefix` / `materialPrefix` 等：各类资源前缀
- `texturesPath` / `materialsPath` 等：各类资源路径
- `maxTextureSize` / `maxTriangleCount` 等：资源限制参数

### AssetRegulationProcessor

继承自 `AssetPostprocessor`，在资源导入时自动执行检查。

**主要方法**：
- `OnPreprocessTexture()`：纹理导入前处理
- `OnPreprocessModel()`：模型导入前处理
- `OnPreprocessMaterial()`：材质导入前处理
- `ValidateAllAssets()`：批量验证所有资源
- `ValidateAssetPath()`：验证单个资源路径

### AssetRegulationWindow

编辑器窗口类，提供可视化界面。

**主要功能**：
- 显示和编辑规范配置
- 提供批量操作按钮
- 生成和显示报告

## 配置示例

### 默认配置

```csharp
// 命名前缀
texturePrefix = "tex_"
materialPrefix = "mat_"
prefabPrefix = "prefab_"
modelPrefix = "model_"
animationPrefix = "anim_"
audioPrefix = "audio_"

// 路径规范
texturesPath = "Art/Textures"
materialsPath = "Art/Materials"
modelsPath = "Art/Models"
prefabsPath = "Art/Prefabs"
animationsPath = "Art/Animations"
audioPath = "Art/Audio"

// 资源限制
enforcePowerOfTwo = true
maxTextureSize = 2048
maxTriangleCount = 10000
maxMaterialChannels = 4
```

### 自定义配置

根据项目需求，可以在编辑器窗口中修改上述配置值。

## 扩展开发

### 添加新的资源类型检查

1. 在 `AssetRegulationSettings` 中添加新的前缀和路径配置
2. 在 `AssetRegulationProcessor` 中添加对应的验证方法
3. 在 `AssetRegulationWindow` 中添加对应的 UI 控件

### 添加新的验证规则

在 `ValidateAssetPath` 方法中添加新的检查逻辑：

```csharp
else if (assetPath.EndsWith(".newExtension"))
{
    // 添加新的验证逻辑
}
```

## 注意事项

1. **配置文件位置**：`Assets/Editor/AssetRegulation/AssetRegulationSettings.asset`
2. **报告文件位置**：`Assets/AssetRegulationReport.txt`
3. **自动修复功能**：使用前建议先备份项目
4. **性能考虑**：批量验证大量资源时可能需要较长时间

## 常见问题

### Q: 如何禁用自动验证？
A: 在编辑器窗口中取消勾选 "启用验证" 选项。

### Q: 如何修改资源前缀？
A: 在编辑器窗口的 "命名规范" 部分修改对应的前缀设置。

### Q: 验证报告在哪里查看？
A: 点击 "生成报告" 后，报告会自动保存到 `Assets/AssetRegulationReport.txt`，并自动打开所在文件夹。

### Q: 自动修复会修改哪些内容？
A: 自动修复只会修改资源的命名（添加正确的前缀），不会修改资源的内容或其他属性。

## 版本历史

- **v1.0** (2026-04-20)
  - 初始版本发布
  - 实现基本的命名和路径规范检查
  - 实现编辑器窗口界面
  - 实现批量验证和报告生成功能
