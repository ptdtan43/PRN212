using SupperMarket.DAL.Models;
using SupperMarket.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.BLL.Service
{
    public class RoleService
    {
        private RoleRepo _repo = new();

        public List<Role> GetAllRoles()
        {
            return _repo.GetAll();
        }

        public Role? GetRoleById(int roleId)
        {
            return _repo.GetById(roleId);
        }
    }
}

