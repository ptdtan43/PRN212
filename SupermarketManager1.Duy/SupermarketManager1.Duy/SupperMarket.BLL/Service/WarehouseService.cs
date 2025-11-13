using SupperMarket.DAL.Models;
using SupperMarket.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.BLL.Service
{
    public class WarehouseService
    {
        private WarehouseRepo _repo = new();

        public List<Warehouse> GetAllWarehouses()
        {
            return _repo.GetAll();
        }

        public Warehouse? GetWarehouseById(int id)
        {
            return _repo.GetById(id);
        }

        public Warehouse? GetCentralWarehouse()
        {
            return _repo.GetCentralWarehouse();
        }

        public List<Warehouse> GetStores()
        {
            return _repo.GetStores();
        }

        // Create Warehouse
        public void CreateWarehouse(Warehouse warehouse)
        {
            _repo.Create(warehouse);
        }

        // Update Warehouse
        public void UpdateWarehouse(Warehouse warehouse)
        {
            _repo.Update(warehouse);
        }

        // Delete Warehouse
        public void DeleteWarehouse(Warehouse warehouse)
        {
            _repo.Delete(warehouse);
        }
    }

        public void CreateStore(Warehouse store)
        {
            _repo.Create(store);
        }

        public void UpdateStore(Warehouse store) 
        { 
            _repo.Update(store); 
        }

        public void DeleteStore(Warehouse store) 
        {
            _repo.Delete(store); 
        }
    }
}
