using SupperMarket.BLL.Service;
using System.Windows;

namespace SupermarketManager1.Duy
{
    public partial class AdjustQuantityWindow : Window
    {
        private InventoryService _inventoryService = new();

        public string ProductCode { get; set; }
        public int WarehouseId { get; set; }
        public int CurrentQuantity { get; set; }

        public AdjustQuantityWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProductCodeLabel.Text = ProductCode;
            CurrentQuantityLabel.Text = CurrentQuantity.ToString();
            NewQuantityTextBox.Text = CurrentQuantity.ToString();
            NewQuantityTextBox.Focus();
            NewQuantityTextBox.SelectAll();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(NewQuantityTextBox.Text, out int newQuantity) || newQuantity < 0)
            {
                MessageBox.Show("Please enter a valid quantity (>= 0)!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                $"Change quantity from {CurrentQuantity} → {newQuantity}?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                _inventoryService.SetStock(WarehouseId, ProductCode, newQuantity);
                MessageBox.Show("Update successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}