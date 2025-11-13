using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for StoreManagerWindow.xaml
    /// </summary>
    public partial class StoreManagerWindow : Window
    {
        public StoreManagerWindow()
        {
            InitializeComponent();
        }

        private WarehouseService _ws = new();

        private void FillData(List<Warehouse> data)
        {
            StoreGrid.ItemsSource = null;
            StoreGrid.ItemsSource = data;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillData(_ws.GetAllWarehouses());
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            DetailStoreManagerWindow dsm = new();
            dsm.ShowDialog();
            FillData(_ws.GetAllWarehouses());
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Warehouse? selected = StoreGrid.SelectedItem as Warehouse;

            if (selected == null)
            {
                MessageBox.Show("Please select a row before updating", "Select one", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DetailStoreManagerWindow dsm = new();
            dsm.EditedOne = selected;
            dsm.ShowDialog();
            FillData(_ws.GetAllWarehouses());
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Warehouse? selected = StoreGrid.SelectedItem as Warehouse;

            if (selected == null)
            {
                MessageBox.Show("Please select a row before deleting", "Select one", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult ans = MessageBox.Show("Are you sure ?", "Conform", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ans == MessageBoxResult.No)
            {
                return;
            }
            _ws.DeleteStore(selected);
            FillData(_ws.GetAllWarehouses());
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
