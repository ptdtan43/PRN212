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
        private SupermarketDb3Context _cxt;
        public List<Category> GetAll()
        {
            _cxt = new();
            return _cxt.Categories.ToList();
        }
    }
}
