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
                MessageBox.Show("Vui lòng chọn kho cần sửa!", "Thông báo", 
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
                MessageBox.Show("Vui lòng chọn kho cần xóa!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Không cho xóa Kho Trung Tâm
            if (selected.Type == "Central")
            {
                MessageBox.Show("Không thể xóa Kho Trung Tâm!", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kiểm tra có Staff/Manager nào đang sử dụng kho này không
            var accountsInWarehouse = _accountService.GetAccountsByWarehouse(selected.WarehouseId);
            if (accountsInWarehouse.Count > 0)
            {
                MessageBox.Show(
                    $"Không thể xóa kho này!\n\nCó {accountsInWarehouse.Count} tài khoản đang sử dụng kho này.\nVui lòng chuyển họ sang kho khác trước.",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa kho:\n\nTên: {selected.WarehouseName}\nLoại: {selected.Type}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _warehouseService.DeleteWarehouse(selected);
                    MessageBox.Show("Xóa kho thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadWarehouses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa kho: {ex.Message}", "Lỗi", 
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

