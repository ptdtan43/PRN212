using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace SupermarketManager1.Duy
{
    public partial class DetailWindow : Window
    {
        public Product? EditedOne { set; get; }
        private ProductService _productService = new();
        private CategoryService _categoryService = new();

        public DetailWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CategoryComboBox.ItemsSource = _categoryService.GetAllCategories();
            CategoryComboBox.SelectedValuePath = "CategoryId";
            CategoryComboBox.DisplayMemberPath = "CategoryName";

            if (EditedOne != null)
            {
                DetailWindowModeLabel.Text = "Edit Product Information";
                ProductIdTextBox.IsReadOnly = true; // Không cho sửa ProductCode khi edit
                ProductIdTextBox.IsEnabled = false;
                ProductIdTextBox.Text = EditedOne.ProductCode;
                ProductNameTextBox.Text = EditedOne.NameP;
                CategoryComboBox.SelectedValue = EditedOne.CateId;
                ProductPriceTextBox.Text = EditedOne.Price?.ToString() ?? "0";
                ProductSupplierNameTextBox.Text = EditedOne.SupplierName;
                ProductWarrantyTextBox.Text = EditedOne.Warranty;
                ProductDescriptionTextBox.Text = EditedOne.Description;
                if (EditedOne.PublicationDay.HasValue)
                {
                    ProductPublicationDayDatePicker.SelectedDate = new DateTime(EditedOne.PublicationDay.Value.Year, 
                                                                                EditedOne.PublicationDay.Value.Month, 
                                                                                EditedOne.PublicationDay.Value.Day);
                }
                else
                {
                    ProductPublicationDayDatePicker.SelectedDate = null;
                }
            }
            else
            {
                DetailWindowModeLabel.Text = "Add New Product";
                ProductIdTextBox.IsReadOnly = false; // Cho phép nhập khi tạo mới
                ProductIdTextBox.IsEnabled = true;
                ProductIdTextBox.Clear();
                ProductIdTextBox.Focus(); // Tự động focus vào ProductIdTextBox
            }
        }

        private void DetailWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckValidate())
                return;

            DateOnly? publicationDay = null;
            if (ProductPublicationDayDatePicker.SelectedDate.HasValue)
            {
                var selectedDate = ProductPublicationDayDatePicker.SelectedDate.Value;
                publicationDay = new DateOnly(selectedDate.Year, selectedDate.Month, selectedDate.Day);
            }

            decimal? price = null;
            if (decimal.TryParse(ProductPriceTextBox.Text, out decimal parsedPrice))
            {
                price = parsedPrice;
            }

            Product obj = new()
            {
                ProductCode = ProductIdTextBox.Text ?? "",
                NameP = ProductNameTextBox.Text ?? "",
                CateId = (int?)CategoryComboBox.SelectedValue,
                Price = price,
                SupplierName = string.IsNullOrWhiteSpace(ProductSupplierNameTextBox.Text) ? null : ProductSupplierNameTextBox.Text,
                Warranty = string.IsNullOrWhiteSpace(ProductWarrantyTextBox.Text) ? null : ProductWarrantyTextBox.Text,
                Description = string.IsNullOrWhiteSpace(ProductDescriptionTextBox.Text) ? null : ProductDescriptionTextBox.Text,
                PublicationDay = publicationDay
            };

            if (EditedOne == null)
                _productService.AddProduct(obj);
            else
                _productService.UpdateProduct(obj);

            this.Close();
        }

        public bool CheckValidate()
        {
            if (string.IsNullOrWhiteSpace(ProductIdTextBox.Text))
            {
                MessageBox.Show("Please enter Product Code!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProductIdTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                MessageBox.Show("Please enter Product Name!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProductNameTextBox.Focus();
                return false;
            }

            if (ProductNameTextBox.Text.Length < 3 || ProductNameTextBox.Text.Length > 100)
            {
                MessageBox.Show("Product Name must be between 3 and 100 characters!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProductNameTextBox.Focus();
                return false;
            }

            if (Regex.IsMatch(ProductNameTextBox.Text, @"[\$\%\^\@]"))
            {
                MessageBox.Show("Product Name cannot contain special characters such as $, %, ^, @.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProductNameTextBox.Focus();
                return false;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select Category!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CategoryComboBox.Focus();
                return false;
            }

            if (!decimal.TryParse(ProductPriceTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Price must be a number greater than or equal to 0!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProductPriceTextBox.Focus();
                return false;
            }

            // Validate Publication Day - không được hơn realtime
            if (ProductPublicationDayDatePicker.SelectedDate.HasValue)
            {
                DateTime selectedDate = ProductPublicationDayDatePicker.SelectedDate.Value.Date;
                DateTime today = DateTime.Today;
                
                if (selectedDate > today)
                {
                    MessageBox.Show("Publication Date cannot be in the future!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ProductPublicationDayDatePicker.Focus();
                    return false;
                }
            }

            // Validate Product Code - không được trùng khi tạo mới
            if (EditedOne == null)
            {
                string productCode = ProductIdTextBox.Text.Trim();
                var existingProduct = _productService.GetProductByCode(productCode);
                if (existingProduct != null)
                {
                    MessageBox.Show($"Product Code '{productCode}' already exists! Please use a different code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ProductIdTextBox.Focus();
                    ProductIdTextBox.SelectAll();
                    return false;
                }
            }

            return true;
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult answer = MessageBox.Show("Are you sure you want to exit?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}