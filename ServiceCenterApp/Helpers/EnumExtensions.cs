using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Helpers
{
    public static class EnumExtensions
    {
        public static string? GetDescription(this System.Enum value)
        {
            FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return null;
            DescriptionAttribute? attribute = (DescriptionAttribute?)fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute));
            return attribute != null ? attribute.Description : value.ToString();
        }

        public static string? GetAdditionalDescription(this Enum value)
        {
            FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return null;

            var attribute = (AdditionalDescriptionAttribute?)fieldInfo.GetCustomAttribute(typeof(AdditionalDescriptionAttribute));

            return attribute != null ? attribute.Text : string.Empty;
        }
    }
}
