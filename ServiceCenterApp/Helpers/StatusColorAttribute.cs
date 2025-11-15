using System;
using System.Windows.Media;

namespace ServiceCenterApp.Helpers
{
    [AttributeUsage(AttributeTargets.Field)] 
    public class StatusColorAttribute : Attribute
    {
        public string ColorName { get; }

        public StatusColorAttribute(string colorName)
        {
            ColorName = colorName;
        }

        public Brush ToBrush()
        {
            return (Brush)new BrushConverter().ConvertFromString(ColorName);
        }
    }
}