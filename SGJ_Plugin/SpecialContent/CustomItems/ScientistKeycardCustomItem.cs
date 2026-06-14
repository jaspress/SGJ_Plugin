namespace SGJ_Plugin.SpecialContent.CustomItems
{
    public class ScientistKeycardCustomItem : GenericCustomItem
    {
        public ScientistKeycardCustomItem() : base("科研权限卡", ItemType.KeycardScientist)
        {
            Description = "科研权限卡，权限由原版门禁系统决定。";
        }
    }
}
