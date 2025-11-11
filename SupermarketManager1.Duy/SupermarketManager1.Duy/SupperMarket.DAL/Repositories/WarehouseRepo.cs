using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.DAL.Repositories
{
    public class WarehouseRepo
    {
        private SupermarketDb3Context _ctx;

        public List<Warehouse> GetAll()
        {
            _ctx = new();
            return _ctx.Warehouses.ToList();
        }

        public Warehouse? GetById(int id)
        {
            _ctx = new();
            return _ctx.Warehouses.FirstOrDefault(w => w.WarehouseId == id);
        }

        public Warehouse? GetCentralWarehouse()
        {
            _ctx = new();
            return _ctx.Warehouses.FirstOrDefault(w => w.Type == "Central");
        }

        public List<Warehouse> GetStores()
        {
            _ctx = new();
            return _ctx.Warehouses.Where(w => w.Type == "Store").ToList();
        }
    }
}
