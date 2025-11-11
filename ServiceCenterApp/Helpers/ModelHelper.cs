using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ServiceCenterApp.Helpers
{
    public static class ModelHelper
    {
        public static int GetMaxLength(Type type, string propertyName)
        {
            PropertyInfo? propInfo = type.GetProperty(propertyName) ?? throw new ArgumentException($"Свойство '{propertyName}' не найдено у типа '{type.Name}'.");
            MaxLengthAttribute? maxLengthAttr = propInfo.GetCustomAttribute<MaxLengthAttribute>();

            return maxLengthAttr?.Length ?? throw new ArgumentException($"Ошибка получения длинны у '{propertyName}'");
        }
    }
}
