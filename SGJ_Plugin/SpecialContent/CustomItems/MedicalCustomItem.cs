namespace SGJ_Plugin.SpecialContent.CustomItems
{
    public class MedicalCustomItem : GenericCustomItem
    {
        public MedicalCustomItem() : base("医疗用品", ItemType.Medkit)
        {
            Description = "医疗包：原版使用后治疗65HP，并移除流血和烧伤。";
        }
    }
}
