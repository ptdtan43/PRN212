using System;
using System.Collections.Generic;

namespace SupperMarket.DAL.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string? Role { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
