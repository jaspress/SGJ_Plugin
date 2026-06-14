using PlayerRoles;
using SGJ_Plugin.SpecialContent.Base;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.CustomRoles
{
    public class GenericCustomRole : CustomRoleBase
    {
        public GenericCustomRole()
        {
        }

        public GenericCustomRole(string name, string camp, RoleTypeId baseRole, int health, int artificialHealth, string badgeColor, List<string> loadoutItems)
        {
            Name = name;
            Camp = camp;
            BaseRole = baseRole.ToString();
            Health = health;
            ArtificialHealth = artificialHealth;
            BadgeColor = badgeColor;
            RoleColor = GuessRoleColor(camp);
            KillExperience = GuessKillExperience(name, baseRole);
            Description = $"{name}，隶属于{camp}。";
            PrimarySkillName = "角色能力";
            PrimarySkillDescription = $"触发 {name} 的主能力。";
            SecondarySkillName = "辅助能力";
            SecondarySkillDescription = $"触发 {name} 的辅助能力。";
            LoadoutItems = loadoutItems ?? new List<string>();
        }

        private static string GuessRoleColor(string camp)
        {
            if (camp.Contains("SCP")) return "#FF4040";
            if (camp.Contains("D级")) return "#FF8C00";
            if (camp.Contains("科研")) return "#FFD700";
            if (camp.Contains("混沌")) return "#32CD32";
            if (camp.Contains("蛇之手")) return "#7FFF00";
            if (camp.Contains("GOC")) return "#00FFFF";
            if (camp.Contains("深红")) return "#DC143C";
            return "#6699FF";
        }

        private static int GuessKillExperience(string name, RoleTypeId baseRole)
        {
            if (name.StartsWith("SCP-")) return 90;
            if (baseRole == RoleTypeId.Tutorial) return 45;
            if (name.Contains("指挥官") || name.Contains("主管")) return 50;
            if (name.Contains("重装") || name.Contains("无畏")) return 45;
            return 30;
        }
    }
}
