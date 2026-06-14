using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class ClassDHackerRole : GenericCustomRole
    {
        public ClassDHackerRole() : base("D级人员 黑客", "D级人员", RoleTypeId.ClassD, 100, 0, "orange", new List<string> { "ID卡", "医疗用品" })
        {
        }
    }
}
