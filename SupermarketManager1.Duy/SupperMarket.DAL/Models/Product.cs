using System;
using System.Collections.Generic;

namespace SupperMarket.DAL.Models;

public partial class Product
{
    public string ProductCode { get; set; } = null!;

    public string NameP { get; set; } = null!;

    public int? CateId { get; set; }

    public decimal? Price { get; set; }

    public string? SupplierName { get; set; }

    public DateOnly? PublicationDay { get; set; }

    public string? Warranty { get; set; }

    public string? Description { get; set; }

    public virtual Category? Cate { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
