using ClosedXML.Excel;
using SupperMarket.DAL.Models;
using SupperMarket.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.BLL.Service
{
    public class SaleService
    {
        private SaleRepo _repo = new();
        private InventoryService _inventoryService = new();

        // Tạo Sale mới (bán hàng)
        public bool CreateSale(int accountId, int warehouseId, string productCode, int quantity, out string errorMessage)
        {
            errorMessage = "";

            // Kiểm tra tồn kho
            int currentStock = _inventoryService.GetStock(warehouseId, productCode);
            if (currentStock < quantity)
            {
                errorMessage = $"Không đủ hàng! Tồn kho hiện tại: {currentStock}, Số lượng yêu cầu: {quantity}";
                return false;
            }

            // Lấy giá sản phẩm
            var productService = new ProductService();
            var product = productService.GetAllProducts().FirstOrDefault(p => p.ProductCode == productCode);
            if (product == null)
            {
                errorMessage = "Không tìm thấy sản phẩm!";
                return false;
            }

            decimal unitPrice = product.Price ?? 0;
            decimal totalAmount = unitPrice * quantity;

            // Tạo Sale
            Sale sale = new Sale
            {
                AccountId = accountId,
                WarehouseId = warehouseId,
                ProductCode = productCode,
                QuantitySold = quantity,
                UnitPrice = unitPrice,
                TotalAmount = totalAmount,
                SaleDate = DateTime.Now
            };

            try
            {
                _repo.Create(sale);
                // Trigger sẽ tự động trừ tồn kho
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi khi tạo đơn bán: {ex.Message}";
                return false;
            }
        }

        // Lấy tất cả Sales
        public List<Sale> GetAllSales()
        {
            return _repo.GetAll();
        }

        // Lấy Sales theo Warehouse
        public List<Sale> GetSalesByWarehouse(int warehouseId)
        {
            return _repo.GetByWarehouse(warehouseId);
        }

        // Lấy Sales theo Account
        public List<Sale> GetSalesByAccount(int accountId)
        {
            return _repo.GetByAccount(accountId);
        }

        // Lấy Sales theo ngày
        public List<Sale> GetSalesByDate(DateTime date)
        {
            return _repo.GetByDate(date);
        }

        // Lấy Sales theo khoảng thời gian
        public List<Sale> GetSalesByDateRange(DateTime startDate, DateTime endDate)
        {
            return _repo.GetByDateRange(startDate, endDate);
        }

        // Tính tổng doanh thu theo Warehouse
        public decimal GetTotalRevenueByWarehouse(int warehouseId)
        {
            var sales = _repo.GetByWarehouse(warehouseId);
            return sales.Sum(s => s.TotalAmount);
        }

        // Tính tổng doanh thu theo ngày
        public decimal GetTotalRevenueByDate(DateTime date)
        {
            var sales = _repo.GetByDate(date);
            return sales.Sum(s => s.TotalAmount);
        }

        public string ExportSalesToExcel(string filePath)
        {
            var sales = _repo.GetAll();
            if (sales == null || !sales.Any())
                throw new Exception("Không có dữ liệu bán hàng để xuất!");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Doanh thu");

                // Header
                worksheet.Cell(1, 1).Value = "Mã hóa đơn";
                worksheet.Cell(1, 2).Value = "Nhân viên";
                worksheet.Cell(1, 3).Value = "Kho hàng";
                worksheet.Cell(1, 4).Value = "Sản phẩm";
                worksheet.Cell(1, 5).Value = "Số lượng";
                worksheet.Cell(1, 6).Value = "Đơn giá";
                worksheet.Cell(1, 7).Value = "Tổng tiền";
                worksheet.Cell(1, 8).Value = "Ngày bán";

                // Ghi dữ liệu
                int row = 2;
                foreach (var s in sales)
                {
                    worksheet.Cell(row, 1).Value = s.SaleId;
                    worksheet.Cell(row, 2).Value = s.Account?.FullName;
                    worksheet.Cell(row, 3).Value = s.Warehouse?.WarehouseName;
                    worksheet.Cell(row, 4).Value = s.ProductCode;
                    worksheet.Cell(row, 5).Value = s.QuantitySold;
                    worksheet.Cell(row, 6).Value = s.UnitPrice;
                    worksheet.Cell(row, 7).Value = s.TotalAmount;
                    worksheet.Cell(row, 8).Value = s.SaleDate.ToString("dd/MM/yyyy HH:mm");
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }

            return filePath;
        }
    }
}
