using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ServiceCenterApp.Helpers
{
    public static class MaxLengthHelper
    {
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached(
                "Attach",
                typeof(bool),
                typeof(MaxLengthHelper),
                new PropertyMetadata(false, OnAttachChanged));

        public static bool GetAttach(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachProperty);
        }

        public static void SetAttach(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachProperty, value);
        }

        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox && (bool)e.NewValue)
            {
                textBox.Loaded += TextBox_Loaded;
                textBox.DataContextChanged += TextBox_DataContextChanged;
                if (textBox.DataContext != null)
                {
                    UpdateMaxLength(textBox);
                }
            }
        }

        private static void TextBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateMaxLength((TextBox)sender);
        }

        private static void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMaxLength((TextBox)sender);
        }

        private static void UpdateMaxLength(TextBox textBox)
        {
            var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression == null) return;
            var dataItem = bindingExpression.DataItem;
            if (dataItem == null) return;
            string bindingPath = bindingExpression.ParentBinding.Path.Path;
            if (string.IsNullOrEmpty(bindingPath)) return;
            string[] properties = bindingPath.Split('.');
            Type currentType = dataItem.GetType();
            PropertyInfo propertyInfo = null;

            foreach (string propertyName in properties)
            {
                if (propertyName.Contains("[")) return;

                propertyInfo = currentType.GetProperty(propertyName);
                if (propertyInfo == null) return;
                currentType = propertyInfo.PropertyType;
            }

            if (propertyInfo != null)
            {
                var maxLengthAttribute = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLengthAttribute != null)
                {
                    textBox.MaxLength = maxLengthAttribute.Length;
                    return; 
                }

                var stringLengthAttribute = propertyInfo.GetCustomAttribute<StringLengthAttribute>();
                if (stringLengthAttribute != null)
                {
                    textBox.MaxLength = stringLengthAttribute.MaximumLength;
                }
            }
        }
    }
}