using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SupermarketManager1.Duy
{
    public partial class TransferStockWindow : Window
    {
        private WarehouseService _warehouseService = new();
        private InventoryService _inventoryService = new();
        private ProductService _productService = new();
        private List<Inventory> _availableProducts = new();

        public TransferStockWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWarehouses();
        }

        private void LoadWarehouses()
        {
            List<Warehouse> warehouses;

            // Phân quyền: Admin thấy tất cả, Manager chỉ chuyển từ Kho Trung Tâm
            if (CurrentUser.IsAdmin)
            {
                warehouses = _warehouseService.GetAllWarehouses();
                // Admin có thể chuyển giữa tất cả các kho
                ToWarehouseComboBox.ItemsSource = warehouses;
            }
            else if (CurrentUser.IsManager)
            {
                // Manager có thể chuyển từ bất kỳ Kho Trung Tâm nào → Store của mình
                warehouses = _warehouseService.GetCentralWarehouses();
                
                // Kho đích chỉ là Store của Manager
                if (CurrentUser.WarehouseId.HasValue)
                {
                    var managerStore = _warehouseService.GetWarehouseById(CurrentUser.WarehouseId.Value);
                    if (managerStore != null)
                    {
                        ToWarehouseComboBox.ItemsSource = new List<Warehouse> { managerStore };
                        ToWarehouseComboBox.SelectedItem = managerStore;
                        ToWarehouseComboBox.IsEnabled = false; // Không cho chọn kho khác
                    }
                    else
                    {
                        // No manager store found - disable and clear
                        ToWarehouseComboBox.ItemsSource = null;
                        ToWarehouseComboBox.IsEnabled = false;
                    }
                }
                else
                {
                    // Manager but no WarehouseId - disable To combobox
                    ToWarehouseComboBox.ItemsSource = null;
                    ToWarehouseComboBox.IsEnabled = false;
                }
            }
            else
            {
                // Staff không có quyền chuyển kho
                MessageBox.Show("You do not have permission to transfer stock!", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            FromWarehouseComboBox.ItemsSource = warehouses;
            if (warehouses.Count > 0)
            {
                FromWarehouseComboBox.SelectedItem = warehouses[0];
                
                // Nếu là Admin và đã chọn kho nguồn, cập nhật danh sách kho đích
                if (CurrentUser.IsAdmin && warehouses[0] is Warehouse firstWarehouse)
                {
                    var allWarehouses = _warehouseService.GetAllWarehouses();
                    var availableToWarehouses = allWarehouses
                        .Where(w => w.WarehouseId != firstWarehouse.WarehouseId)
                        .ToList();
                    
                    ToWarehouseComboBox.ItemsSource = availableToWarehouses;
                }
            }
        }

        private void FromWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
            
            // Khi Admin chọn kho nguồn, cập nhật danh sách kho đích (loại trừ kho nguồn)
            if (CurrentUser.IsAdmin && FromWarehouseComboBox.SelectedItem is Warehouse selectedFromWarehouse)
            {
                var allWarehouses = _warehouseService.GetAllWarehouses();
                var availableToWarehouses = allWarehouses
                    .Where(w => w.WarehouseId != selectedFromWarehouse.WarehouseId)
                    .ToList();
                
                ToWarehouseComboBox.ItemsSource = availableToWarehouses;
                ToWarehouseComboBox.SelectedItem = null; // Reset selection
            }
        }

        private void ToWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra không được chọn cùng kho
            if (FromWarehouseComboBox.SelectedItem is Warehouse fromWarehouse &&
                ToWarehouseComboBox.SelectedItem is Warehouse toWarehouse)
            {
                if (fromWarehouse.WarehouseId == toWarehouse.WarehouseId)
                {
                    MessageBox.Show("Source warehouse and destination warehouse cannot be the same!", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ToWarehouseComboBox.SelectedItem = null;
                }
            }
        }

        private void LoadProducts()
        {
            if (FromWarehouseComboBox.SelectedItem is Warehouse fromWarehouse)
            {
                _availableProducts = _inventoryService.GetInventoryByWarehouse(fromWarehouse.WarehouseId)
                    .Where(inv => inv.Quantity > 0) // Chỉ hiển thị sản phẩm có tồn kho
                    .ToList();

                ProductComboBox.ItemsSource = _availableProducts;
                ProductComboBox.SelectedItem = null;
                StockInfoLabel.Text = "Stock: 0";
            }
            else
            {
                _availableProducts.Clear();
                ProductComboBox.ItemsSource = null;
                StockInfoLabel.Text = "Stock: 0";
            }
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductComboBox.SelectedItem is Inventory selected)
            {
                StockInfoLabel.Text = $"Stock: {selected.Quantity}";
                QuantityTextBox.Text = "1";
            }
        }

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TransferButton_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (FromWarehouseComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select source warehouse!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ToWarehouseComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select destination warehouse!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a product!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                QuantityTextBox.Focus();
                return;
            }

            Warehouse fromWarehouse = (Warehouse)FromWarehouseComboBox.SelectedItem;
            Warehouse toWarehouse = (Warehouse)ToWarehouseComboBox.SelectedItem;
            Inventory selectedInventory = (Inventory)ProductComboBox.SelectedItem;

            // Kiểm tra tồn kho
            if (quantity > selectedInventory.Quantity)
            {
                MessageBox.Show($"Insufficient stock! Stock: {selectedInventory.Quantity}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                QuantityTextBox.Focus();
                QuantityTextBox.SelectAll();
                return;
            }

            // Xác nhận
            MessageBoxResult confirm = MessageBox.Show(
                $"Transfer {quantity} {selectedInventory.Product?.NameP}\n" +
                $"From: {fromWarehouse.WarehouseName}\n" +
                $"To: {toWarehouse.WarehouseName}\n\n" +
                $"Confirm stock transfer?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            // Thực hiện chuyển kho
            bool success = _inventoryService.TransferStock(
                fromWarehouse.WarehouseId,
                toWarehouse.WarehouseId,
                selectedInventory.ProductCode,
                quantity);

            if (success)
            {
                MessageBox.Show("Stock transfer successful!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Reload products
                LoadProducts();
                QuantityTextBox.Text = "1";
            }
            else
            {
                MessageBox.Show("Stock transfer failed! Please check again.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

