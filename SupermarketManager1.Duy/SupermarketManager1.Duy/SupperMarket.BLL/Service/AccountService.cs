using SupperMarket.DAL.Models;
using SupperMarket.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.BLL.Service
{
    public class AccountService
    {
        private AccountRepo _repo = new();

        // Login - Validate username và password
        public Account? Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            return _repo.ValidateLogin(username, password);
        }

        // Lấy account theo username
        public Account? GetByUsername(string username)
        {
            return _repo.GetByUsername(username);
        }

        // Lấy tất cả accounts
        public List<Account> GetAllAccounts()
        {
            return _repo.GetAll();
        }

        // Lấy account theo ID
        public Account? GetAccountById(int accountId)
        {
            return _repo.GetById(accountId);
        }

        // Create account
        public void CreateAccount(Account account)
        {
            _repo.Create(account);
        }

        // Update account
        public void UpdateAccount(Account account)
        {
            _repo.Update(account);
        }

        // Delete account
        public void DeleteAccount(Account account)
        {
            _repo.Delete(account);
        }

        // Lấy accounts theo Role
        public List<Account> GetAccountsByRole(int roleId)
        {
            return _repo.GetAll().Where(a => a.RoleId == roleId).ToList();
        }

        // Lấy accounts theo Warehouse
        public List<Account> GetAccountsByWarehouse(int warehouseId)
        {
            return _repo.GetAll().Where(a => a.WarehouseId == warehouseId).ToList();
        }
    }
}

