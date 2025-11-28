using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupermarketManager1.Duy
{
    // Class để lưu thông tin user đã login (Session)
    public static class CurrentUser
    {
        public static Account? Account { get; set; }

        public static bool IsLoggedIn => Account != null;

        public static bool IsAdmin => Account?.Role?.RoleName == "Admin";
        public static bool IsManager => Account?.Role?.RoleName == "Manager";
        public static bool IsStaff => Account?.Role?.RoleName == "Staff";

        public static int? WarehouseId => Account?.WarehouseId;

        public static void Login(Account account)
        {
            Account = account;
        }

        public static void Logout()
        {
            Account = null;
        }
    }
}

