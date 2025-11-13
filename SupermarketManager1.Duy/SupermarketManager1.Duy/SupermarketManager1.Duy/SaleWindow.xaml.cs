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
    public partial class SaleWindow : Window
    {
        private InventoryService _inventoryService = new();
        private ProductService _productService = new();
        private SaleService _saleService = new();
        private List<Inventory> _availableProducts = new();
        private List<CartItem> _cart = new();

        // Class để lưu item trong giỏ hàng
        private class CartItem
        {
            public string ProductCode { get; set; } = "";
            public string ProductName { get; set; } = "";
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalAmount => Quantity * UnitPrice;
        }

        public SaleWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAvailableProducts();
        }

        // Load danh sách sản phẩm có tồn kho trong Store của Staff
        private void LoadAvailableProducts()
        {
            if (!CurrentUser.WarehouseId.HasValue)
            {
                MessageBox.Show("Cannot determine Store!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            _availableProducts = _inventoryService.GetInventoryByWarehouse(CurrentUser.WarehouseId.Value)
                .Where(inv => inv.Quantity > 0) // Chỉ hiển thị sản phẩm còn hàng
                .ToList();

            ProductDataGrid.ItemsSource = _availableProducts;
        }

        // Tìm kiếm sản phẩm
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                ProductDataGrid.ItemsSource = _availableProducts;
            }
            else
            {
                var filtered = _availableProducts.Where(inv =>
                    inv.ProductCode.ToLower().Contains(searchText) ||
                    inv.Product?.NameP?.ToLower().Contains(searchText) == true
                ).ToList();
                ProductDataGrid.ItemsSource = filtered;
            }
        }

        // Chọn sản phẩm
        private void ProductDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductDataGrid.SelectedItem is Inventory selected)
            {
                SelectedProductTextBox.Text = $"{selected.ProductCode} - {selected.Product?.NameP}";
                QuantityTextBox.Text = "1";
                QuantityTextBox.Focus();
                QuantityTextBox.SelectAll();
            }
        }

        // Chỉ cho phép nhập số
        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Thêm vào giỏ hàng
        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductDataGrid.SelectedItem is not Inventory selected)
            {
                MessageBox.Show("Please select a product!", "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                QuantityTextBox.Focus();
                return;
            }

            // Kiểm tra tồn kho
            if (quantity > selected.Quantity)
            {
                MessageBox.Show($"Insufficient stock! Stock: {selected.Quantity}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                QuantityTextBox.Focus();
                QuantityTextBox.SelectAll();
                return;
            }

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var existingItem = _cart.FirstOrDefault(c => c.ProductCode == selected.ProductCode);
            if (existingItem != null)
            {
                int newQuantity = existingItem.Quantity + quantity;
                if (newQuantity > selected.Quantity)
                {
                    MessageBox.Show($"Total quantity exceeds stock! Stock: {selected.Quantity}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                existingItem.Quantity = newQuantity;
            }
            else
            {
                // Thêm mới vào giỏ
                CartItem newItem = new CartItem
                {
                    ProductCode = selected.ProductCode,
                    ProductName = selected.Product?.NameP ?? "",
                    Quantity = quantity,
                    UnitPrice = selected.Product?.Price ?? 0
                };
                _cart.Add(newItem);
            }

            RefreshCart();
            QuantityTextBox.Text = "1";
        }

        // Xóa khỏi giỏ hàng
        private void RemoveFromCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string productCode)
            {
                var item = _cart.FirstOrDefault(c => c.ProductCode == productCode);
                if (item != null)
                {
                    _cart.Remove(item);
                    RefreshCart();
                }
            }
        }

        // Xóa toàn bộ giỏ hàng
        private void ClearCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0) return;

            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear the entire cart?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _cart.Clear();
                RefreshCart();
            }
        }

        // Refresh giỏ hàng
        private void RefreshCart()
        {
            CartDataGrid.ItemsSource = null;
            CartDataGrid.ItemsSource = _cart;

            // Tính tổng tiền
            decimal total = _cart.Sum(c => c.TotalAmount);
            TotalAmountLabel.Text = $"{total:N0} VNĐ";
        }

        // Thanh toán
        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Cart is empty!", "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!CurrentUser.IsLoggedIn || !CurrentUser.WarehouseId.HasValue)
            {
                MessageBox.Show("User authentication error!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Xác nhận thanh toán
            decimal total = _cart.Sum(c => c.TotalAmount);
            MessageBoxResult confirm = MessageBox.Show(
                $"Total Amount: {total:N0} VNĐ\n\nConfirm payment?",
                "Confirm Payment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            // Tạo Sales cho từng sản phẩm
            int successCount = 0;
            int failCount = 0;
            List<string> errors = new();

            foreach (var item in _cart)
            {
                bool success = _saleService.CreateSale(
                    CurrentUser.Account!.AccountId,
                    CurrentUser.WarehouseId.Value,
                    item.ProductCode,
                    item.Quantity,
                    out string errorMessage);

                if (success)
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                    errors.Add($"{item.ProductName}: {errorMessage}");
                }
            }

            // Hiển thị kết quả
            if (failCount == 0)
            {
                MessageBox.Show(
                    $"Payment successful!\n\nSold {successCount} products\nTotal Amount: {total:N0} VNĐ",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Xóa giỏ hàng và reload danh sách
                _cart.Clear();
                RefreshCart();
                LoadAvailableProducts();
            }
            else
            {
                string errorDetails = string.Join("\n", errors);
                MessageBox.Show(
                    $"An error occurred!\n\nSuccess: {successCount}\nFailed: {failCount}\n\nError details:\n{errorDetails}",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Reload danh sách để cập nhật tồn kho
                LoadAvailableProducts();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra nếu có sản phẩm trong giỏ hàng
            if (_cart.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    "You have products in your cart.\nAre you sure you want to close this window?",
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            this.Close();
        }
    }
}

