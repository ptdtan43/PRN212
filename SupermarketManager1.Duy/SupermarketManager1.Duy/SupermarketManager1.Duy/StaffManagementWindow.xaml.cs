using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System;
using System.Linq;
using System.Windows;

namespace SupermarketManager1.Duy
{
    public partial class StaffManagementWindow : Window
    {
        private AccountService _accountService = new();
        public int WarehouseId { get; set; }

        public StaffManagementWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStaff();
        }

        private void LoadStaff()
        {
            // Chỉ hiển thị Staff (RoleId = 3) trong Store này
            var allAccounts = _accountService.GetAccountsByWarehouse(WarehouseId);
            var staffList = allAccounts.Where(a => a.RoleId == 3).ToList(); // RoleId = 3 là Staff

            StaffDataGrid.ItemsSource = null;
            StaffDataGrid.ItemsSource = staffList;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            AccountDetailWindow detailWindow = new AccountDetailWindow();
            // Set mặc định là Staff và WarehouseId
            detailWindow.DefaultRoleId = 3; // Staff
            detailWindow.DefaultWarehouseId = WarehouseId;
            if (detailWindow.ShowDialog() == true)
            {
                LoadStaff();
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Account? selected = StaffDataGrid.SelectedItem as Account;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần sửa!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AccountDetailWindow detailWindow = new AccountDetailWindow();
            detailWindow.EditedAccount = selected;
            if (detailWindow.ShowDialog() == true)
            {
                LoadStaff();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Account? selected = StaffDataGrid.SelectedItem as Account;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa nhân viên:\n\nUsername: {selected.Username}\nHọ tên: {selected.FullName}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _accountService.DeleteAccount(selected);
                    MessageBox.Show("Xóa nhân viên thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadStaff();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa nhân viên: {ex.Message}", "Lỗi", 
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

