using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.DAL.Repositories
{
    public class ProductRepo
    {
        SupermarketManagerContext _ctx;
        public List<Product> GetAll()
        {
            using (_ctx = new SupermarketManagerContext())
            {
                return _ctx.Products.Include("Cate").ToList();
            }
        }
        public Product GetById(string id)
        {
            using (_ctx = new SupermarketManagerContext())
            {
                return _ctx.Products.Include("Cate").FirstOrDefault(p => p.ProductCode == id);
            }
        }
        public void Add(Product obj)
        {
            _ctx = new();
            _ctx.Products.Add(obj);
            _ctx.SaveChanges();
        }
        public void Update(Product obj)
        {
            _ctx = new();
            _ctx.Products.Update(obj);
            _ctx.SaveChanges();
        }
        public void Delete(Product obj)
        {
            _ctx = new();
            _ctx.Products.Remove(obj);
            _ctx.SaveChanges();   
        }
    }
}
