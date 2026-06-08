# SGJ_Plugin - 枪械无限子弹实现指南

本文档详细说明了如何根据 EXILED 框架实现仅限枪械武器的无限子弹功能。

## 功能概述

### InfiniteAmmoModule（枪械无限子弹模块）

**主要特性：**
- ? **仅限枪械** - 只对枪械武器有效，排除手榴弹、SCP物品等
- ? **装弹配置** - 控制是否恢复弹匣
- ? **备弹配置** - 控制是否提供无限备弹
- ? **玩家黑名单** - 排除特定玩家
- ? **枪械白名单** - 仅应用于指定的枪械
- ? **枪械黑名单** - 排除特定的枪械
- ? **调试模式** - 详细的事件日志

## 支持的枪械类型

### 手枪 (Pistols)
```
COM15              COM15 手枪
Makarov            Makarov PM
Deagle             Desert Eagle
USP                USP 匹配赛
Jailbird           特殊霰枪型手枪
```

### 冲锋枪 (SMGs)
```
Crossvec           Crossvec SMG
MP7                MP7 冲锋枪
ACP                ACP-7 冲锋枪
```

### 步枪 (Rifles)
```
E11SR              E-11 标准步枪
Famas              FAMAS 步枪
AK                 AK 自动步枪
```

### 霰枪 (Shotguns)
```
Shotgun            Combat Shotgun
```

### 特殊武器 (Special)
```
Tesla              Tesla Cannon
Loopback           Loopback 环回枪
```

## 配置说明

### 完整配置示例

```yaml
sgj_plugin:
  enabled: true              # 启用插件
  debug: false               # 调试模式
  infinite_ammo:
    enabled: true            # 启用无限子弹功能
    restore_on_reload: true  # 装弹时恢复弹匣
    infinite_reserve_ammo: true  # 无限备弹
    ignore_players: []       # 忽略的玩家ID
    allowed_guns: []         # 允许的枪械（空=所有）
    excluded_guns: []        # 排除的枪械
```

### 配置字段说明

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `IsEnabled` | bool | true | 启用/禁用功能 |
| `RestoreOnReload` | bool | true | 装弹时是否恢复弹匣 |
| `InfiniteReserveAmmo` | bool | true | 是否提供无限备弹 |
| `IgnorePlayers` | List<string> | [] | 玩家ID黑名单 |
| `AllowedGuns` | List<string> | [] | 允许的枪械列表（空=所有） |
| `ExcludedGuns` | List<string> | [] | 排除的枪械列表 |

### 配置用例

**用例1：仅限E11步枪无限子弹**
```yaml
infinite_ammo:
  enabled: true
  allowed_guns:
    - E11SR            # 仅E11步枪
  excluded_guns: []
```

**用例2：除了Desert Eagle外都有无限子弹**
```yaml
infinite_ammo:
  enabled: true
  allowed_guns: []            # 所有枪械
  excluded_guns:
    - Deagle                  # 排除Desert Eagle
```

**用例3：特定玩家无法使用**
```yaml
infinite_ammo:
  enabled: true
  ignore_players:
    - "76561198123456789"     # Steam ID
    - "76561198987654321"
```

## 模块工作流程

### 装弹事件处理流程

```
玩家装弹
  ↓
[ReloadingWeapon 事件触发]
  ↓
检查：模块是否启用？ ? 否 ? 结束
  ↓ 是
检查：玩家是否在黑名单？ ? 是 ? 结束
  ↓ 否
检查：武器是否是枪械？ ? 否 ? 结束
  ↓ 是
检查：武器是否在黑名单？ ? 是 ? 结束
  ↓ 否
检查：是否设置了白名单？
  ├─ 是：武器在白名单中？ ? 否 ? 结束
  └─ 否：继续
  ↓
允许装弹 (ev.IsAllowed = true)
  ↓
恢复弹匣 (如果启用)
  ↓
恢复备弹 (如果启用)
  ↓
记录日志 (如果调试模式)
```

## 代码实现细节

### 关键函数

#### 1. IsGunWeapon(string weaponName)
检查武器是否是枪械
```csharp
private bool IsGunWeapon(string weaponName)
{
    // 检查武器名称是否在枪械列表中
    foreach (var gunName in GunWeaponNames)
    {
        if (weaponName.Equals(gunName, StringComparison.OrdinalIgnoreCase))
            return true;
    }
    return false;
}
```

#### 2. GetMagazineCapacity(string weaponName)
获取武器的弹匣容量
```csharp
private int GetMagazineCapacity(string weaponName)
{
    string weapon = weaponName.ToLower();

    // 根据武器名称返回弹匣容量
    if (weapon == "com15") return 15;
    if (weapon == "makarov") return 8;
    // ...更多武器
    return 30;  // 默认值
}
```

