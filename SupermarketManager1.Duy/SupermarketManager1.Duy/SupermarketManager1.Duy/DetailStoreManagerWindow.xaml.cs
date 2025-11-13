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
using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;

namespace SupermarketManager1.Duy
{
    /// <summary>
    /// Interaction logic for DetailStoreManagerWindow.xaml
    /// </summary>
    public partial class DetailStoreManagerWindow : Window
    {
        public DetailStoreManagerWindow()
        {
            InitializeComponent();
        }

        public Warehouse? EditedOne { get; set; }
        private WarehouseService _ws = new();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckValidation())
                return;

            Warehouse warehouse = new()
            {
                WarehouseId = int.Parse(IdWarehouseTextBox.Text),
                WarehouseName = NameWarehouseTextBox.Text ?? "",
                Type = TypeWarehouseTextBox.Text ?? "",
                Address = AddressWarehouseTextBox.Text
            };

            if (EditedOne == null)
            {
                _ws.CreateStore(warehouse);
            }
            else
            {
                warehouse.WarehouseId = EditedOne.WarehouseId; // Giữ nguyên ID khi update
                _ws.UpdateStore(warehouse);
            }
            
            Close();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if(EditedOne != null)
            {
                DetailLabel.Content = "Update the Warehouse";
                IdWarehouseTextBox.Text = EditedOne.WarehouseId.ToString();
                NameWarehouseTextBox.Text = EditedOne.WarehouseName;
                TypeWarehouseTextBox.Text = EditedOne.Type;
                AddressWarehouseTextBox.Text = EditedOne.Address;
                IdWarehouseTextBox.IsEnabled = false;
            }
            else
            {
                DetailLabel.Content = "Create the Warehouse";
            }
        }

        private bool CheckValidation()
        {
            // 1. CHECK ID (Must be an integer & not empty)
            if (int.TryParse(IdWarehouseTextBox.Text, out int parsedId) == false)
            {
                MessageBox.Show("ID is required and must be a valid integer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IdWarehouseTextBox.Focus(); // Focus on the error field
                return false;
            }

            // (Optional) Check if ID is a positive number
            if (parsedId <= 0)
            {
                MessageBox.Show("ID must be a positive number (greater than 0).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IdWarehouseTextBox.Focus();
                return false;
            }

            // 2. CHECK NAME
            if (string.IsNullOrWhiteSpace(NameWarehouseTextBox.Text))
            {
                MessageBox.Show("Name must not be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NameWarehouseTextBox.Focus();
                return false;
            }

            // 3. CHECK TYPE
            if (string.IsNullOrWhiteSpace(TypeWarehouseTextBox.Text))
            {
                MessageBox.Show("Type must not be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TypeWarehouseTextBox.Focus();
                return false;
            }

            // 4. CHECK ADDRESS
            if (string.IsNullOrWhiteSpace(AddressWarehouseTextBox.Text))
            {
                MessageBox.Show("Address must not be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AddressWarehouseTextBox.Focus();
                return false;
            }

            return true;
        }
    }
}
