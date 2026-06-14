using PlayerRoles;
using System.Collections.Generic;

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
            PrimarySkillName = "入伍";
            PrimarySkillDescription = "武装D级，并将D级人员转换成混沌分裂者。";
            PrimarySkillCooldownSeconds = 3f;
        }
    }
}
