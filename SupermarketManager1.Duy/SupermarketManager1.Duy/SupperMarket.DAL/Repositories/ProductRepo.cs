using Microsoft.EntityFrameworkCore;
using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.DAL.Repositories
{
    public class ProductRepo
    {
        private SupermarketManagerContext _ctx;
        public List<Product> GetAll()
        {
            _ctx = new();
            return _ctx.Products.Include("Cate").ToList();
            
        }
       
        public void Create(Product obj)
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
