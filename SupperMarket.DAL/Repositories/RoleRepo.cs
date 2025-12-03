using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.DAL.Repositories
{
    public class RoleRepo
    {
        private SupermarketDb3Context? _ctx;

        public List<Role> GetAll()
        {
            _ctx = new();
            return _ctx.Roles.ToList();
        }

        public Role? GetById(int roleId)
        {
            _ctx = new();
            return _ctx.Roles.FirstOrDefault(r => r.RoleId == roleId);
        }
    }
}