#### 3. HandleInfiniteAmmo(ReloadingWeaponEventArgs ev, string weaponName)
处理无限子弹的核心逻辑
```csharp
private void HandleInfiniteAmmo(ReloadingWeaponEventArgs ev, string weaponName)
{
    var player = ev.Player;

    // 恢复弹匣
    if (_config.InfiniteAmmoConfig.RestoreOnReload)
    {
        int capacity = GetMagazineCapacity(weaponName);
        // 恢复弹匣逻辑
    }

    // 恢复备弹
    if (_config.InfiniteAmmoConfig.InfiniteReserveAmmo)
    {
        // 恢复备弹逻辑
    }
}
```

## 事件处理

### ReloadingWeapon 事件

**触发时机：** 玩家开始装弹时

**事件参数：**
- `ev.Player` - 装弹的玩家
- `ev.Player.CurrentItem` - 正在装弹的武器
- `ev.IsAllowed` - 是否允许装弹（可设置）

**使用示例：**
```csharp
private void OnReloadingWeapon(ReloadingWeaponEventArgs ev)
{
    if (ev?.Player?.CurrentItem == null)
        return;

    // 检查是否是枪械
    if (!IsGunWeapon(ev.Player.CurrentItem.Type.ToString()))
        return;

    // 允许装弹
    ev.IsAllowed = true;
}
```

## 常见问题与解决方案

### Q1: 无限子弹功能不生效

**可能原因：**
1. 功能未启用（`IsEnabled = false`）
2. 武器不在支持列表中
3. 玩家在黑名单中
4. 武器在黑名单中

**解决方案：**
```
1. 检查 Config 中 InfiniteAmmoConfig.IsEnabled = true
2. 查看调试日志确认武器类型
3. 检查玩家ID是否在黑名单中
4. 启用调试模式查看详细日志
```

### Q2: 如何针对特定玩家启用功能？

**使用允许列表反向逻辑：**
```csharp
// 在配置中设置
if (_allowedGunsSet.Count > 0 && !_allowedGunsSet.Contains(weaponName.ToLower()))
    return;  // 不在白名单中，跳过处理
```

### Q3: 如何记录装弹事件用于日志？

**启用调试模式：**
```yaml
debug: true  # 启用调试模式
```

然后在代码中：
```csharp
if (_config.Debug)
{
    Log.Debug($"[{Name}] 玩家 '{player.Nickname}' 装弹枪械: {weaponName}");
}
```

## 扩展功能建议

### 1. 限制无限子弹的时间

```csharp
private DateTime _featureStartTime;
private TimeSpan _featureDuration = TimeSpan.FromHours(1);

// 在 OnEnable 中
_featureStartTime = DateTime.UtcNow;

// 在事件处理中
if (DateTime.UtcNow - _featureStartTime > _featureDuration)
    return;  // 功能已过期
```

### 2. 每个玩家的装弹次数统计

```csharp
private Dictionary<string, int> _playerReloadCounts = new();

// 每次装弹时计数
if (_playerReloadCounts.TryGetValue(playerId, out int count))
    _playerReloadCounts[playerId]++;
else
    _playerReloadCounts[playerId] = 1;
```

### 3. 基于伤害的弹匣消耗

```csharp
// 根据造成的伤害量消耗备弹
private void OnPlayerHurting(HurtingEventArgs ev)
{
    if (ev.Attacker?.CurrentItem?.Type.ToString() is string weaponName)
    {
        int damageAsCost = (int)ev.Damage;
        // 消耗备弹
    }
}
```

## 测试清单

- [ ] 枪械可以正常装弹
- [ ] 装弹后没有消耗备弹
- [ ] 手榴弹不受影响
- [ ] SCP物品不受影响
- [ ] 玩家黑名单生效
- [ ] 白名单逻辑正确
- [ ] 黑名单逻辑正确
- [ ] 调试模式日志输出正确
- [ ] 模块启用/禁用日志输出正确

## 参考资源

- [EXILED 官方 GitHub](https://github.com/ExMod-Team/EXILED)
- [EXILED 深度文档](https://deepwiki.com/ExMod-Team/EXILED)
- [EXILED Wiki](https://github.com/ExMod-Team/EXILED/wiki)

## 版本信息

- **框架版本：** .NET Framework 4.8.1
- **EXILED 版本：** 兼容最新版本
- **模块版本：** 1.0.0

---

**最后更新：** 2024年
**维护者：** WJ
