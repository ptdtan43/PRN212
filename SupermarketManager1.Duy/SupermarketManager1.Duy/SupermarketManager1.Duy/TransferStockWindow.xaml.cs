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

                // ToWarehouse should be selectable for Admin (all warehouses except the currently selected From)
                ToWarehouseComboBox.ItemsSource = warehouses;
                ToWarehouseComboBox.IsEnabled = true;
                ToWarehouseComboBox.SelectedItem = null;
            }
            else if (CurrentUser.IsManager)
            {
                // Manager chỉ có thể chuyển từ Kho Trung Tâm → Store của mình
                var centralWarehouse = _warehouseService.GetCentralWarehouse();
                warehouses = new List<Warehouse>();
                if (centralWarehouse != null)
                {
                    warehouses.Add(centralWarehouse);
                }

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
                MessageBox.Show("Bạn không có quyền chuyển kho!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            FromWarehouseComboBox.ItemsSource = warehouses;
            if (warehouses.Count > 0)
            {
                FromWarehouseComboBox.SelectedItem = warehouses[0];
                // Ensure ToWarehouse list excludes selected From by default for Admin
                UpdateToWarehouseOptions((Warehouse)FromWarehouseComboBox.SelectedItem);
            }
            else
            {
                FromWarehouseComboBox.SelectedItem = null;
            }
        }

        private void FromWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When From changes we must refresh available products AND update To list (so user can't pick same warehouse)
            LoadProducts();
            if (FromWarehouseComboBox.SelectedItem is Warehouse fromWarehouse)
            {
                UpdateToWarehouseOptions(fromWarehouse);
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
                    MessageBox.Show("Kho nguồn và kho đích không được trùng nhau!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ToWarehouseComboBox.SelectedItem = null;
                }
            }
        }

        // Ensure ToWarehouseComboBox contains valid targets given a selected From warehouse.
        private void UpdateToWarehouseOptions(Warehouse fromWarehouse)
        {
            // If To combobox is disabled (Manager fixed store) do nothing
            if (ToWarehouseComboBox.IsEnabled == false)
                return;

            // Get all warehouses and exclude the selected From
            var all = _warehouseService.GetAllWarehouses() ?? new List<Warehouse>();
            var targets = all.Where(w => w.WarehouseId != fromWarehouse.WarehouseId).ToList();

            ToWarehouseComboBox.ItemsSource = targets;

            // If previously selected To is now invalid, clear it
            if (ToWarehouseComboBox.SelectedItem is Warehouse selected &&
                targets.All(t => t.WarehouseId != selected.WarehouseId))
            {
                ToWarehouseComboBox.SelectedItem = null;
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
                StockInfoLabel.Text = "Tồn kho: 0";
            }
            else
            {
                _availableProducts.Clear();
                ProductComboBox.ItemsSource = null;
                StockInfoLabel.Text = "Tồn kho: 0";
            }
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductComboBox.SelectedItem is Inventory selected)
            {
                StockInfoLabel.Text = $"Tồn kho: {selected.Quantity}";
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
                MessageBox.Show("Vui lòng chọn kho nguồn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ToWarehouseComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn kho đích!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                QuantityTextBox.Focus();
                return;
            }

            Warehouse fromWarehouse = (Warehouse)FromWarehouseComboBox.SelectedItem;
            Warehouse toWarehouse = (Warehouse)ToWarehouseComboBox.SelectedItem;
            Inventory selectedInventory = (Inventory)ProductComboBox.SelectedItem;

            // Kiểm tra tồn kho
            if (quantity > selectedInventory.Quantity)
            {
                MessageBox.Show($"Không đủ hàng! Tồn kho: {selectedInventory.Quantity}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                QuantityTextBox.Focus();
                QuantityTextBox.SelectAll();
                return;
            }

            // Xác nhận
            MessageBoxResult confirm = MessageBox.Show(
                $"Chuyển {quantity} {selectedInventory.Product?.NameP}\n" +
                $"Từ: {fromWarehouse.WarehouseName}\n" +
                $"Đến: {toWarehouse.WarehouseName}\n\n" +
                $"Xác nhận chuyển kho?",
                "Xác nhận",
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
                MessageBox.Show("Chuyển kho thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Reload products
                LoadProducts();
                QuantityTextBox.Text = "1";
            }
            else
            {
                MessageBox.Show("Chuyển kho thất bại! Vui lòng kiểm tra lại.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

