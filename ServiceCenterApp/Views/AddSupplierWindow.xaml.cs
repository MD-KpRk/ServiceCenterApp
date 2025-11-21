using ServiceCenterApp.Models;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddSupplierWindow : Window
    {
        public Supplier NewSupplier { get; private set; }

        public AddSupplierWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTb.Text) || string.IsNullOrWhiteSpace(ContactTb.Text))
            {
                MessageBox.Show("Название и Контакты обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewSupplier = new Supplier
            {
                Name = NameTb.Text,
                Contacts = ContactTb.Text,
                Details = DetailsTb.Text
            };

            DialogResult = true;
        }
    }
}