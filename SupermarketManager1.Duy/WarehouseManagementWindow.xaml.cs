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
            // Staff chỉ xem, không được sửa tồn kho
            if (CurrentUser.IsStaff)
            {
                // Ẩn cột "Hành động" cho Staff
                if (InventoryDataGrid.Columns.Count > 4)
                {
                    InventoryDataGrid.Columns[4].Visibility = Visibility.Collapsed;
                }
            }
        }

        private void LoadWarehouses()
        {
            List<Warehouse> warehouses;

            // Phân quyền: Admin thấy tất cả, Manager/Staff chỉ thấy Store của mình
            if (CurrentUser.IsAdmin)
            {
                warehouses = _warehouseService.GetAllWarehouses();
                WarehouseComboBox.ItemsSource = warehouses;
                // Mặc định chọn Kho Chính
                var centralWarehouse = _warehouseService.GetCentralWarehouse();
                if (centralWarehouse != null)
                {
                    WarehouseComboBox.SelectedItem = centralWarehouse;
                }
            }
            else if (CurrentUser.IsManager || CurrentUser.IsStaff)
            {
                // Manager/Staff chỉ thấy Store của mình
                if (CurrentUser.WarehouseId.HasValue)
                {
                    var myWarehouse = _warehouseService.GetWarehouseById(CurrentUser.WarehouseId.Value);
                    if (myWarehouse != null)
                    {
                        warehouses = new List<Warehouse> { myWarehouse };
                        WarehouseComboBox.ItemsSource = warehouses;
                        WarehouseComboBox.SelectedItem = myWarehouse;
                        WarehouseComboBox.IsEnabled = false; // Không cho chọn kho khác
                    }
                }
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

        // Button +
        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string productCode)
            {
                _inventoryService.IncreaseStock(_selectedWarehouseId, productCode, 1);
                LoadInventory(); // Refresh
            }
        }

        // Button -
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
                    MessageBox.Show("Cannot decrease! Stock is already 0.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // Button Điều chỉnh
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}