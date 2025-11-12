using System;
using System.Collections.Generic;

namespace SupperMarket.DAL.Models;

public partial class Sale
{
    public int SaleId { get; set; }

    public int AccountId { get; set; }

    public string ProductCode { get; set; } = null!;

    public int? QuantitySold { get; set; }

    public DateTime? SaleDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Product ProductCodeNavigation { get; set; } = null!;
}
