using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SupermarketManager1.Duy
{
    public partial class WarehouseDetailWindow : Window
    {
        private WarehouseService _warehouseService = new();
        private AccountService _accountService = new();

        public Warehouse? EditedWarehouse { get; set; }
        public bool IsEditMode => EditedWarehouse != null;

        public WarehouseDetailWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Kiểm tra null
            if (ManagerComboBox == null || TypeComboBox == null)
                return;

            // Load danh sách Manager (chỉ những account có Role = Manager)
            var managers = _accountService.GetAccountsByRole(2); // RoleId = 2 là Manager
            ManagerComboBox.ItemsSource = managers;

            if (IsEditMode)
            {
                TitleLabel.Text = "Edit Warehouse/Store Information";
                LoadWarehouseData();
            }
            else
            {
                TitleLabel.Text = "Create New Warehouse/Store";
                // Khi tạo mới, nếu mặc định chọn "Cửa hàng" thì hiển thị ManagerComboBox
                if (TypeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string type && type == "Store")
                {
                    ManagerComboBox.Visibility = Visibility.Visible;
                }
            }
        }

        private void LoadWarehouseData()
        {
            if (EditedWarehouse == null) return;

            WarehouseNameTextBox.Text = EditedWarehouse.WarehouseName;
            AddressTextBox.Text = EditedWarehouse.Address ?? "";

            // Set Type
            foreach (ComboBoxItem item in TypeComboBox.Items)
            {
                if (item.Tag.ToString() == EditedWarehouse.Type)
                {
                    TypeComboBox.SelectedItem = item;
                    break;
                }
            }

            // Set Manager
            if (EditedWarehouse.ManagerId.HasValue)
            {
                ManagerComboBox.SelectedValue = EditedWarehouse.ManagerId.Value;
            }

            // Nếu đang edit Central Warehouse, khóa Type và Manager, chỉ cho sửa Name và Address
            if (EditedWarehouse.Type == "Central")
            {
                TypeComboBox.IsEnabled = false; // Không cho đổi type
                ManagerComboBox.Visibility = Visibility.Collapsed; // Ẩn Manager (Central không có Manager)
                // Chỉ cho phép sửa: WarehouseName và Address
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra null để tránh NullReferenceException
            if (TypeComboBox == null || ManagerComboBox == null)
                return;

            // Hiển thị Manager dropdown chỉ khi chọn Store
            if (TypeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string type)
            {
                if (type == "Store")
                {
                    // ManagerLabel.Visibility = Visibility.Visible; // Không có trong XAML
                    ManagerComboBox.Visibility = Visibility.Visible;
                }
                else // Central
                {
                    // ManagerLabel.Visibility = Visibility.Collapsed; // Không có trong XAML
                    ManagerComboBox.Visibility = Visibility.Collapsed;
                    ManagerComboBox.SelectedValue = null;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                Warehouse warehouse;

                if (IsEditMode)
                {
                    warehouse = EditedWarehouse!;
                }
                else
                {
                    warehouse = new Warehouse();
                }

                // Cập nhật thông tin
                warehouse.WarehouseName = WarehouseNameTextBox.Text.Trim();
                warehouse.Address = string.IsNullOrWhiteSpace(AddressTextBox.Text) ? null : AddressTextBox.Text.Trim();

                // ⭐ Nếu đang edit Central Warehouse, giữ nguyên Type
                bool isEditingCentral = IsEditMode && EditedWarehouse?.Type == "Central";
                
                if (isEditingCentral)
                {
                    // Giữ nguyên Type của Central Warehouse
                    warehouse.Type = EditedWarehouse!.Type;
                    warehouse.ManagerId = null; // Central không có Manager
                }
                else
                {
                    // Type
                    if (TypeComboBox.SelectedItem is ComboBoxItem typeItem && typeItem.Tag is string type)
                    {
                        warehouse.Type = type;
                    }

                    // ManagerId: NULL cho Central, có giá trị cho Store
                    if (warehouse.Type == "Store")
                    {
                        warehouse.ManagerId = ManagerComboBox.SelectedValue as int?;
                    }
                    else // Central
                    {
                        warehouse.ManagerId = null;
                    }
                }

                if (IsEditMode)
                {
                    _warehouseService.UpdateWarehouse(warehouse);
                    MessageBox.Show("Warehouse updated successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _warehouseService.CreateWarehouse(warehouse);
                    MessageBox.Show("Warehouse created successfully!", "Success", 
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
            // Warehouse Name
            if (string.IsNullOrWhiteSpace(WarehouseNameTextBox.Text))
            {
                MessageBox.Show("Please enter Warehouse/Store Name!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WarehouseNameTextBox.Focus();
                return false;
            }

            // Type
            if (TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select Type!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Manager (nếu là Store)
            if (TypeComboBox.SelectedItem is ComboBoxItem typeItem && typeItem.Tag is string type)
            {
                if (type == "Store" && ManagerComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select Manager for Store!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
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

