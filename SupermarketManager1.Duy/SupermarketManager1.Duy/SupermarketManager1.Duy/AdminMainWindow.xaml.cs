using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace SupermarketManager1.Duy
{
    public partial class AdminMainWindow : Window
    {
        ProductService _service = new();

        public AdminMainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsLoggedIn)
            {
                WelcomeLabel.Text = $"Hello, {CurrentUser.Account?.FullName} (Admin)";
            }
            LoadProducts();
        }

        private void LoadProducts()
        {
            FillDataGrid(_service.GetAllProducts());
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            DetailWindow detail = new();
            detail.ShowDialog();
            FillDataGrid(_service.GetAllProducts());
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult answer = MessageBox.Show("Are you sure you want to exit?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        public void FillDataGrid(List<Product> data)
        {
            ProductListDataGrid.ItemsSource = null;
            ProductListDataGrid.ItemsSource = data;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Product? selected = ProductListDataGrid.SelectedItem as Product;
            if (selected == null)
            {
                MessageBox.Show("Please select a product to delete!", "Notification", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult answer = MessageBox.Show(
                $"Are you sure you want to delete the product:\n\nCode: {selected.ProductCode}\nName: {selected.NameP}?",
                "Confirm Delete", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            
            if (answer == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                _service.DeleteProduct(selected);
                MessageBox.Show("Product deleted successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                FillDataGrid(_service.GetAllProducts());
            }
            catch (InvalidOperationException ex)
            {
                // Kiểm tra xem có phải lỗi do tồn kho không
                if (ex.Message.Contains("tồn kho"))
                {
                    // Hỏi người dùng có muốn xóa cả tồn kho không
                    MessageBoxResult deleteInventory = MessageBox.Show(
                        ex.Message + "\n\nDo you want to delete the inventory of this product as well?",
                        "Confirm Delete Inventory",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (deleteInventory == MessageBoxResult.Yes)
                    {
                        try
                        {
                            _service.DeleteProduct(selected, deleteInventory: true);
                            MessageBox.Show("Product and inventory deleted successfully!", "Success", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            FillDataGrid(_service.GetAllProducts());
                        }
                        catch (Exception ex2)
                        {
                            MessageBox.Show($"Error deleting product: {ex2.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    // Lỗi khác (ví dụ: có lịch sử bán hàng)
                    MessageBox.Show(ex.Message, "Cannot Delete", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting product: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Product? selected = ProductListDataGrid.SelectedItem as Product;
            if (selected == null)
            {
                MessageBox.Show("Please select a product to edit!", "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DetailWindow detail = new();
            detail.EditedOne = selected;
            detail.ShowDialog();

            FillDataGrid(_service.GetAllProducts());
        }

        private void ManageWarehouseButton_Click(object sender, RoutedEventArgs e)
        {
            WarehouseManagementWindow warehouseWindow = new WarehouseManagementWindow();
            warehouseWindow.ShowDialog();
        }

        private void ManageAccountButton_Click(object sender, RoutedEventArgs e)
        {
            AdminAccountManagementWindow accountWindow = new AdminAccountManagementWindow();
            accountWindow.ShowDialog();
        }

        private void ManageWarehouseCRUDButton_Click(object sender, RoutedEventArgs e)
        {
            WarehouseCRUDWindow warehouseCRUDWindow = new WarehouseCRUDWindow();
            warehouseCRUDWindow.ShowDialog();
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

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
            var allProducts = _service.GetAllProducts();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                FillDataGrid(allProducts);
            }
            else
            {
                var filtered = allProducts.Where(p =>
                    p.ProductCode.ToLower().Contains(searchText) ||
                    p.NameP?.ToLower().Contains(searchText) == true ||
                    p.Cate?.CategoryName?.ToLower().Contains(searchText) == true
                ).ToList();
                FillDataGrid(filtered);
            }
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

