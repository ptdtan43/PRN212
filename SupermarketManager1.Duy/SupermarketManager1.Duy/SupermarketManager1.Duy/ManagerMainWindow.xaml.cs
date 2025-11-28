using System.Windows;

namespace SupermarketManager1.Duy
{
    public partial class ManagerMainWindow : Window
    {
        public ManagerMainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsLoggedIn)
            {
                string storeName = CurrentUser.Account?.Warehouse?.WarehouseName ?? "Store";
                WelcomeLabel.Text = $"Hello, {CurrentUser.Account?.FullName} - {storeName}";
            }
        }

        private void ManageInventoryButton_Click(object sender, RoutedEventArgs e)
        {
            WarehouseManagementWindow warehouseWindow = new WarehouseManagementWindow();
            warehouseWindow.ShowDialog();
        }

        private void ManageStaffButton_Click(object sender, RoutedEventArgs e)
        {
            // Manager quản lý Staff trong Store của mình
            if (CurrentUser.WarehouseId.HasValue)
            {
                StaffManagementWindow staffWindow = new StaffManagementWindow();
                staffWindow.WarehouseId = CurrentUser.WarehouseId.Value;
                staffWindow.ShowDialog();
            }
        }

        private void TransferStockButton_Click(object sender, RoutedEventArgs e)
        {
            TransferStockWindow transferWindow = new TransferStockWindow();
            transferWindow.ShowDialog();
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            ReportWindow reportWindow = new ReportWindow();
            reportWindow.ShowDialog();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to logout?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                CurrentUser.Logout();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}

