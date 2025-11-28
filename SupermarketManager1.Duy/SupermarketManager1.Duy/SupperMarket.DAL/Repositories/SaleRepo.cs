using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.DAL.Repositories
{
    public class SaleRepo
    {
        private SupermarketDb3Context? _ctx;

        // Tạo Sale mới
        public void Create(Sale sale)
        {
            _ctx = new();
            
            // Vì bảng Sales có trigger, EF Core không thể dùng OUTPUT clause
            // Dùng raw SQL với SqlParameter để insert trực tiếp, không qua EF Core SaveChanges
            var connection = _ctx.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            
            try
            {
                if (!wasOpen)
                {
                    connection.Open();
                }
                
                // Dùng transaction để đảm bảo atomicity với trigger
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            
                            // Tạo parameters - phải cast về DbParameter
                            DbParameter param1 = new SqlParameter("@AccountId", sale.AccountId);
                            DbParameter param2 = new SqlParameter("@WarehouseId", sale.WarehouseId);
                            DbParameter param3 = new SqlParameter("@ProductCode", sale.ProductCode ?? (object)DBNull.Value);
                            DbParameter param4 = new SqlParameter("@QuantitySold", sale.QuantitySold);
                            DbParameter param5 = new SqlParameter("@UnitPrice", sale.UnitPrice);
                            DbParameter param6 = new SqlParameter("@TotalAmount", sale.TotalAmount);
                            DbParameter param7 = new SqlParameter("@SaleDate", sale.SaleDate);
                            
                            command.Parameters.Add(param1);
                            command.Parameters.Add(param2);
                            command.Parameters.Add(param3);
                            command.Parameters.Add(param4);
                            command.Parameters.Add(param5);
                            command.Parameters.Add(param6);
                            command.Parameters.Add(param7);
                            
                            // Execute raw SQL (không dùng OUTPUT clause để tránh conflict với trigger)
                            // Trigger sẽ tự động trừ tồn kho và rollback nếu không đủ hàng
                            command.CommandText = @"
                                INSERT INTO Sales (AccountId, WarehouseId, ProductCode, QuantitySold, UnitPrice, TotalAmount, SaleDate)
                                VALUES (@AccountId, @WarehouseId, @ProductCode, @QuantitySold, @UnitPrice, @TotalAmount, @SaleDate);
                                SELECT CAST(SCOPE_IDENTITY() as int);";
                            
                            // Execute và lấy SaleId
                            var result = command.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                if (int.TryParse(result.ToString(), out int saleId))
                                {
                                    sale.SaleId = saleId;
                                }
                            }
                            
                            // Commit transaction nếu thành công
                            transaction.Commit();
                        }
                    }
                    catch
                    {
                        // Rollback nếu có lỗi (trigger sẽ rollback nếu không đủ hàng)
                        transaction.Rollback();
                        throw; // Re-throw để SaleService có thể catch và xử lý
                    }
                }
            }
            finally
            {
                // Chỉ đóng connection nếu chúng ta đã mở nó
                if (!wasOpen && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        // Lấy tất cả Sales
        public List<Sale> GetAll()
        {
            _ctx = new();
            return _ctx.Sales
                .Include(s => s.Account)
                .Include(s => s.Warehouse)
                .Include(s => s.ProductCodeNavigation)
                .OrderByDescending(s => s.SaleDate)
                .ToList();
        }

        // Lấy Sales theo Warehouse
        public List<Sale> GetByWarehouse(int warehouseId)
        {
            _ctx = new();
            return _ctx.Sales
                .Include(s => s.Account)
                .Include(s => s.Warehouse)
                .Include(s => s.ProductCodeNavigation)
                .Where(s => s.WarehouseId == warehouseId)
                .OrderByDescending(s => s.SaleDate)
                .ToList();
        }

        // Lấy Sales theo Account (Staff)
        public List<Sale> GetByAccount(int accountId)
        {
            _ctx = new();
            return _ctx.Sales
                .Include(s => s.Account)
                .Include(s => s.Warehouse)
                .Include(s => s.ProductCodeNavigation)
                .Where(s => s.AccountId == accountId)
                .OrderByDescending(s => s.SaleDate)
                .ToList();
        }

        // Lấy Sales theo ngày
        public List<Sale> GetByDate(DateTime date)
        {
            _ctx = new();
            return _ctx.Sales
                .Include(s => s.Account)
                .Include(s => s.Warehouse)
                .Include(s => s.ProductCodeNavigation)
                .Where(s => s.SaleDate.Date == date.Date)
                .OrderByDescending(s => s.SaleDate)
                .ToList();
        }

        // Lấy Sales theo khoảng thời gian
        public List<Sale> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            _ctx = new();
            return _ctx.Sales
                .Include(s => s.Account)
                .Include(s => s.Warehouse)
                .Include(s => s.ProductCodeNavigation)
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                .OrderByDescending(s => s.SaleDate)
                .ToList();
        }
    }



}
