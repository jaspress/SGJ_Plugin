namespace SGJ_Plugin.SpecialContent.CustomItems
{
    public class IdCardCustomItem : GenericCustomItem
    {
        public IdCardCustomItem() : base("ID卡", ItemType.KeycardJanitor)
        {
            Description = "基础身份卡，权限由原版门禁系统决定。";
        }
    }
}
