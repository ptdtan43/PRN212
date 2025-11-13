using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SupermarketManager1.Duy
{
    public partial class AdminAccountManagementWindow : Window
    {
        private AccountService _accountService = new();
        private List<Account> _allAccounts = new();

        public AdminAccountManagementWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            _allAccounts = _accountService.GetAllAccounts();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            // Kiểm tra null để tránh NullReferenceException
            if (RoleFilterComboBox == null || AccountDataGrid == null || _allAccounts == null)
                return;

            List<Account> filtered;

            // Kiểm tra SelectedItem
            if (RoleFilterComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string roleTag)
            {
                if (int.TryParse(roleTag, out int roleId))
                {
                    if (roleId == 0) // Tất cả
                    {
                        filtered = _allAccounts;
                    }
                    else
                    {
                        filtered = _allAccounts.Where(a => a.RoleId == roleId).ToList();
                    }
                }
                else
                {
                    // Nếu không parse được, hiển thị tất cả
                    filtered = _allAccounts;
                }
            }
            else
            {
                // Nếu không có item được chọn, hiển thị tất cả
                filtered = _allAccounts;
            }

            AccountDataGrid.ItemsSource = null;
            AccountDataGrid.ItemsSource = filtered;
        }

        private void RoleFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            AccountDetailWindow detailWindow = new AccountDetailWindow();
            if (detailWindow.ShowDialog() == true)
            {
                LoadAccounts();
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Account? selected = AccountDataGrid.SelectedItem as Account;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần sửa!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AccountDetailWindow detailWindow = new AccountDetailWindow();
            detailWindow.EditedAccount = selected;
            if (detailWindow.ShowDialog() == true)
            {
                LoadAccounts();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Account? selected = AccountDataGrid.SelectedItem as Account;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần xóa!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Không cho xóa chính mình
            if (CurrentUser.IsLoggedIn && selected.AccountId == CurrentUser.Account?.AccountId)
            {
                MessageBox.Show("Không thể xóa tài khoản đang đăng nhập!", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa tài khoản:\n\nUsername: {selected.Username}\nHọ tên: {selected.FullName}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _accountService.DeleteAccount(selected);
                    MessageBox.Show("Xóa tài khoản thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAccounts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa tài khoản: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AccountDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Có thể thêm logic khi chọn account
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
