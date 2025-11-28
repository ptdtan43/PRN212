using Microsoft.EntityFrameworkCore;
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
        private SupermarketDb3Context? _ctx;

        public List<Warehouse> GetAll()
        {
            _ctx = new();
            return _ctx.Warehouses
                .Include(w => w.Manager)
                .ToList();
        }

        public Warehouse? GetById(int id)
        {
            _ctx = new();
            return _ctx.Warehouses
                .Include(w => w.Manager)
                .FirstOrDefault(w => w.WarehouseId == id);
        }

        public Warehouse? GetCentralWarehouse()
        {
            _ctx = new();
            return _ctx.Warehouses.FirstOrDefault(w => w.Type == "Central");
        }

        public List<Warehouse> GetCentralWarehouses()
        {
            _ctx = new();
            return _ctx.Warehouses
                .Include(w => w.Manager)
                .Where(w => w.Type == "Central")
                .ToList();
        }

        public List<Warehouse> GetStores()
        {
            _ctx = new();
            return _ctx.Warehouses.Where(w => w.Type == "Store").ToList();
        }

        // Create Warehouse
        public void Create(Warehouse warehouse)
        {
            _ctx = new();
            _ctx.Warehouses.Add(warehouse);
            _ctx.SaveChanges();
        }

        // Update Warehouse
        public void Update(Warehouse warehouse)
        {
            _ctx = new();
            _ctx.Warehouses.Update(warehouse);
            _ctx.SaveChanges();
        }

        // Delete Warehouse
        public void Delete(Warehouse warehouse)
        {
            _ctx = new();
            _ctx.Warehouses.Remove(warehouse);
            _ctx.SaveChanges();
        }
    }
}
