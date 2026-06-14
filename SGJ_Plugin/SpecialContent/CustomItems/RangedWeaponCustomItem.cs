using Exiled.API.Features;
using Exiled.API.Features.Items;

namespace SGJ_Plugin.SpecialContent.CustomItems
{
    public class RangedWeaponCustomItem : GenericCustomItem
    {
        public RangedWeaponCustomItem() : base("远程武器", ItemType.GunCOM15)
        {
            Description = "COM-15：原版9x19mm半自动手枪，12(+1)弹容量；作为特殊远程武器时切换会补充基础弹药。";
            AmmoOnSelect = 80;
        }

        public override bool OnSelected(Player player, Item item)
        {
            return base.OnSelected(player, item);
        }
    }
}
