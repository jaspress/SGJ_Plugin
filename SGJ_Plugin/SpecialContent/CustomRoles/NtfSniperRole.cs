using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class NtfSniperRole : GenericCustomRole
    {
        public NtfSniperRole() : base("九尾狐 狙击手", "九尾狐小队", RoleTypeId.NtfSergeant, 150, 0, "blue_green", new List<string>
        {
            "缴械与手铐",
            "对讲机",
            "九尾狐权限卡",
            "m82a1狙击枪",
            "特殊护理医疗包",
            "肾上腺素注射器",
            "绿色夜视仪",
        })
        {
            Stamina = 100;
            Speed = 214.83f;
            BulletResistanceHead = 0;
            BulletResistanceBody = 30;
            BulletResistanceArm = 20;
            BulletResistanceLeg = 20;
            RoleColor = "#6699FF";
            Description = "九尾狐 狙击手负责九尾狐小队的远距离火力压制。\n解锁等级：33\n转生等级：2";
            PrimarySkillEnabled = false;
            SecondarySkillEnabled = false;
        }
    }
}
