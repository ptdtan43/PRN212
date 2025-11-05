using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
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
    /// Interaction logic for DetailWindow.xaml
    /// </summary>
    public partial class DetailWindow : Window
    {
        public Product EditedOne { set; get; }
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
                ProductIdTextBox.Text = EditedOne.ProductCode;
                ProductNameTextBox.Text = EditedOne.NameP;
                CategoryComboBox.SelectedValue = EditedOne.CateId;
                ProductPriceTextBox.Text = EditedOne.Price.ToString();
                ProductSupplierNameTextBox.Text = EditedOne.SupplierName;
                //ProductPublicationDayTextBox.SelectedDate = EditedOne.PublicationDay.HasValue ? new DateTime(EditedOne.PublicationDay.Value.Year, EditedOne.PublicationDay.Value.Month, EditedOne.PublicationDay.Value.Day) : (DateTime?)null;
                ProductQuantityTextBox.Text = EditedOne.Quantity.ToString();
                ProductWarrantyTextBox.Text = EditedOne.Warranty;
                ProductDescriptionTextBox.Text = EditedOne.Description;

            }
            else
            {
                DetailWindowModeLabel.Content = "Add new product";
            }
        }
    }
}
