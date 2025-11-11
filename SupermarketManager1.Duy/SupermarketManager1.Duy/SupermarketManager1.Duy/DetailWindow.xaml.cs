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
                DetailWindowModeLabel.Content = "Edit product details";
                ProductIdTextBox.IsEnabled = false;
                ProductIdTextBox.Text = EditedOne.ProductCode;
                ProductNameTextBox.Text = EditedOne.NameP;
                CategoryComboBox.SelectedValue = EditedOne.CateId;
                ProductPriceTextBox.Text = EditedOne.Price.ToString();
                ProductSupplierNameTextBox.Text = EditedOne.SupplierName;
                // BỎ: ProductQuantityTextBox.Text = EditedOne.Quantity.ToString();
                ProductWarrantyTextBox.Text = EditedOne.Warranty;
                ProductDescriptionTextBox.Text = EditedOne.Description;
                ProductPublicationDayTextBox.Text = EditedOne.PublicationDay?.ToString("yyyy-MM-dd") ?? "";
            }
            else
            {
                DetailWindowModeLabel.Content = "Add new product";
            }
        }

        private void DetailWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckValidate())
                return;

            DateOnly? publicationDay = null;
            if (!string.IsNullOrWhiteSpace(ProductPublicationDayTextBox.Text))
            {
                if (DateOnly.TryParse(ProductPublicationDayTextBox.Text, out DateOnly parsedDate))
                {
                    publicationDay = parsedDate;
                }
            }

            Product obj = new()
            {
                ProductCode = ProductIdTextBox.Text,
                NameP = ProductNameTextBox.Text,
                CateId = (int?)CategoryComboBox.SelectedValue,
                Price = decimal.Parse(ProductPriceTextBox.Text),
                SupplierName = ProductSupplierNameTextBox.Text,
                // BỎ: Quantity = int.Parse(ProductQuantityTextBox.Text),
                Warranty = ProductWarrantyTextBox.Text,
                Description = ProductDescriptionTextBox.Text,
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
                MessageBox.Show("Product ID is required!", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                MessageBox.Show("Product name is required!", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (ProductNameTextBox.Text.Length < 3 || ProductNameTextBox.Text.Length > 100)
            {
                MessageBox.Show("Product name must be between 3 and 100 characters!", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (Regex.IsMatch(ProductNameTextBox.Text, @"[\$\%\^\@]"))
            {
                MessageBox.Show("Product name cannot contain special characters like $, %, ^, @.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Category must be selected!", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(ProductPriceTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Price must be a number greater than or equal to 0!", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // BỎ validation cho Quantity

            return true;
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult answer = MessageBox.Show("Are you sure?", "Confirm?", MessageBoxButton.YesNo);
            if (answer == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}