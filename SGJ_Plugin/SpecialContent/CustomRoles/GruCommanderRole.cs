using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class GruCommanderRole : GenericCustomRole
    {
        public GruCommanderRole() : base("GRU-P侵入部队 指挥官", "格鲁乌P（后时代）侵入部队", RoleTypeId.Tutorial, 125, 0, "blue_green", new List<string> { "远程武器", "防护装备", "医疗用品" })
        {
        }
    }
}
