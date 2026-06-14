using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using PluginHelper = SGJ_Plugin.Helper.Helper;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class NtfCommanderRole : GenericCustomRole
    {
        public NtfCommanderRole() : base("九尾狐 指挥官", "九尾狐小队", RoleTypeId.NtfCaptain, 150, 0, "blue_green", new List<string>
        {
            "缴械与手铐",
            "对讲机",
            "九尾狐权限卡",
            "P90-NTF冲锋枪",
            "沙漠之鹰手枪",
            "特殊护理医疗包",
            "肾上腺素注射器",
            "苏打水",
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
            Description = "九尾狐 指挥官是九尾狐小队的领导者。可调用设施内监控对扫描对象定位。\n解锁等级：16\n转生等级：1\n注意：九尾狐 指挥官可在移动时使用医疗包。";
            PrimarySkillEnabled = true;
            PrimarySkillName = "摄像头扫描";
            PrimarySkillDescription = "在设施摄像头上寻找特定目标（SCP、D级人员、未知人员、科研人员）。";
            PrimarySkillCooldownSeconds = 100f;
        }

        public override bool UsePrimarySkill(Player player)
        {
            if (player == null)
                return false;

            List<Player> targets = Player.List
                .Where(target => target != null && target.IsAlive && target != player)
                .Where(IsScanTarget)
                .OrderBy(target => (target.Position - player.Position).sqrMagnitude)
                .Take(6)
                .ToList();

            string body = targets.Count == 0
                ? "未发现可扫描目标。"
                : string.Join("\n", targets.Select(target =>
                    $"<color=#90EE90>{target.Nickname}</color> | {PluginHelper.GetChineseRoleName(target.Role.Type)} | 距离 {(target.Position - player.Position).magnitude:0}m"));

            PluginHelper.ShowCenterTopHint(player, $"<b><size=21><color=#7FFFD4>[摄像头扫描]</color>\n{body}</size></b>", 6f);
            return true;
        }

        private static bool IsScanTarget(Player player)
        {
            switch (player.Role.Team)
            {
                case Team.SCPs:
                case Team.ClassD:
                case Team.Scientists:
                    return true;
                default:
                    return player.Role.Type == RoleTypeId.Tutorial;
            }
        }
    }
}
