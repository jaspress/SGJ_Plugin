using Exiled.API.Features;
using Exiled.API.Features.Items;

namespace SGJ_Plugin.SpecialContent.CustomItems
{
    public class CiRadioCustomItem : GenericCustomItem
    {
        public CiRadioCustomItem() : base("CI对讲机", ItemType.Radio)
        {
            Description = "无线电：原版可远距离通信、切换范围和开关电源；CI版本切换时短暂提升机动状态。";
            EffectOnSelect = "Invigorated";
            EffectOnSelectDuration = 6f;
            EffectOnSelectIntensity = 1;
        }

        public override bool OnSelected(Player player, Item item)
        {
            return base.OnSelected(player, item);
        }
    }
}
