using System;
using System.Collections.Generic;

namespace SupperMarket.DAL.Models;

public partial class Sale
{
    public int SaleId { get; set; }

    public int AccountId { get; set; }

    public int WarehouseId { get; set; }  // Bán từ kho nào

    public string ProductCode { get; set; } = null!;

    public int QuantitySold { get; set; }  // NOT NULL

    public decimal UnitPrice { get; set; }  // Giá tại thời điểm bán

    public decimal TotalAmount { get; set; }  //  Tổng tiền = QuantitySold × UnitPrice

    public DateTime SaleDate { get; set; }  // NOT NULL

    public virtual Account Account { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;  // Navigation property

    public virtual Product ProductCodeNavigation { get; set; } = null!;
}
