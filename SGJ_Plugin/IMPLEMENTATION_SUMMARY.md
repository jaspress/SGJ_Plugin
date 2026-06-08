# 保安下班功能 - 实现总结

## ? 已完成的修改

### 1. 配置文件简化 (`Config.cs`)
- ? **移除了忽略列表** - `IgnorePlayers` 字段已删除
- ? **移除了广播消息设置** - 保留仅关键配置
- ? **移除了日志控制开关** - 使用全局 Debug 模式
- ? **添加 RoleTypeId 直接支持** - 配置直接使用 `RoleTypeId` 枚举，YAML 可序列化

**配置项现在只有2个：**
```csharp
public class GuardOffDutyConfigClass
{
    public bool IsEnabled { get; set; } = true;              // 启用开关
    public RoleTypeId EscapeRole { get; set; } = RoleTypeId.NtfPrivate;  // 逃离角色
}
```

### 2. 模块实现优化 (`GuardOffDutyModule.cs`)
- ? **使用 Escaping 事件** - 在 `EscapingEventArgs` 中修改 `ev.NewRole`
- ? **直接角色比较** - 使用 `RoleTypeId` 直接对比，无需字符串解析
- ? **移除忽略列表逻辑** - 所有设施安保都会被处理
- ? **简化日志输出** - 根据 Debug 模式输出适当级别日志

**核心逻辑：**
```csharp
private void OnPlayerEscaping(EscapingEventArgs ev)
{
    if (ev?.Player?.Role != GuardRole)
        return;

    ev.NewRole = _config.GuardOffDutyConfig.EscapeRole;
}
```

### 3. 已删除的功能
- ? 广播消息系统
- ? 玩家忽略列表
- ? 详细日志选项
- ? 字符串角色名称解析

## ?? 配置示例

### YAML 配置格式
```yaml
sgj_plugin:
  enabled: true
  debug: false
  guard_off_duty:
    enabled: true
    escape_role: NtfPrivate
```

### 配置说明

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `enabled` | bool | `true` | 启用/禁用保安下班功能 |
| `escape_role` | RoleTypeId | `NtfPrivate` | 逃离后玩家转换为的角色 |

## ?? 功能行为

### 触发条件
1. **事件**：玩家逃离（Escaping 事件）
2. **角色**：当前角色是设施安保（FacilityGuard）
3. **开关**：功能启用且插件启用

### 执行流程
```
玩家（设施安保）逃离事件触发
    ↓
检查是否启用 → 禁用时退出
    ↓ 启用
检查玩家角色 → 非安保时退出
    ↓ 是安保
修改 ev.NewRole = 配置的逃离角色
    ↓
记录日志（Debug模式下详细，否则简要）
    ↓
玩家转换为新角色并完成逃离
```

## ?? 日志输出示例

### 正常模式
```
[保安下班模块] 已启用
[保安下班模块] 保安 'Player123' 已转换为 NtfPrivate
```

### 调试模式（Debug=true）
```
[保安下班模块] 已启用
[保安下班模块] 设施安保逃离后角色: NtfPrivate
[保安下班模块] 玩家 'Player123' 逃离时角色转换: FacilityGuard → NtfPrivate
[保安下班模块] =============== 保安下班事件 ===============
[保安下班模块] 玩家昵称: Player123
[保安下班模块] 玩家ID: 76561198123456789
[保安下班模块] 原始角色: FacilityGuard
[保安下班模块] 转换前新角色: Spectator
[保安下班模块] 转换后新角色: NtfPrivate
[保安下班模块] 处理时间: 2024-01-20 15:30:45 UTC
[保安下班模块] ==========================================
```

## ?? 代码位置

### 修改的文件
1. **`SGJ_Plugin\Config.cs`** - 配置类
   - 添加了 `using PlayerRoles;`
   - 简化了 `GuardOffDutyConfigClass`

2. **`SGJ_Plugin\Modules\GuardOffDutyModule.cs`** - 模块实现
   - 使用 `Escaping` 事件而非 `Escaped`
   - 直接 RoleTypeId 操作
   - 移除了忽略列表逻辑

3. **`SGJ_Plugin\Main.cs`** - 无需修改
   - 模块已自动注册在 `InitializeModules()` 中

## ?? 支持的角色类型

### 常用角色
- `NtfPrivate` - 九尾狐列兵（默认）
- `NtfSergeant` - 九尾狐中士
- `NtfSpecialist` - 九尾狐专家
- `NtfCaptain` - 九尾狐队长
- `ChaosMarauder` - 混沌掠夺者
- `ChaosConscript` - 混沌征兵
- `Scientist` - 科学家
- `ClassD` - D级人员

### 详见
查看 `GUARD_OFF_DUTY_GUIDE.md` 获取完整的角色列表

## ? 优势总结

| 方面 | 改进 |
|------|------|
| **配置复杂度** | 从7项→2项 |
| **代码行数** | 减少约30% |
| **类型安全** | 使用枚举而非字符串 |
| **维护成本** | 大幅降低 |
| **性能** | 无字符串解析开销 |

## ?? 下一步

1. **编译测试**
   ```bash
   dotnet build
   ```

2. **配置文件**
   - 编辑 EXILED 配置文件
   - 设置 `guard_off_duty: enabled: true`
   - 选择逃离角色

3. **运行测试**
   - 以设施安保身份加入服务器
   - 触发逃离事件
   - 验证转换为指定角色

## ?? 文档

- 详细使用指南：`GUARD_OFF_DUTY_GUIDE.md`
- 项目总体文档：请查看项目 README

---

**实现日期：** 2024年  
**版本：** 1.0.0  
**框架：** EXILED (.NET Framework 4.8.1)  
**状态：** ? 完成
