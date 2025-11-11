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

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

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
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B7CB913CD");

            entity.Property(e => e.CategoryName).HasMaxLength(255);
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__Inventor__F5FDE6B3E9F35511");

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
            entity.HasKey(e => e.ProductCode).HasName("PK__Products__2F4E024ED8C1A48C");

            entity.HasIndex(e => e.NameP, "IX_Product_Name");

            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.NameP).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SupplierName).HasMaxLength(255);
            entity.Property(e => e.Warranty).HasMaxLength(255);

            entity.HasOne(d => d.Cate).WithMany(p => p.Products)
                .HasForeignKey(d => d.CateId)
                .HasConstraintName("FK_Product_Category");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("PK__Sales__1EE3C3FFF46C9B96");

            entity.HasIndex(e => new { e.StaffId, e.SaleDate }, "IX_Sale_StaffDate");

            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.SaleDate).HasColumnType("datetime");

            entity.HasOne(d => d.ProductCodeNavigation).WithMany(p => p.Sales)
                .HasForeignKey(d => d.ProductCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Product");

            entity.HasOne(d => d.Staff).WithMany(p => p.Sales)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Staff");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staffs__96D4AB1745332A85");

            entity.HasIndex(e => e.Username, "UQ__Staffs__536C85E4D1FA75B1").IsUnique();

            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId).HasName("PK__Warehous__2608AFF94C4B3253");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.WarehouseName).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
