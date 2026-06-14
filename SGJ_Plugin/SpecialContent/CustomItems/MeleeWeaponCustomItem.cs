using Exiled.API.Features;
using Exiled.API.Features.Items;

namespace SGJ_Plugin.SpecialContent.CustomItems
{
    public class MeleeWeaponCustomItem : GenericCustomItem
    {
        public MeleeWeaponCustomItem() : base("近战武器", ItemType.Jailbird)
        {
            Description = "近战突击武器，切换时获得短暂加速，方便贴近目标。";
            EffectOnSelect = "MovementBoost";
            EffectOnSelectDuration = 4f;
            EffectOnSelectIntensity = 10;
        }

        public override bool OnSelected(Player player, Item item)
        {
            return base.OnSelected(player, item);
        }
    }
}
