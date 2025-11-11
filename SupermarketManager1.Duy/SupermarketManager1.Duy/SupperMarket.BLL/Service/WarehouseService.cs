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
    }

}
