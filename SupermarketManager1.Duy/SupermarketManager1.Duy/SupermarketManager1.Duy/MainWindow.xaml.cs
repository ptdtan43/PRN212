using SupperMarket.BLL.Service;
using SupperMarket.DAL.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SupermarketManager1.Duy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ProductService _service = new();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillDataGrid(_service.GetAllProducts());
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            DetailWindow detail = new();
            detail.ShowDialog();
            FillDataGrid(_service.GetAllProducts());
        }



        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult answer = MessageBox.Show("Are you sure?", "Comfrim?", MessageBoxButton.YesNo);
            if (answer == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        public void FillDataGrid(List<Product> data)
        {
            ProductListDataGrid.ItemsSource = null;
            ProductListDataGrid.ItemsSource = data;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Product selected = ProductListDataGrid.SelectedItem as Product;
            if (selected == null)
            {
                MessageBox.Show("Please choose a line to select", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBoxResult answer = MessageBox.Show("Are you sure?", "Comfirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.No)
            {
                return;
            }
            _service.DeleteProduct(selected);
            FillDataGrid(_service.GetAllProducts());
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Product selected = ProductListDataGrid.SelectedItem as Product;
            if (selected == null)
            {
                MessageBox.Show("Please selected before updating", "Select one", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DetailWindow detail = new();
            detail.EditedOne = selected;
            detail.ShowDialog();

            FillDataGrid(_service.GetAllProducts());
        }
        private void ManageWarehouseButton_Click(object sender, RoutedEventArgs e)
        {
            WarehouseManagementWindow warehouseWindow = new WarehouseManagementWindow();
            warehouseWindow.ShowDialog();
        }

        private void ManageStoreButton_Click(object sender, RoutedEventArgs e)
        {
            StoreManagerWindow sm = new();
            sm.ShowDialog();
        }
    }
}