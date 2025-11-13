namespace ServiceCenterApp.Helpers
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AdditionalDescriptionAttribute : Attribute
    {
        public string Text { get; }

        public AdditionalDescriptionAttribute(string text)
        {
            Text = text;
        }
    }
}
