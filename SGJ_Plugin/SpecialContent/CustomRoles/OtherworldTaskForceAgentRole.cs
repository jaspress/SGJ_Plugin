using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class OtherworldTaskForceAgentRole : GenericCustomRole
    {
        public OtherworldTaskForceAgentRole() : base("异界特遣队 特工", "异界特遣队C组", RoleTypeId.Tutorial, 100, 0, "purple", new List<string> { "远程武器", "防护装备", "医疗用品" })
        {
        }
    }
}
