using SupperMarket.DAL.Models;
using SupperMarket.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.BLL.Service
{
    public class CategoryService
    {
        private CategoryRepo _categoryRepo = new();
        public List<Category> GetAllCategories()
        {
            return _categoryRepo.GetAll();
        }
    }
}
