using SupperMarket.BLL.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SupermarketManager1.Duy
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private AccountService _accountService = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = EmailTextBox.Text.Trim();
            string password = PasswordTextBox.Password;

            // Validation
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Vui lòng nhập Username!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập Password!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordTextBox.Focus();
                return;
            }

            // Validate login
            var account = _accountService.Login(username, password);

            if (account == null)
            {
                MessageBox.Show("Username hoặc Password không đúng!\nHoặc tài khoản đã bị khóa.", 
                    "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordTextBox.Clear();
                PasswordTextBox.Focus();
                return;
            }

            // Lưu thông tin user đã login
            CurrentUser.Login(account);

            // Đóng LoginWindow
            this.Hide();

            // Mở MainWindow theo Role
            Window? mainWindow = account.Role.RoleName switch
            {
                "Admin" => new AdminMainWindow(),
                "Manager" => new ManagerMainWindow(),
                "Staff" => new StaffMainWindow(),
                _ => null // Không có Role nào khác
            };

            if (mainWindow == null)
            {
                MessageBox.Show("Vai trò không hợp lệ! Vui lòng liên hệ quản trị viên.", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            mainWindow.Show();
            this.Close();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Bạn có chắc muốn thoát?", 
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EmailTextBox.Focus();
        }
    }
}
