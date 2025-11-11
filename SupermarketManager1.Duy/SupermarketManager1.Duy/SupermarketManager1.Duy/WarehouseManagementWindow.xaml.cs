using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SupermarketManager1.Duy
{
    public partial class WarehouseManagementWindow : Window
    {
        private WarehouseService _warehouseService = new();
        private InventoryService _inventoryService = new();
        private int _selectedWarehouseId;

        public WarehouseManagementWindow()
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
            WarehouseComboBox.ItemsSource = warehouses;

            // Mặc định chọn Kho Chính
            var centralWarehouse = _warehouseService.GetCentralWarehouse();
            if (centralWarehouse != null)
            {
                WarehouseComboBox.SelectedItem = centralWarehouse;
            }
        }

        private void WarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WarehouseComboBox.SelectedItem is Warehouse selectedWarehouse)
            {
                _selectedWarehouseId = selectedWarehouse.WarehouseId;
                LoadInventory();
            }
        }

        private void LoadInventory()
        {
            var inventoryList = _inventoryService.GetInventoryByWarehouse(_selectedWarehouseId);
            InventoryDataGrid.ItemsSource = inventoryList;
        }

        // Button ➕
        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string productCode)
            {
                _inventoryService.IncreaseStock(_selectedWarehouseId, productCode, 1);
                LoadInventory(); // Refresh
            }
        }

        // Button ➖
        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string productCode)
            {
                int currentStock = _inventoryService.GetStock(_selectedWarehouseId, productCode);
                if (currentStock > 0)
                {
                    _inventoryService.DecreaseStock(_selectedWarehouseId, productCode, 1);
                    LoadInventory(); // Refresh
                }
                else
                {
                    MessageBox.Show("Không thể giảm! Tồn kho đã bằng 0.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // Button 📝 Điều chỉnh
        private void AdjustButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string productCode)
            {
                AdjustQuantityWindow adjustWindow = new AdjustQuantityWindow();
                adjustWindow.ProductCode = productCode;
                adjustWindow.WarehouseId = _selectedWarehouseId;
                adjustWindow.CurrentQuantity = _inventoryService.GetStock(_selectedWarehouseId, productCode);

                if (adjustWindow.ShowDialog() == true)
                {
                    LoadInventory(); // Refresh sau khi điều chỉnh
                }
            }
        }
    }
}