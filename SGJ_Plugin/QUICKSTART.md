# 快速开始指南 - SGJ_Plugin

## 5分钟快速设置

### 1. 编译项目

```bash
# 在 Visual Studio 中
Ctrl + Shift + B  # 编译解决方案
```

### 2. 放置插件文件

编译后的 DLL 放到 EXILED 插件目录：
```
ServerPath/EXILED/Plugins/SGJ_Plugin.dll
```

### 3. 配置文件

创建或修改配置文件：
```
ServerPath/EXILED/Configs/SGJ_Plugin.yml
```

### 4. 最小配置示例

```yaml
sgj_plugin:
  enabled: true
  debug: false
  infinite_ammo:
    enabled: true
    restore_on_reload: true
    infinite_reserve_ammo: true
    ignore_players: []
    allowed_guns: []
    excluded_guns: []
```

### 5. 重启服务器

配置完成后重启服务器，功能即可生效。

---

## 常用配置场景

### 场景1：启用所有枪械的无限子弹

```yaml
infinite_ammo:
  enabled: true
  restore_on_reload: true
  infinite_reserve_ammo: true
```

### 场景2：仅E11步枪无限子弹

```yaml
infinite_ammo:
  enabled: true
  allowed_guns:
    - E11SR
```

### 场景3：除Deagle外的所有枪械

```yaml
infinite_ammo:
  enabled: true
  excluded_guns:
    - Deagle
```

### 场景4：特定玩家禁用此功能

```yaml
infinite_ammo:
  enabled: true
  ignore_players:
    - "76561198123456789"
```

---

## 调试模式

启用调试模式查看详细日志：

```yaml
debug: true  # 启用调试模式
```

### 预期日志输出

```
[枪械无限子弹模块] 已启用 - 支持 14 种枪械类型
[枪械无限子弹模块] 排除的枪械: 无
[枪械无限子弹模块] 允许的枪械: 所有枪械
[枪械无限子弹模块] 玩家 'PlayerName' 装弹: 枪械=E11SR
```

---

## 模块添加新功能

### 添加自定义模块（如杀敌连击检测）

1. **创建新模块文件**

```csharp
// SGJ_Plugin\Modules\CustomModule.cs
public class CustomModule : ModuleBase
{
    public override string Name => "自定义模块";

    protected override void OnEnable()
    {
        // 初始化
    }

    protected override void OnDisable()
    {
        // 清理
    }
}
```

2. **在 Main.cs 中注册**

```csharp
private void InitializeModules()
{
    _modules.Add(new InfiniteAmmoModule(Config));
    _modules.Add(new CustomModule(Config));  // 新增
}
```

3. **编译并重启**

---

## 常见错误排查

| 错误 | 原因 | 解决方案 |
|------|------|---------|
| 插件未加载 | DLL 路径错误 | 检查 EXILED/Plugins 目录 |
| 功能无效 | 配置未启用 | 检查 YAML 中 `enabled: true` |
| 性能下降 | 频繁的事件处理 | 查看调试日志中是否有错误 |
| 配置无法读取 | YAML 格式错误 | 使用 YAML 验证器检查 |

---

## 文件结构说明

```
SGJ_Plugin/
├── Main.cs                      # 插件主类
├── Config.cs                    # 配置定义
├── README.md                    # 详细文档
├── BEST_PRACTICES.md           # 最佳实践
├── GUN_INFINITE_AMMO_GUIDE.md  # 枪械无限子弹指南
├── QUICKSTART.md               # 本文件
└── Modules/
    ├── ModuleBase.cs           # 基类
    ├── InfiniteAmmoModule.cs   # 无限子弹模块
    ├── ExampleModule.cs        # 示例模块
    └── AdvancedExampleModule.cs # 高级示例
```

---

## 项目属性

- **名称：** SGJ_Plugin
- **作者：** WJ
- **版本：** 1.0.0
- **框架：** .NET Framework 4.8.1
- **依赖：** EXILED 框架

---

## 后续步骤

1. ? 阅读 [README.md](README.md) - 完整项目文档
2. ? 查看 [GUN_INFINITE_AMMO_GUIDE.md](GUN_INFINITE_AMMO_GUIDE.md) - 功能指南
3. ? 学习 [BEST_PRACTICES.md](BEST_PRACTICES.md) - 最佳实践
4. ? 自定义模块实现自己的功能

---

**需要帮助？** 检查日志文件或启用调试模式。
