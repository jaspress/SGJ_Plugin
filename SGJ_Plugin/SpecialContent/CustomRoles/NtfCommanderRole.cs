using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class NtfCommanderRole : GenericCustomRole
    {
        public NtfCommanderRole() : base("九尾狐 指挥官", "九尾狐小队", RoleTypeId.NtfCaptain, 125, 0, "blue_green", new List<string> { "安保权限卡", "远程武器", "防护装备", "医疗用品" })
        {
        }
    }
}
