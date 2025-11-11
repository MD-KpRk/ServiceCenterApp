using System.Windows;
using System.Windows.Controls;

namespace ServiceCenterApp.Helpers
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxHelper), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));
        private static readonly DependencyPropertyKey IsEmptyPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsEmpty", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(true));
        public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;
        
        public static string GetBoundPassword(DependencyObject d)
        {
            return (string)d.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject d, string value)
        {
            d.SetValue(BoundPasswordProperty, value);
        }

        public static bool GetIsEmpty(DependencyObject d)
        {
            return (bool)d.GetValue(IsEmptyProperty);
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox box)
            {
                box.PasswordChanged -= PasswordChanged;
                string newPassword = (string)e.NewValue;

                if (box.Password != newPassword)
                {
                    box.Password = newPassword;
                }

                box.SetValue(IsEmptyPropertyKey, string.IsNullOrEmpty(newPassword));

                box.PasswordChanged += PasswordChanged;
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
            {
                SetBoundPassword(box, box.Password);
                box.SetValue(IsEmptyPropertyKey, string.IsNullOrEmpty(box.Password));
            }
        }
    }
}