using SupperMarket.DAL.Models;
using SupperMarket.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.BLL.Service
{
    public class InventoryService
    {
        private InventoryRepo _repo = new();

        public List<Inventory> GetInventoryByWarehouse(int warehouseId)
        {
            return _repo.GetByWarehouse(warehouseId);
        }

        public int GetStock(int warehouseId, string productCode)
        {
            return _repo.GetStock(warehouseId, productCode);
        }

        public void IncreaseStock(int warehouseId, string productCode, int amount)
        {
            _repo.UpdateQuantity(warehouseId, productCode, amount);
        }

        public void DecreaseStock(int warehouseId, string productCode, int amount)
        {
            _repo.UpdateQuantity(warehouseId, productCode, -amount);
        }

        public void SetStock(int warehouseId, string productCode, int quantity)
        {
            _repo.SetQuantity(warehouseId, productCode, quantity);
        }

        public bool TransferStock(int fromWarehouseId, int toWarehouseId, string productCode, int quantity)
        {
            return _repo.TransferStock(fromWarehouseId, toWarehouseId, productCode, quantity);
        }
    }
}
