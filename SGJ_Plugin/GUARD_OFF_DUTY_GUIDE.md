# 保安下班功能 - 快速使用指南

## 功能简介

**保安下班模块 (GuardOffDutyModule)** 是一个简单但强大的功能模块，当设施安保（Facility Guard）玩家进行逃离时，会自动将其转换为配置指定的新角色。

## 功能特性

? **简化的配置** - 仅需2个配置项
? **灵活的角色转换** - 支持任何 RoleTypeId 角色
? **逃离进行时触发** - 在 Escaping 事件中修改 ev.NewRole
? **调试模式支持** - 详细的事件日志记录
? **高效的实现** - 最小化代码，最大化性能

## 配置说明

### 配置文件位置
```
EXILED/Configs/sgj_plugin.yml
```

### 配置格式
```yaml
sgj_plugin:
  enabled: true              # 插件全局开关
  debug: false               # 调试模式

  guard_off_duty:            # 保安下班功能
    enabled: true            # 启用保安下班功能
    escape_role: NtfPrivate  # 逃离后转换为的角色
```

### 配置字段说明

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `enabled` | bool | true | 启用/禁用保安下班功能 |
| `escape_role` | RoleTypeId | NtfPrivate | 逃离后转换为的角色类型 |

## 可用的角色类型

以下是常用的 RoleTypeId 值：

### 基地人员
```
ClassD              - D级人员
Scientist          - 科学家
FacilityGuard      - 设施安保 (原始角色)
```

### 九尾狐
```
NtfPrivate         - 九尾狐列兵 (默认)
NtfSergeant        - 九尾狐中士
NtfSpecialist      - 九尾狐专家
NtfCaptain         - 九尾狐队长
```

### 混沌分裂
```
ChaosRepressor     - 混沌控制者
ChaosMarauder      - 混沌掠夺者
ChaosConscript     - 混沌征兵
ChaosRifleman      - 步枪手
```

### 其他
```
Scp049             - SCP-049
Scp0492            - SCP-049-2
Scp106             - SCP-106
Scp173             - SCP-173
Scp939             - SCP-939
RoundSummary       - 回合总结
Spectator          - 旁观者
Ensnared           - 被困
Unverified         - 未验证
```

## 工作原理

### 事件流程

```
玩家（设施安保）逃离
    ↓
Escaping 事件触发
    ↓
检查：是否启用？ → 否 → 无操作
    ↓ 是
检查：玩家是否是设施安保？ → 否 → 无操作
    ↓ 是
修改 ev.NewRole = 配置的角色
    ↓
玩家转换为新角色并逃离
```

### 修改点

在 EXILED 事件系统中，`Escaping` 事件是在玩家逃离**进行中**时触发的，允许在此阶段修改 `ev.NewRole` 来改变逃离后的角色。

## 配置示例

### 示例1：转换为九尾狐列兵（默认）
```yaml
guard_off_duty:
  enabled: true
  escape_role: NtfPrivate
```

### 示例2：转换为混沌特种兵
```yaml
guard_off_duty:
  enabled: true
  escape_role: ChaosMarauder
```

### 示例3：转换为九尾狐队长
```yaml
guard_off_duty:
  enabled: true
  escape_role: NtfCaptain
```

### 示例4：禁用功能
```yaml
guard_off_duty:
  enabled: false
  escape_role: NtfPrivate
```

## 日志输出

### 正常模式
```
[保安下班模块] 已启用
[保安下班模块] 保安 'PlayerName' 已转换为 NtfPrivate
```

### 调试模式 (debug: true)
```
[保安下班模块] 已启用
[保安下班模块] 设施安保逃离后角色: NtfPrivate
[保安下班模块] 玩家 'PlayerName' 逃离时角色转换: FacilityGuard → NtfPrivate
[保安下班模块] =============== 保安下班事件 ===============
[保安下班模块] 玩家昵称: PlayerName
[保安下班模块] 玩家ID: 76561198123456789
[保安下班模块] 原始角色: FacilityGuard
[保安下班模块] 转换前新角色: Spectator
[保安下班模块] 转换后新角色: NtfPrivate
[保安下班模块] 处理时间: 2024-01-01 12:00:00 UTC
[保安下班模块] ==========================================
```

## 常见问题

### Q1：配置错误的角色会怎样？
**A：** 配置会被忽略，玩家会按照默认行为逃离。请检查配置文件中的角色名称是否正确。

### Q2：只有设施安保会受影响吗？
**A：** 是的。只有当前角色是设施安保（FacilityGuard）的玩家在逃离时才会被转换。

### Q3：可以设置多个不同的转换角色吗？
**A：** 当前版本只支持配置一个固定的逃离角色。所有设施安保都会转换为同一个角色。

### Q4：如何完全禁用此功能？
**A：** 在配置中将 `enabled` 设置为 `false`，或在插件配置中禁用整个插件。

### Q5：这个功能会影响游戏平衡吗？
**A：** 取决于您选择的逃离角色。建议选择与设施安保实力相近的角色。

## 代码集成

### 模块已自动注册
在 `Main.cs` 中，保安下班模块已自动注册：

```csharp
private void InitializeModules()
{
    _modules.Add(new InfiniteAmmoModule(Config));
    _modules.Add(new GuardOffDutyModule(Config));  // 已自动注册
}
```

### 手动触发（如需要）
如果需要在代码中手动检查或操作，可以访问：

```csharp
// 获取配置
var escapeRole = Config.GuardOffDutyConfig.EscapeRole;
var isEnabled = Config.GuardOffDutyConfig.IsEnabled;
```

## 技术细节

### 使用的事件
- **Escaping** - 玩家逃离进行中
  - 允许修改 `ev.NewRole`
  - 在逃离动画开始时触发

### 检查的角色
- **FacilityGuard** - 设施安保

### 配置类型
- **RoleTypeId** - EXILED 原生类型，支持 YAML 序列化

## 支持

如有问题或需要进一步定制，请查看：
- [README.md](../README.md) - 完整项目文档
- [BEST_PRACTICES.md](../BEST_PRACTICES.md) - 开发规范
- 项目代码注释和日志输出

---

**最后更新：** 2024年  
**功能版本：** 1.0.0  
**框架：** EXILED (.NET Framework 4.8.1)
