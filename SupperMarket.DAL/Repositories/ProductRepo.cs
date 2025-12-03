using Microsoft.EntityFrameworkCore;
using SupperMarket.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupperMarket.DAL.Repositories
{
    public class ProductRepo
    {
        private SupermarketDb3Context? _ctx;
        public List<Product> GetAll()
        {
            _ctx = new();
            return _ctx.Products.Include("Cate").ToList();
            
        }

        public Product? GetByCode(string productCode)
        {
            _ctx = new();
            return _ctx.Products.FirstOrDefault(p => p.ProductCode == productCode);
        }
       
        public void Create(Product obj)
        {
            _ctx = new();
            _ctx.Products.Add(obj);
            _ctx.SaveChanges();
        }
        public void Update(Product obj)
        {
            _ctx = new();
            _ctx.Products.Update(obj);
            _ctx.SaveChanges();
        }
        public void Delete(Product obj, bool deleteInventory = false)
        {
            _ctx = new();
            
            // Kiểm tra xem Product có đang được sử dụng trong Inventory không
            bool hasInventory = _ctx.Inventories.Any(i => i.ProductCode == obj.ProductCode);
            if (hasInventory)
            {
                if (deleteInventory)
                {
                    // Xóa tất cả tồn kho của sản phẩm này
                    var inventories = _ctx.Inventories.Where(i => i.ProductCode == obj.ProductCode).ToList();
                    _ctx.Inventories.RemoveRange(inventories);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Không thể xóa sản phẩm '{obj.NameP}' vì sản phẩm này đang có trong tồn kho. " +
                        "Vui lòng xóa tất cả tồn kho của sản phẩm này trước khi xóa sản phẩm.");
                }
            }

            // Kiểm tra xem Product có đang được sử dụng trong Sales không
            bool hasSales = _ctx.Sales.Any(s => s.ProductCode == obj.ProductCode);
            if (hasSales)
            {
                throw new InvalidOperationException(
                    $"Không thể xóa sản phẩm '{obj.NameP}' vì sản phẩm này đã có lịch sử bán hàng. " +
                    "Không thể xóa sản phẩm đã có giao dịch.");
            }

            _ctx.Products.Remove(obj);
            _ctx.SaveChanges();   
        }
    }
}
