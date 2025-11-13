using Microsoft.Win32;
using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SupermarketManager1.Duy
{
    public partial class ReportWindow : Window
    {
        private SaleService _saleService = new();
        private WarehouseService _warehouseService = new();

        public ReportWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWarehouses();
            LoadSales();
        }

        private void LoadWarehouses()
        {
            // Giữ lại item "Tất cả" đã có trong XAML
            List<Warehouse> warehouses;
            
            // Phân quyền: Admin thấy tất cả, Manager chỉ thấy Store của mình
            if (CurrentUser.IsAdmin)
            {
                warehouses = _warehouseService.GetAllWarehouses();
            }
            else if (CurrentUser.IsManager)
            {
                // Manager chỉ thấy Store của mình
                if (CurrentUser.WarehouseId.HasValue)
                {
                    var managerStore = _warehouseService.GetWarehouseById(CurrentUser.WarehouseId.Value);
                    if (managerStore != null)
                    {
                        warehouses = new List<Warehouse> { managerStore };
                        // Tự động chọn Store của Manager và disable ComboBox
                        WarehouseFilterComboBox.IsEnabled = false;
                    }
                    else
                    {
                        warehouses = new List<Warehouse>();
                    }
                }
                else
                {
                    warehouses = new List<Warehouse>();
                }
            }
            else
            {
                // Staff không có quyền xem báo cáo
                MessageBox.Show("Bạn không có quyền xem báo cáo!", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            foreach (var warehouse in warehouses)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = warehouse.WarehouseName,
                    Tag = warehouse.WarehouseId
                };
                WarehouseFilterComboBox.Items.Add(item);
            }
            
            // Tự động chọn item đầu tiên cho Manager
            if (CurrentUser.IsManager && warehouses.Count > 0)
            {
                WarehouseFilterComboBox.SelectedIndex = 0;
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSales();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSales();
        }

        private void LoadSales()
        {
            // Kiểm tra null để tránh NullReferenceException
            if (WarehouseFilterComboBox == null || FromDatePicker == null || 
                ToDatePicker == null || SalesDataGrid == null || 
                TotalRevenueLabel == null || TotalOrdersLabel == null)
                return;

            List<Sale> sales;

            // Lọc theo Warehouse
            int? warehouseId = null;
            if (WarehouseFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Tag is int id && id > 0)
                {
                    warehouseId = id;
                }
                // Nếu Tag = 0 hoặc null thì warehouseId = null (hiển thị tất cả)
            }

            // Lọc theo ngày
            if (FromDatePicker.SelectedDate.HasValue && ToDatePicker.SelectedDate.HasValue)
            {
                DateTime fromDate = FromDatePicker.SelectedDate.Value.Date;
                DateTime toDate = ToDatePicker.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1); // End of day

                if (warehouseId.HasValue && warehouseId.Value > 0)
                {
                    sales = _saleService.GetSalesByWarehouse(warehouseId.Value)
                        .Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate)
                        .ToList();
                }
                else
                {
                    sales = _saleService.GetSalesByDateRange(fromDate, toDate);
                }
            }
            else if (FromDatePicker.SelectedDate.HasValue)
            {
                DateTime fromDate = FromDatePicker.SelectedDate.Value.Date;
                if (warehouseId.HasValue && warehouseId.Value > 0)
                {
                    sales = _saleService.GetSalesByWarehouse(warehouseId.Value)
                        .Where(s => s.SaleDate >= fromDate)
                        .ToList();
                }
                else
                {
                    sales = _saleService.GetAllSales()
                        .Where(s => s.SaleDate >= fromDate)
                        .ToList();
                }
            }
            else
            {
                if (warehouseId.HasValue && warehouseId.Value > 0)
                {
                    sales = _saleService.GetSalesByWarehouse(warehouseId.Value);
                }
                else
                {
                    sales = _saleService.GetAllSales();
                }
            }

            // Đảm bảo sales không null
            if (sales == null)
            {
                sales = new List<Sale>();
            }

            // Hiển thị
            SalesDataGrid.ItemsSource = null;
            SalesDataGrid.ItemsSource = sales;

            // Tính tổng (kiểm tra null để tránh lỗi)
            decimal totalRevenue = sales?.Sum(s => s?.TotalAmount ?? 0) ?? 0;
            int totalOrders = sales?.Count ?? 0;

            TotalRevenueLabel.Text = $"{totalRevenue:N0} VNĐ";
            TotalOrdersLabel.Text = totalOrders.ToString();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var service = new SaleService();
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Chọn nơi lưu file Excel",
                    FileName = "DoanhThu.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    string filePath = saveDialog.FileName;
                    service.ExportSalesToExcel(filePath);
                    MessageBox.Show("Xuất file Excel thành công!\n" + filePath, "Thành công",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

