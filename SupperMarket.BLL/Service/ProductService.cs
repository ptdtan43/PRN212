using SupperMarket.DAL.Models;
using SupperMarket.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.BLL.Service
{
    public class ProductService
    {
        ProductRepo _repo= new();
        public List<Product> GetAllProducts()
        {
            return _repo.GetAll();
        }

        public Product? GetProductByCode(string productCode)
        {
            return _repo.GetByCode(productCode);
        }

        public void AddProduct(Product obj)
        {
            _repo.Create(obj);
        }
        public void UpdateProduct(Product obj)
        {
            _repo.Update(obj);
        }
        public void DeleteProduct(Product obj, bool deleteInventory = false)
        {
            _repo.Delete(obj, deleteInventory);
        }
    }
}
