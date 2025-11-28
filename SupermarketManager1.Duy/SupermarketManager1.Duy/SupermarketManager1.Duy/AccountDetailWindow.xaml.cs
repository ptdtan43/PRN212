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
            
            // Manager chỉ được chọn Staff, không được chọn Manager hoặc Admin
            // (vì 1 cửa hàng đã có 1 Manager rồi, không thể có Manager thứ 2)
            List<Role> availableRoles;
            if (CurrentUser.IsManager)
            {
                // Manager: chỉ được tạo/sửa Staff (vì store đã có 1 Manager rồi)
                availableRoles = allRoles.Where(r => r.RoleName == "Staff").ToList();
            }
            else if (CurrentUser.IsAdmin)
            {
                // Admin: 
                // - Khi create: không thấy Admin (chỉ có 1 Admin)
                // - Khi edit: nếu edit Admin thì thấy tất cả (nhưng sẽ bị disable), nếu edit tài khoản khác thì không thấy Admin
                if (IsEditMode && EditedAccount?.Role?.RoleName == "Admin")
                {
                    // Edit Admin: hiển thị tất cả (nhưng sẽ bị disable trong LoadAccountData)
                    availableRoles = allRoles;
                }
                else
                {
                    // Create hoặc edit tài khoản khác: không cho chọn Admin
                    availableRoles = allRoles.Where(r => r.RoleName != "Admin").ToList();
                }
            }
            else
            {
                // Staff không có quyền tạo/sửa tài khoản
                availableRoles = new List<Role>();
            }
            
            if (IsEditMode)
            {
                RoleComboBox.ItemsSource = availableRoles;
                TitleLabel.Text = "Edit Account Information";
                LoadAccountData();
                // Status sẽ được enable/disable trong LoadAccountData tùy theo tài khoản
            }
            else
            {
                RoleComboBox.ItemsSource = availableRoles;
                
                TitleLabel.Text = "Create New Account";
                
                // Set trạng thái mặc định là "Active" và disable khi tạo mới
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
                    // Chỉ set nếu role đó có trong danh sách available
                    var defaultRole = availableRoles.FirstOrDefault(r => r.RoleId == DefaultRoleId.Value);
                    if (defaultRole != null)
                    {
                        RoleComboBox.SelectedValue = DefaultRoleId.Value;
                    }
                }
                if (DefaultWarehouseId.HasValue)
                {
                    WarehouseComboBox.SelectedValue = DefaultWarehouseId.Value;
                    // ⭐ Nếu Manager tạo account, set cứng Warehouse và disable
                    if (CurrentUser.IsManager)
                    {
                        WarehouseComboBox.IsEnabled = false; // Không cho đổi warehouse
                    }
                }
            }

            // Load Warehouses (chỉ Store, không có Central)
            LoadWarehouses();
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

            // Nếu đang edit tài khoản Admin, khóa Role và Status, chỉ cho sửa thông tin cá nhân
            if (EditedAccount.Role?.RoleName == "Admin")
            {
                RoleComboBox.IsEnabled = false; // Không cho đổi role
                StatusComboBox.IsEnabled = false; // Không cho đổi status
                PasswordTextBox.IsEnabled = false; // Không cho đổi password
                
                // Đảm bảo Status là Active
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content.ToString() == "Active")
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                // Chỉ cho phép sửa: FullName, Email, PhoneNumber, DateOfBirth
                // (Các field khác đã được disable ở trên)
            }
            // Nếu Admin đang edit tài khoản khác (Manager/Staff)
            else if (CurrentUser.IsAdmin)
            {
                // Không cho Admin sửa mật khẩu của tài khoản khác
                PasswordTextBox.IsEnabled = false;
                
                // Status vẫn cho phép sửa (có thể lock/unlock tài khoản)
                StatusComboBox.IsEnabled = true;
            }
        }

        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Hiển thị Warehouse dropdown tùy theo role
            if (RoleComboBox.SelectedItem is Role selectedRole)
            {
                if (selectedRole.RoleName == "Manager")
                {
                    // Khi tạo mới Manager: ẩn Store (vì Manager không bắt buộc phải chọn cửa hàng)
                    // Khi edit Manager: hiển thị Store (để có thể xem/sửa cửa hàng)
                    if (IsEditMode)
                    {
                        WarehouseComboBox.Visibility = Visibility.Visible;
                        LoadWarehouses();
                    }
                    else
                    {
                        // Tạo mới Manager: ẩn Store
                        WarehouseComboBox.Visibility = Visibility.Collapsed;
                        WarehouseComboBox.SelectedValue = null;
                    }
                }
                else if (selectedRole.RoleName == "Staff")
                {
                    // Staff luôn phải chọn cửa hàng
                    WarehouseComboBox.Visibility = Visibility.Visible;
                    LoadWarehouses();
                }
                else // Admin
                {
                    WarehouseComboBox.Visibility = Visibility.Collapsed;
                    WarehouseComboBox.SelectedValue = null;
                }
            }
        }

        private void LoadWarehouses()
        {
            // Kiểm tra role hiện tại
            if (RoleComboBox.SelectedItem is Role selectedRole)
            {
                if (selectedRole.RoleName == "Manager")
                {
                    // Nếu là Manager: chỉ hiển thị những cửa hàng chưa có manager
                    var allStores = _warehouseService.GetStores();

                    // Nếu đang edit Manager và Manager đã có cửa hàng, hiển thị tất cả (để có thể xem cửa hàng hiện tại)
                    if (IsEditMode && EditedAccount != null && EditedAccount.WarehouseId.HasValue)
                    {
                        // Hiển thị tất cả cửa hàng, nhưng loại trừ những cửa hàng đã có manager khác
                        var availableStores = allStores
                            .Where(s => !s.ManagerId.HasValue || s.ManagerId.Value == EditedAccount.AccountId)
                            .ToList();
                        WarehouseComboBox.ItemsSource = availableStores;
                    }
                    else
                    {
                        // Chỉ hiển thị những cửa hàng chưa có manager (hoặc có thể để trống)
                        var availableStores = allStores
                            .Where(s => !s.ManagerId.HasValue)
                            .ToList();
                        WarehouseComboBox.ItemsSource = availableStores;
                    }
                }
                else if (selectedRole.RoleName == "Staff")
                {
                    // Nếu là Staff: hiển thị tất cả cửa hàng
                    var stores = _warehouseService.GetStores();
                    WarehouseComboBox.ItemsSource = stores;
                }
            }
            else
            {
                // Nếu chưa chọn role, hiển thị tất cả cửa hàng
                var stores = _warehouseService.GetStores();
                WarehouseComboBox.ItemsSource = stores;
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
                bool isEditingAdmin = IsEditMode && EditedAccount?.Role?.RoleName == "Admin";
                bool isAdminEditingOther = IsEditMode && CurrentUser.IsAdmin && !isEditingAdmin;
                
                if (!IsEditMode)
                {
                    account.Username = UsernameTextBox.Text.Trim();
                    account.Password = PasswordTextBox.Password;
                }
                else if (!isEditingAdmin && !isAdminEditingOther && !string.IsNullOrWhiteSpace(PasswordTextBox.Password))
                {
                    // Cho phép đổi password khi edit (trừ Admin edit Admin và Admin edit tài khoản khác)
                    account.Password = PasswordTextBox.Password;
                }
                // Nếu Admin edit Admin hoặc Admin edit tài khoản khác, không đổi password (giữ nguyên password cũ)

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

                // Nếu đang edit Admin, giữ nguyên RoleId và Status
                if (isEditingAdmin)
                {
                    // Giữ nguyên RoleId và Status của Admin
                    account.RoleId = EditedAccount!.RoleId;
                    account.Status = "Active"; // Luôn là Active
                    account.WarehouseId = null; // Admin không có Warehouse
                }
                else
                {
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
                }

                if (IsEditMode)
                {
                    _accountService.UpdateAccount(account);
                    MessageBox.Show("Account updated successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _accountService.CreateAccount(account);
                    MessageBox.Show("Account created successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", 
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
                    MessageBox.Show("Please enter Username!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    UsernameTextBox.Focus();
                    return false;
                }

                // Username: 3-50 ký tự, chỉ chữ, số, gạch dưới, không có khoảng trắng
                if (username.Length < 3 || username.Length > 50)
                {
                    MessageBox.Show("Username must be between 3 and 50 characters!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    UsernameTextBox.Focus();
                    return false;
                }

                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                {
                    MessageBox.Show("Username can only contain letters, numbers and underscore (_)!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    UsernameTextBox.Focus();
                    return false;
                }

                // Kiểm tra username đã tồn tại chưa
                var existing = _accountService.GetByUsername(username);
                if (existing != null)
                {
                    MessageBox.Show("Username already exists! Please choose another username.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Please enter Password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordTextBox.Focus();
                    return false;
                }
            }

            // Nếu có nhập password (tạo mới hoặc đổi password khi edit)
            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length < 6 || password.Length > 50)
                {
                    MessageBox.Show("Password must be between 6 and 50 characters!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordTextBox.Focus();
                    return false;
                }

                // Password không được chứa khoảng trắng
                if (password.Contains(" "))
                {
                    MessageBox.Show("Password cannot contain spaces!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordTextBox.Focus();
                    return false;
                }
            }

            // ========== VALIDATE FULL NAME ==========
            string fullName = FullNameTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Please enter Full Name!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                FullNameTextBox.Focus();
                return false;
            }

            if (fullName.Length < 2 || fullName.Length > 255)
            {
                MessageBox.Show("Full Name must be between 2 and 255 characters!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                FullNameTextBox.Focus();
                return false;
            }

            // Họ tên không được chỉ có số hoặc ký tự đặc biệt
            if (Regex.IsMatch(fullName, @"^[0-9\s\W]+$"))
            {
                MessageBox.Show("Invalid Full Name! Please enter a valid full name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Invalid email! Example: example@email.com", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    EmailTextBox.Focus();
                    return false;
                }

                if (email.Length > 255)
                {
                    MessageBox.Show("Email cannot exceed 255 characters!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Invalid phone number! Please enter a Vietnamese phone number (10-11 digits).\nExample: 0901234567 or +84901234567", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    PhoneNumberTextBox.Focus();
                    return false;
                }

                if (phoneDigits.Length > 50)
                {
                    MessageBox.Show("Phone number cannot exceed 50 characters!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Date of birth cannot be in the future!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    DateOfBirthDatePicker.Focus();
                    return false;
                }

                // Tuổi hợp lý: ít nhất 16 tuổi, không quá 100 tuổi
                if (age < 16)
                {
                    MessageBox.Show("User must be at least 16 years old!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    DateOfBirthDatePicker.Focus();
                    return false;
                }

                if (age > 100)
                {
                    MessageBox.Show("Invalid date of birth! Age cannot exceed 100.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    DateOfBirthDatePicker.Focus();
                    return false;
                }
            }

            // ========== VALIDATE ROLE ==========
            if (RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select Role!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RoleComboBox.Focus();
                return false;
            }

            // Không cho phép tạo thêm Admin
            if (!IsEditMode && RoleComboBox.SelectedItem is Role selectedRole && selectedRole.RoleName == "Admin")
            {
                MessageBox.Show("System only allows 1 Admin!\nCannot create additional Admin accounts.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RoleComboBox.Focus();
                return false;
            }

            // Kiểm tra nếu đang edit và cố gắng đổi thành Admin (nếu tài khoản hiện tại không phải Admin)
            if (IsEditMode && RoleComboBox.SelectedItem is Role newRole && newRole.RoleName == "Admin")
            {
                // Kiểm tra xem tài khoản hiện tại có phải Admin không
                if (EditedAccount != null && EditedAccount.RoleId != newRole.RoleId)
                {
                    // Đang cố gắng đổi role thành Admin - KHÔNG CHO PHÉP (chỉ có 1 Admin duy nhất)
                    MessageBox.Show("System only allows 1 Admin!\nCannot change role to Admin.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    RoleComboBox.Focus();
                    return false;
                }
            }

            // ========== VALIDATE WAREHOUSE (nếu là Manager hoặc Staff) ==========
            if (RoleComboBox.SelectedItem is Role role && (role.RoleName == "Manager" || role.RoleName == "Staff"))
            {
                // ⭐ Manager không bắt buộc phải chọn cửa hàng khi tạo mới (có thể tạo manager chưa gán cửa hàng)
                // Staff thì bắt buộc phải chọn cửa hàng
                if (role.RoleName == "Staff" && WarehouseComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select Store for Staff!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    WarehouseComboBox.Focus();
                    return false;
                }

                // Kiểm tra nếu role là Manager và Store đã có Manager khác
                if (role.RoleName == "Manager" && WarehouseComboBox.SelectedValue is int selectedWarehouseId)
                {
                    // Kiểm tra xem Store mới đã có Manager chưa
                    var selectedWarehouse = _warehouseService.GetWarehouseById(selectedWarehouseId);
                    if (selectedWarehouse != null && selectedWarehouse.ManagerId.HasValue)
                    {
                        // Nếu đang edit và Manager hiện tại đang quản lý Store này, cho phép
                        bool isCurrentManager = IsEditMode && EditedAccount != null && 
                                               EditedAccount.AccountId == selectedWarehouse.ManagerId.Value;
                        
                        // Nếu Store đã có Manager khác (không phải Manager hiện tại đang edit)
                        if (!isCurrentManager)
                        {
                            var existingManager = _accountService.GetAccountById(selectedWarehouse.ManagerId.Value);
                            string managerName = existingManager?.FullName ?? "Unknown";
                            MessageBox.Show(
                                $"Store '{selectedWarehouse.WarehouseName}' already has a manager: {managerName}!\n\nEach Store can only have 1 Manager. Please select another Store.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            WarehouseComboBox.Focus();
                            return false;
                        }
                    }
                }
            }

            // ========== VALIDATE STATUS ==========
            // Khi tạo mới: mặc định là "Active" (đã set trong Window_Loaded)
            // Khi edit: phải chọn trạng thái
            if (IsEditMode && StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select Status!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

