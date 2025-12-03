using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.DAL.Repositories
{
    public class CategoryRepo
    {
        private SupermarketDb3Context? _ctx;
        public List<Category> GetAll()
        {
            _ctx = new();
            return _ctx.Categories.ToList();
        }
    }
}
