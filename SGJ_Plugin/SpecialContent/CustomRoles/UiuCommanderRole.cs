using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class UiuCommanderRole : GenericCustomRole
    {
        public UiuCommanderRole() : base("UIU特工组 指挥官", "特异事故处特工小组", RoleTypeId.Tutorial, 125, 0, "silver", new List<string> { "远程武器", "防护装备", "医疗用品" })
        {
        }
    }
}
