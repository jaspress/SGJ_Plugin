namespace SGJ_Plugin.SpecialContent.CustomItems
{
    public class ArmorCustomItem : GenericCustomItem
    {
        public ArmorCustomItem() : base("防护装备", ItemType.ArmorCombat)
        {
            Description = "战斗护甲：原版提供身体60%、头部80%防护，并提高弹药和武器携带上限。";
        }
    }
}
