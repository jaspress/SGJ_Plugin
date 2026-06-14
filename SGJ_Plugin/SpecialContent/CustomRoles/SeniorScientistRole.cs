using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class SeniorScientistRole : GenericCustomRole
    {
        public SeniorScientistRole() : base("高级科研员", "科研人员", RoleTypeId.Scientist, 100, 0, "yellow", new List<string> { "科研权限卡", "医疗用品", "SCP-500 万能药" })
        {
        }
    }
}
