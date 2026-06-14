namespace SGJ_Plugin.SpecialContent.CustomItems
{
    public class FoodCustomItem : GenericCustomItem
    {
        public FoodCustomItem() : base("食品", ItemType.Painkillers)
        {
            Description = "止痛药：原版使用后获得15秒回血，总计恢复50HP，并移除脑震荡。";
        }
    }
}
