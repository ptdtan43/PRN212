using Microsoft.EntityFrameworkCore;
using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.DAL.Repositories
{
    public class InventoryRepo
    {
        private SupermarketDb3Context? _ctx;

        // Lấy tất cả inventory của 1 kho
        public List<Inventory> GetByWarehouse(int warehouseId)
        {
            _ctx = new();
            return _ctx.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .Where(i => i.WarehouseId == warehouseId)
                .ToList();
        }

        // Lấy số lượng của 1 sản phẩm trong 1 kho
        public int GetStock(int warehouseId, string productCode)
        {
            _ctx = new();
            var inventory = _ctx.Inventories
                .FirstOrDefault(i => i.WarehouseId == warehouseId && i.ProductCode == productCode);
            return inventory?.Quantity ?? 0;
        }

        // Cập nhật số lượng (dùng cho +/-)
        public void UpdateQuantity(int warehouseId, string productCode, int quantityChange)
        {
            _ctx = new();
            var inventory = _ctx.Inventories
                .FirstOrDefault(i => i.WarehouseId == warehouseId && i.ProductCode == productCode);

            if (inventory != null)
            {
                inventory.Quantity += quantityChange;
                if (inventory.Quantity < 0) inventory.Quantity = 0;
                _ctx.SaveChanges();
            }
            else if (quantityChange > 0)
            {
                // Chưa có trong kho -> Tạo mới
                _ctx.Inventories.Add(new Inventory
                {
                    WarehouseId = warehouseId,
                    ProductCode = productCode,
                    Quantity = quantityChange
                });
                _ctx.SaveChanges();
            }
        }

        // Set số lượng cố định (dùng cho Adjust)
        public void SetQuantity(int warehouseId, string productCode, int quantity)
        {
            _ctx = new();
            var inventory = _ctx.Inventories
                .FirstOrDefault(i => i.WarehouseId == warehouseId && i.ProductCode == productCode);

            if (inventory != null)
            {
                inventory.Quantity = quantity;
                _ctx.SaveChanges();
            }
            else
            {
                _ctx.Inventories.Add(new Inventory
                {
                    WarehouseId = warehouseId,
                    ProductCode = productCode,
                    Quantity = quantity
                });
                _ctx.SaveChanges();
            }
        }

        // Chuyển hàng giữa 2 kho (từ Kho Chính -> Store)
        public bool TransferStock(int fromWarehouseId, int toWarehouseId, string productCode, int quantity)
        {
            _ctx = new();
            using var transaction = _ctx.Database.BeginTransaction();

            try
            {
                // Kiểm tra kho nguồn có đủ hàng không
                var fromInventory = _ctx.Inventories
                    .FirstOrDefault(i => i.WarehouseId == fromWarehouseId && i.ProductCode == productCode);

                if (fromInventory == null || fromInventory.Quantity < quantity)
                {
                    return false; // Không đủ hàng
                }

                // Trừ kho nguồn
                fromInventory.Quantity -= quantity;

                // Cộng kho đích
                var toInventory = _ctx.Inventories
                    .FirstOrDefault(i => i.WarehouseId == toWarehouseId && i.ProductCode == productCode);

                if (toInventory != null)
                {
                    toInventory.Quantity += quantity;
                }
                else
                {
                    _ctx.Inventories.Add(new Inventory
                    {
                        WarehouseId = toWarehouseId,
                        ProductCode = productCode,
                        Quantity = quantity
                    });
                }

                _ctx.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}
