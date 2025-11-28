using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System;
using System.Linq;
using System.Windows;

namespace SupermarketManager1.Duy
{
    public partial class WarehouseCRUDWindow : Window
    {
        private WarehouseService _warehouseService = new();
        private AccountService _accountService = new();

        public WarehouseCRUDWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWarehouses();
        }

        private void LoadWarehouses()
        {
            var warehouses = _warehouseService.GetAllWarehouses();
            WarehouseDataGrid.ItemsSource = warehouses;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            WarehouseDetailWindow detailWindow = new WarehouseDetailWindow();
            if (detailWindow.ShowDialog() == true)
            {
                LoadWarehouses();
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Warehouse? selected = WarehouseDataGrid.SelectedItem as Warehouse;
            if (selected == null)
            {
                MessageBox.Show("Please select a warehouse to edit!", "Notification", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            WarehouseDetailWindow detailWindow = new WarehouseDetailWindow();
            detailWindow.EditedWarehouse = selected;
            if (detailWindow.ShowDialog() == true)
            {
                LoadWarehouses();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Warehouse? selected = WarehouseDataGrid.SelectedItem as Warehouse;
            if (selected == null)
            {
                MessageBox.Show("Please select a warehouse to delete!", "Notification", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Không cho xóa Kho Trung Tâm (Central Warehouse đầu tiên duy nhất)
            if (selected.Type == "Central")
            {
                // Kiểm tra xem có phải Central Warehouse đầu tiên (duy nhất) không
                var centralWarehouse = _warehouseService.GetCentralWarehouse();
                if (centralWarehouse != null && centralWarehouse.WarehouseId == selected.WarehouseId)
                {
                    MessageBox.Show("Cannot delete the default Central Warehouse!\n\nThis is the only Central Warehouse in the system.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Nếu có nhiều Central Warehouse, có thể xóa các Central khác (nhưng không nên có)
                MessageBox.Show("Cannot delete Central Warehouse!", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kiểm tra có Staff/Manager nào đang sử dụng kho này không
            var accountsInWarehouse = _accountService.GetAccountsByWarehouse(selected.WarehouseId);
            if (accountsInWarehouse.Count > 0)
            {
                MessageBox.Show(
                    $"Cannot delete this warehouse!\n\nThere are {accountsInWarehouse.Count} accounts using this warehouse.\nPlease transfer them to another warehouse first.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                $"Are you sure you want to delete the warehouse:\n\nName: {selected.WarehouseName}\nType: {selected.Type}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _warehouseService.DeleteWarehouse(selected);
                    MessageBox.Show("Warehouse deleted successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadWarehouses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting warehouse: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

