using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddPositionWindow : Window
    {
        public string PositionName { get; private set; } = string.Empty;

        public AddPositionWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PositionNameTextBox.Text))
            {
                MessageBox.Show("Введите название должности.");
                return;
            }

            PositionName = PositionNameTextBox.Text;
            DialogResult = true;
        }
    }
}