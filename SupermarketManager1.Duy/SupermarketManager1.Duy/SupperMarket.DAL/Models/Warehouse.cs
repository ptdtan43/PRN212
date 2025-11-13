using System;
using System.Collections.Generic;

namespace SupperMarket.DAL.Models;

public partial class Warehouse
{
    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string? Address { get; set; }

    public int? ManagerId { get; set; }  // Manager quản lý Store này

    public virtual Account? Manager { get; set; }  // Navigation property đến Manager

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();  // Staff/Manager thuộc warehouse này

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();  // Sales từ warehouse này
}
