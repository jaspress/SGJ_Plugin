using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class SecurityCaptainRole : GenericCustomRole
    {
        public SecurityCaptainRole() : base("安保部门 上尉", "安保人员", RoleTypeId.FacilityGuard, 125, 0, "blue_green", new List<string> { "安保权限卡", "远程武器", "防护装备", "医疗用品" })
        {
        }
    }
}
