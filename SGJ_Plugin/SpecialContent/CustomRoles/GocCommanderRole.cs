using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class GocCommanderRole : GenericCustomRole
    {
        public GocCommanderRole() : base("GOC 指挥官", "全球超自然联盟 攻击小组", RoleTypeId.Tutorial, 125, 0, "cyan", new List<string> { "远程武器", "防护装备", "医疗用品" })
        {
        }
    }
}
