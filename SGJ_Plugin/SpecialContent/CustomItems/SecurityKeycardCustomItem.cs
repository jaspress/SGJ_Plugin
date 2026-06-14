namespace SGJ_Plugin.SpecialContent.CustomItems
{
    public class SecurityKeycardCustomItem : GenericCustomItem
    {
        public SecurityKeycardCustomItem() : base("安保权限卡", ItemType.KeycardMTFPrivate)
        {
            Description = "安保权限卡，权限由原版门禁系统决定。";
        }
    }
}
