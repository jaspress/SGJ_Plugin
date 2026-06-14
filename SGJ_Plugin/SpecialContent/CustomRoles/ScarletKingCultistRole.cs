using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class ScarletKingCultistRole : GenericCustomRole
    {
        public ScarletKingCultistRole() : base("深红王之子 狂信徒", "深红王之子", RoleTypeId.Tutorial, 100, 0, "crimson", new List<string> { "远程武器", "医疗用品" })
        {
        }
    }
}
