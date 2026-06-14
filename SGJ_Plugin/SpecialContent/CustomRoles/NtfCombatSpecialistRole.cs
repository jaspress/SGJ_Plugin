using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using PluginHelper = SGJ_Plugin.Helper.Helper;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class NtfCombatSpecialistRole : GenericCustomRole
    {
        public NtfCombatSpecialistRole() : base("九尾狐 战斗专家", "九尾狐小队", RoleTypeId.NtfSpecialist, 150, 0, "blue_green", new List<string>
        {
            "缴械与手铐",
            "对讲机",
            "九尾狐权限卡",
            "P90-NTF冲锋枪",
            "沙漠之鹰手枪",
            "特殊护理医疗包",
            "肾上腺素注射器",
            "绿色夜视仪",
            "防毒面具",
        })
        {
            Stamina = 100;
            Speed = 214.83f;
            BulletResistanceHead = 0;
            BulletResistanceBody = 30;
            BulletResistanceArm = 20;
            BulletResistanceLeg = 20;
            RoleColor = "#6699FF";
            Description = "九尾狐 战斗专家是九尾狐小队的收容专家。可使用技能硬控被捕获对象一段时间。\n解锁等级：8\n注意：九尾狐 战斗专家可在移动时使用医疗包。";
            PrimarySkillEnabled = true;
            PrimarySkillName = "电磁捕获网";
            PrimarySkillDescription = "向前发射一面电磁捕获网，被命中的SCP会被定住几秒钟。";
            PrimarySkillCooldownSeconds = 70f;
        }

        public override bool UsePrimarySkill(Player player)
        {
            if (player == null)
                return false;

            Player target = Player.List
                .Where(candidate => candidate != null && candidate.IsAlive && candidate.Role.Team == Team.SCPs)
                .Where(candidate => (candidate.Position - player.Position).magnitude <= 12f)
                .OrderBy(candidate => (candidate.Position - player.Position).sqrMagnitude)
                .FirstOrDefault();

            if (target == null)
            {
                PluginHelper.ShowTopRightHint(player, "<b><size=20><color=#FF9999>[电磁捕获网]</color> 范围内没有可捕获的SCP</size></b>", 3f);
                return false;
            }

            target.EnableEffect(EffectType.Ensnared, 1, 4.5f, true);
            target.EnableEffect(EffectType.Disabled, 1, 4.5f, true);
            PluginHelper.ShowCenterTopHint(player, $"<b><size=21><color=#7FFFD4>[电磁捕获网]</color>\n已捕获 <color=#90EE90>{target.Nickname}</color> 4.5秒</size></b>", 4f);
            PluginHelper.ShowCenterTopHint(target, "<b><size=21><color=#FF9999>[电磁捕获网]</color>\n你被九尾狐战斗专家捕获，暂时无法行动。</size></b>", 4f);
            return true;
        }
    }
}
