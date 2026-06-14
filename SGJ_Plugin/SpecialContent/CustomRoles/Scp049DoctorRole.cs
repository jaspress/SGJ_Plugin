using PlayerRoles;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class Scp049DoctorRole : GenericCustomRole
    {
        public Scp049DoctorRole() : base("SCP-049 瘟疫医生", "SCP", RoleTypeId.Scp049, 500, 200, "red", new List<string>())
        {
        }
    }
}
