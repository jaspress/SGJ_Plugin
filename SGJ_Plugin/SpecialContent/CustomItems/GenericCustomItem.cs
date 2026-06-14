using SGJ_Plugin.SpecialContent.Base;

namespace SGJ_Plugin.SpecialContent.CustomItems
{
    public class GenericCustomItem : CustomItemBase
    {
        public GenericCustomItem()
        {
        }

        public GenericCustomItem(string name, ItemType itemType, bool giveByDefault = false)
        {
            Name = name;
            GameItem = itemType.ToString();
            GiveByDefault = giveByDefault;
            Description = name;
        }
    }
}
