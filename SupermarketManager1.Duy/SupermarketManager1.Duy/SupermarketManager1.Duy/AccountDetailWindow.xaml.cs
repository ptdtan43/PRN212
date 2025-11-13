using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SupermarketManager1.Duy
{
    public partial class AccountDetailWindow : Window
    {
        private AccountService _accountService = new();
        private RoleService _roleService = new();
        private WarehouseService _warehouseService = new();

        public Account? EditedAccount { get; set; }
        public bool IsEditMode => EditedAccount != null;
        public int? DefaultRoleId { get; set; } // Để set mặc định Role khi tạo mới
        public int? DefaultWarehouseId { get; set; } // Để set mặc định Warehouse khi tạo mới

        public AccountDetailWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load Roles
            var allRoles = _roleService.GetAllRoles();
            
            if (IsEditMode)
            {
                // Khi edit: hiển thị tất cả Roles (bao gồm Admin)
                RoleComboBox.ItemsSource = allRoles;
                TitleLabel.Text = "Sửa thông tin tài khoản";
                LoadAccountData();
                // Cho phép sửa trạng thái khi edit
                StatusComboBox.IsEnabled = true;
            }
            else
            {
                // ⭐ Khi tạo mới: Loại bỏ Role "Admin" - chỉ cho phép tạo Manager hoặc Staff
                var rolesWithoutAdmin = allRoles.Where(r => r.RoleName != "Admin").ToList();
                RoleComboBox.ItemsSource = rolesWithoutAdmin;
                
                TitleLabel.Text = "Tạo tài khoản mới";
                
                // ⭐ Set trạng thái mặc định là "Active" và disable khi tạo mới
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content.ToString() == "Active")
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
                StatusComboBox.IsEnabled = false; // Không cho sửa trạng thái khi tạo mới
                
                // Set mặc định nếu có
                if (DefaultRoleId.HasValue)
                {
                    // Chỉ set nếu không phải Admin
                    var defaultRole = allRoles.FirstOrDefault(r => r.RoleId == DefaultRoleId.Value);
                    if (defaultRole != null && defaultRole.RoleName != "Admin")
                    {
                        RoleComboBox.SelectedValue = DefaultRoleId.Value;
                    }
                }
                if (DefaultWarehouseId.HasValue)
                {
                    WarehouseComboBox.SelectedValue = DefaultWarehouseId.Value;
                }
            }

            // Load Warehouses (chỉ Store, không có Central)
            var stores = _warehouseService.GetStores();
            WarehouseComboBox.ItemsSource = stores;
        }

        private void LoadAccountData()
        {
            if (EditedAccount == null) return;

            UsernameTextBox.Text = EditedAccount.Username;
            UsernameTextBox.IsEnabled = false; // Không cho sửa username
            FullNameTextBox.Text = EditedAccount.FullName;
            EmailTextBox.Text = EditedAccount.Email ?? "";
            PhoneNumberTextBox.Text = EditedAccount.PhoneNumber ?? "";
            
            if (EditedAccount.DateOfBirth.HasValue)
            {
                DateOfBirthDatePicker.SelectedDate = new DateTime(
                    EditedAccount.DateOfBirth.Value.Year,
                    EditedAccount.DateOfBirth.Value.Month,
                    EditedAccount.DateOfBirth.Value.Day);
            }

            RoleComboBox.SelectedValue = EditedAccount.RoleId;
            
            if (EditedAccount.WarehouseId.HasValue)
            {
                WarehouseComboBox.SelectedValue = EditedAccount.WarehouseId.Value;
            }

            // Set Status
            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == EditedAccount.Status)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Hiển thị Warehouse dropdown nếu chọn Manager hoặc Staff
            if (RoleComboBox.SelectedItem is Role selectedRole)
            {
                if (selectedRole.RoleName == "Manager" || selectedRole.RoleName == "Staff")
                {
                    
                    WarehouseComboBox.Visibility = Visibility.Visible;
                }
                else // Admin
                {
                    
                    WarehouseComboBox.Visibility = Visibility.Collapsed;
                    WarehouseComboBox.SelectedValue = null;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                Account account;

                if (IsEditMode)
                {
                    account = EditedAccount!;
                }
                else
                {
                    account = new Account();
                }

                // Cập nhật thông tin
                if (!IsEditMode)
                {
                    account.Username = UsernameTextBox.Text.Trim();
                    account.Password = PasswordTextBox.Password;
                }
                else if (!string.IsNullOrWhiteSpace(PasswordTextBox.Password))
                {
                    // Cho phép đổi password khi edit
                    account.Password = PasswordTextBox.Password;
                }

                account.FullName = FullNameTextBox.Text.Trim();
                account.Email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? null : EmailTextBox.Text.Trim();
                
                // Format phone number: loại bỏ khoảng trắng và dấu gạch ngang
                string phoneNumber = PhoneNumberTextBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    account.PhoneNumber = phoneNumber.Replace(" ", "").Replace("-", "");
                }
                else
                {
                    account.PhoneNumber = null;
                }

                if (DateOfBirthDatePicker.SelectedDate.HasValue)
                {
                    account.DateOfBirth = DateOnly.FromDateTime(DateOfBirthDatePicker.SelectedDate.Value);
                }
                else
                {
                    account.DateOfBirth = null;
                }

                account.RoleId = (int)RoleComboBox.SelectedValue!;

                // WarehouseId: NULL cho Admin, có giá trị cho Manager/Staff
                if (RoleComboBox.SelectedItem is Role role && (role.RoleName == "Manager" || role.RoleName == "Staff"))
                {
                    account.WarehouseId = WarehouseComboBox.SelectedValue as int?;
                }
                else
                {
                    account.WarehouseId = null;
                }

                // Status
                if (StatusComboBox.SelectedItem is ComboBoxItem statusItem)
                {
                    account.Status = statusItem.Content.ToString();
                }

                if (IsEditMode)
                {
                    _accountService.UpdateAccount(account);
                    MessageBox.Show("Cập nhật tài khoản thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _accountService.CreateAccount(account);
                    MessageBox.Show("Tạo tài khoản thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            // ========== VALIDATE USERNAME ==========
            if (!IsEditMode)
            {
                string username = UsernameTextBox.Text.Trim();
                
                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Vui lòng nhập Username!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    UsernameTextBox.Focus();
                    return false;
                }

                // Username: 3-50 ký tự, chỉ chữ, số, gạch dưới, không có khoảng trắng
                if (username.Length < 3 || username.Length > 50)
                {
                    MessageBox.Show("Username phải có từ 3 đến 50 ký tự!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    UsernameTextBox.Focus();
                    return false;
                }

                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                {
                    MessageBox.Show("Username chỉ được chứa chữ cái, số và dấu gạch dưới (_)!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    UsernameTextBox.Focus();
                    return false;
                }

                // Kiểm tra username đã tồn tại chưa
                var existing = _accountService.GetByUsername(username);
                if (existing != null)
                {
                    MessageBox.Show("Username đã tồn tại! Vui lòng chọn username khác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    UsernameTextBox.Focus();
                    return false;
                }
            }

            // ========== VALIDATE PASSWORD ==========
            string password = PasswordTextBox.Password;
            
            if (!IsEditMode)
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Vui lòng nhập Password!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordTextBox.Focus();
                    return false;
                }
            }

            // Nếu có nhập password (tạo mới hoặc đổi password khi edit)
            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length < 6 || password.Length > 50)
                {
                    MessageBox.Show("Password phải có từ 6 đến 50 ký tự!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordTextBox.Focus();
                    return false;
                }

                // Password không được chứa khoảng trắng
                if (password.Contains(" "))
                {
                    MessageBox.Show("Password không được chứa khoảng trắng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordTextBox.Focus();
                    return false;
                }
            }

            // ========== VALIDATE FULL NAME ==========
            string fullName = FullNameTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Vui lòng nhập Họ tên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                FullNameTextBox.Focus();
                return false;
            }

            if (fullName.Length < 2 || fullName.Length > 255)
            {
                MessageBox.Show("Họ tên phải có từ 2 đến 255 ký tự!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                FullNameTextBox.Focus();
                return false;
            }

            // Họ tên không được chỉ có số hoặc ký tự đặc biệt
            if (Regex.IsMatch(fullName, @"^[0-9\s\W]+$"))
            {
                MessageBox.Show("Họ tên không hợp lệ! Vui lòng nhập họ tên đầy đủ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                FullNameTextBox.Focus();
                return false;
            }

            // ========== VALIDATE EMAIL ==========
            string email = EmailTextBox.Text.Trim();
            
            if (!string.IsNullOrWhiteSpace(email))
            {
                // Email format validation (cải thiện regex)
                if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    MessageBox.Show("Email không hợp lệ! Ví dụ: example@email.com", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    EmailTextBox.Focus();
                    return false;
                }

                if (email.Length > 255)
                {
                    MessageBox.Show("Email không được vượt quá 255 ký tự!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    EmailTextBox.Focus();
                    return false;
                }
            }

            // ========== VALIDATE PHONE NUMBER ==========
            string phoneNumber = PhoneNumberTextBox.Text.Trim();
            
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                // Chỉ chứa số và có thể có dấu + ở đầu
                string phonePattern = @"^(\+84|0)[0-9]{9,10}$";
                string phoneDigits = phoneNumber.Replace(" ", "").Replace("-", "");
                
                if (!Regex.IsMatch(phoneDigits, phonePattern))
                {
                    MessageBox.Show("Số điện thoại không hợp lệ! Vui lòng nhập số điện thoại Việt Nam (10-11 số).\nVí dụ: 0901234567 hoặc +84901234567", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    PhoneNumberTextBox.Focus();
                    return false;
                }

                if (phoneDigits.Length > 50)
                {
                    MessageBox.Show("Số điện thoại không được vượt quá 50 ký tự!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    PhoneNumberTextBox.Focus();
                    return false;
                }
            }

            // ========== VALIDATE DATE OF BIRTH ==========
            if (DateOfBirthDatePicker.SelectedDate.HasValue)
            {
                DateTime selectedDate = DateOfBirthDatePicker.SelectedDate.Value;
                DateTime today = DateTime.Today;
                int age = today.Year - selectedDate.Year;
                
                // Điều chỉnh tuổi nếu chưa đến sinh nhật trong năm nay
                if (selectedDate.Date > today.AddYears(-age))
                    age--;

                // Không được là tương lai
                if (selectedDate > today)
                {
                    MessageBox.Show("Ngày sinh không được là tương lai!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    DateOfBirthDatePicker.Focus();
                    return false;
                }

                // Tuổi hợp lý: ít nhất 16 tuổi, không quá 100 tuổi
                if (age < 16)
                {
                    MessageBox.Show("Người dùng phải ít nhất 16 tuổi!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    DateOfBirthDatePicker.Focus();
                    return false;
                }

                if (age > 100)
                {
                    MessageBox.Show("Ngày sinh không hợp lệ! Tuổi không được vượt quá 100.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    DateOfBirthDatePicker.Focus();
                    return false;
                }
            }

            // ========== VALIDATE ROLE ==========
            if (RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn Vai trò!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                RoleComboBox.Focus();
                return false;
            }

            // ⭐ QUAN TRỌNG: Không cho phép tạo thêm Admin
            if (!IsEditMode && RoleComboBox.SelectedItem is Role selectedRole && selectedRole.RoleName == "Admin")
            {
                MessageBox.Show("Hệ thống chỉ cho phép có 1 Admin duy nhất!\nKhông thể tạo thêm tài khoản Admin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                RoleComboBox.Focus();
                return false;
            }

            // ⭐ Kiểm tra nếu đang edit và cố gắng đổi thành Admin (nếu tài khoản hiện tại không phải Admin)
            if (IsEditMode && RoleComboBox.SelectedItem is Role newRole && newRole.RoleName == "Admin")
            {
                // Kiểm tra xem tài khoản hiện tại có phải Admin không
                if (EditedAccount != null && EditedAccount.RoleId != newRole.RoleId)
                {
                    // Đang cố gắng đổi role thành Admin
                    var adminAccounts = _accountService.GetAccountsByRole(newRole.RoleId);
                    if (adminAccounts.Count > 0)
                    {
                        MessageBox.Show("Hệ thống chỉ cho phép có 1 Admin duy nhất!\nKhông thể đổi vai trò thành Admin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        RoleComboBox.Focus();
                        return false;
                    }
                }
            }

            // ========== VALIDATE WAREHOUSE (nếu là Manager hoặc Staff) ==========
            if (RoleComboBox.SelectedItem is Role role && (role.RoleName == "Manager" || role.RoleName == "Staff"))
            {
                if (WarehouseComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn Store cho " + role.RoleName + "!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    WarehouseComboBox.Focus();
                    return false;
                }
            }

            // ========== VALIDATE STATUS ==========
            // Khi tạo mới: mặc định là "Active" (đã set trong Window_Loaded)
            // Khi edit: phải chọn trạng thái
            if (IsEditMode && StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn Trạng thái!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusComboBox.Focus();
                return false;
            }
            
            // Đảm bảo có trạng thái được chọn (mặc định là "Active" khi tạo mới)
            if (StatusComboBox.SelectedItem == null)
            {
                // Nếu không có gì được chọn, set mặc định là "Active"
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content.ToString() == "Active")
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}

