using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PluginHelper = SGJ_Plugin.Helper.Helper;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class ChaosCommanderRole : GenericCustomRole
    {
        public ChaosCommanderRole() : base("混沌分裂者 指挥官", "混沌分裂者", RoleTypeId.ChaosRifleman, 150, 0, "green", new List<string> { "CI对讲机", "特殊权限卡", "远程武器", "医疗用品", "防护装备" })
        {
            Speed = 203.28f;
            BulletResistanceHead = 0;
            BulletResistanceBody = 30;
            BulletResistanceArm = 20;
            BulletResistanceLeg = 20;
            Description = "混沌分裂者常规部队的领导者，持有未知权限卡，配备有SCAR-H自动步枪。";
            PrimarySkillEnabled = true;
            PrimarySkillName = "入伍";
            PrimarySkillDescription = "武装D级，并将D级人员转换成混沌分裂者。";
            PrimarySkillCooldownSeconds = 3f;
        }

        public override bool UsePrimarySkill(Player player)
        {
            if (player == null || !PrimarySkillEnabled)
                return false;

            Player target = Player.List
                .Where(candidate => candidate != null && candidate != player && candidate.Role.Type == RoleTypeId.ClassD)
                .Where(candidate => Vector3.Distance(candidate.Position, player.Position) <= 5f)
                .OrderBy(candidate => Vector3.Distance(candidate.Position, player.Position))
                .FirstOrDefault();

            if (target == null)
            {
                PluginHelper.ShowTopRightHint(player, "<b><size=20><color=#FF9999>[技能]</color> 附近5米内没有可入伍的D级人员</size></b>", 3f);
                return false;
            }

            string targetName = target.Nickname;
            target.Role.Set(RoleTypeId.ChaosConscript, SpawnReason.ForceClass);
            PluginHelper.ShowTopRightHint(player, $"<b><size=20><color=#90EE90>[技能]</color> 已将 <color=#7FFFD4>{targetName}</color> 编入混沌分裂者</size></b>", 4f);
            PluginHelper.ShowTopRightHint(target, $"<b><size=20><color=#90EE90>[特殊角色]</color> 你已被 <color=#7FFFD4>{player.Nickname}</color> 编入混沌分裂者</size></b>", 4f);
            return true;
        }
    }
}
