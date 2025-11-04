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
        public List<Product> GetAll()
        {
            return _repo.GetAll();
        }
        public void Add(Product obj)
        {
            _repo.Create(obj);
        }
        public void Update(Product obj)
        {
            _repo.Update(obj);
        }
        public void Delete(Product obj)
    {
            _repo.Delete(obj);
        }
    }
}
