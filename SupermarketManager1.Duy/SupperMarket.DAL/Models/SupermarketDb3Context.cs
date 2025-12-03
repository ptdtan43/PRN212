using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace SupperMarket.DAL.Models;

public partial class SupermarketDb3Context : DbContext
{
    public SupermarketDb3Context()
    {
    }

    public SupermarketDb3Context(DbContextOptions<SupermarketDb3Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }
    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnectionStringDB"];

        return strConn;
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Accounts__349DA5A6A26653B2");

            entity.HasIndex(e => e.Username, "UQ__Accounts__536C85E415A3515F").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Role");

            
            entity.HasOne(d => d.Warehouse).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK_Account_Warehouse");

            
            entity.HasIndex(e => e.WarehouseId, "IX_Account_Warehouse");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BCAB2A668");

            entity.Property(e => e.CategoryName).HasMaxLength(255);
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__Inventor__F5FDE6B3E2058EE4");

            entity.HasIndex(e => e.WarehouseId, "IX_Inventory_Warehouse");

            entity.HasIndex(e => new { e.WarehouseId, e.ProductCode }, "UQ_Warehouse_Product").IsUnique();

            entity.Property(e => e.ProductCode).HasMaxLength(50);

            entity.HasOne(d => d.Product).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.ProductCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventory_Product");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventory_Warehouse");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductCode).HasName("PK__Products__2F4E024EB265D8C5");

            entity.HasIndex(e => e.NameP, "IX_Product_Name");

            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.NameP).HasMaxLength(255);
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SupplierName).HasMaxLength(255);
            entity.Property(e => e.Warranty).HasMaxLength(255);

            entity.HasOne(d => d.Cate).WithMany(p => p.Products)
                .HasForeignKey(d => d.CateId)
                .HasConstraintName("FK_Product_Category");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A54217374");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61604CEC34CE").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("PK__Sales__1EE3C3FF9179F090");

            entity.HasIndex(e => new { e.AccountId, e.SaleDate }, "IX_Sale_AccountDate");

            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.SaleDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Sales)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Account");

            entity.HasOne(d => d.ProductCodeNavigation).WithMany(p => p.Sales)
                .HasForeignKey(d => d.ProductCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Product");


            entity.HasOne(d => d.Warehouse).WithMany(p => p.Sales)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Warehouse");

            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SaleDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasIndex(e => new { e.WarehouseId, e.SaleDate }, "IX_Sale_WarehouseDate");

            entity.ToTable(tb => tb.HasTrigger("trg_AfterSale_UpdateInventory"));
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId).HasName("PK__Warehous__2608AFF962D1F120");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.WarehouseName).HasMaxLength(255);

            entity.HasOne(d => d.Manager).WithMany()
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_Warehouse_Manager");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
