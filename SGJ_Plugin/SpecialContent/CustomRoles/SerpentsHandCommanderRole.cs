using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class SerpentsHandCommanderRole : GenericCustomRole
    {
        public SerpentsHandCommanderRole() : base("蛇之手 指挥官", "蛇之手", RoleTypeId.Tutorial, 125, 0, "lime", new List<string> { "远程武器", "防护装备", "医疗用品" })
        {
        }
    }
}
