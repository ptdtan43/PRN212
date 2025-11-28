-- ===================================
-- FULL DATABASE SCHEMA - IMPROVED VERSION
-- Supermarket Management System v3
-- Cải thiện để phù hợp với Flow Diagram
-- ===================================
DROP DATABASE IF EXISTS SupermarketDB2;
CREATE DATABASE SupermarketDB2;
GO
USE SupermarketDB2;
GO

-- 1. Bảng Category (Danh mục sản phẩm)
CREATE TABLE Categories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(255) NOT NULL
);

-- 2. Bảng Product (Sản phẩm)
CREATE TABLE Products (
    ProductCode NVARCHAR(50) PRIMARY KEY,
    NameP NVARCHAR(255) NOT NULL,
    CateId INT NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,  -- ⭐ Cải thiện: NOT NULL với DEFAULT
    SupplierName NVARCHAR(255) NULL,
    PublicationDay DATE NULL,
    Warranty NVARCHAR(255) NULL,
    Description NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Product_Category FOREIGN KEY (CateId) REFERENCES Categories(CategoryId)
);

-- 3. Bảng Roles (Quyền hệ thống)
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE -- Admin, Manager, Staff
);

-- 4. Bảng Warehouse (Kho/Cửa hàng) - ⭐ CẢI THIỆN: Thêm ManagerId
-- ⚠️ LƯU Ý: ManagerId sẽ được thêm sau khi tạo bảng Accounts (ALTER TABLE)
CREATE TABLE Warehouses (
    WarehouseId INT PRIMARY KEY IDENTITY(1,1),
    WarehouseName NVARCHAR(255) NOT NULL,
    Type NVARCHAR(50) NOT NULL CHECK (Type IN ('Central', 'Store')),  -- ⭐ Cải thiện: CHECK constraint
    Address NVARCHAR(500) NULL,
    ManagerId INT NULL  -- ⭐ MỚI: Gán Manager cho Store (FK sẽ thêm sau)
);

-- 5. Bảng Accounts (Tài khoản người dùng) - ⭐ CẢI THIỆN: Thêm WarehouseId
CREATE TABLE Accounts (
    AccountId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    [Password] NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(255) NOT NULL,
    DateOfBirth DATE NULL,
    Email NVARCHAR(255) NULL,
    PhoneNumber NVARCHAR(50) NULL,
    RoleId INT NOT NULL,
    WarehouseId INT NULL,  -- ⭐ MỚI: Staff/Manager thuộc Store nào (NULL cho Admin)
    CreatedAt DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT N'Active' CHECK (Status IN ('Active', 'Locked')),  -- ⭐ Cải thiện: CHECK constraint (đã bỏ Inactive)
    CONSTRAINT FK_Account_Role FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT FK_Account_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId)
);

-- ⭐ THÊM Foreign Key cho Warehouses.ManagerId (sau khi đã có bảng Accounts)
ALTER TABLE Warehouses
ADD CONSTRAINT FK_Warehouse_Manager 
FOREIGN KEY (ManagerId) REFERENCES Accounts(AccountId);

-- 6. Bảng Inventory (Tồn kho)
CREATE TABLE Inventories (
    InventoryId INT PRIMARY KEY IDENTITY(1,1),
    WarehouseId INT NOT NULL,
    ProductCode NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Inventory_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_Inventory_Product FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode),
    CONSTRAINT UQ_Warehouse_Product UNIQUE (WarehouseId, ProductCode)
);

-- 7. Bảng Sales (Lịch sử bán hàng) - ⭐ CẢI THIỆN: Thêm WarehouseId, UnitPrice, TotalAmount
CREATE TABLE Sales (
    SaleId INT PRIMARY KEY IDENTITY(1,1),
    AccountId INT NOT NULL,
    WarehouseId INT NOT NULL,  -- ⭐ MỚI: Bán từ kho nào
    ProductCode NVARCHAR(50) NOT NULL,
    QuantitySold INT NOT NULL,  -- ⭐ Cải thiện: NOT NULL
    UnitPrice DECIMAL(18,2) NOT NULL,  -- ⭐ MỚI: Giá tại thời điểm bán
    TotalAmount DECIMAL(18,2) NOT NULL,  -- ⭐ MỚI: Tổng tiền = QuantitySold × UnitPrice
    SaleDate DATETIME NOT NULL DEFAULT GETDATE(),  -- ⭐ Cải thiện: NOT NULL với DEFAULT
    CONSTRAINT FK_Sale_Account FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT FK_Sale_Product FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode),
    CONSTRAINT FK_Sale_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId)
);

-- ===================================
-- DỮ LIỆU MẪU (SAMPLE DATA)
-- ===================================

-- Roles
INSERT INTO Roles (RoleName) VALUES
('Admin'),
('Manager'),
('Staff');

-- Categories
INSERT INTO Categories (CategoryName) VALUES
(N'Điện tử'),
(N'Thời trang'),
(N'Thực phẩm'),
(N'Đồ gia dụng');

-- Products
INSERT INTO Products (ProductCode, NameP, CateId, Price, SupplierName, Warranty, Description) VALUES
('P001', N'Áo thun nam', 2, 150000, N'Nhà cung cấp A', N'6 tháng', N'Áo thun cotton cao cấp'),
('P002', N'Quần jean nữ', 2, 350000, N'Nhà cung cấp B', N'1 năm', N'Quần jean co giãn'),
('P003', N'Giày sneaker', 2, 500000, N'Nhà cung cấp C', N'1 năm', N'Giày thể thao'),
('P004', N'Túi xách', 2, 250000, N'Nhà cung cấp D', N'6 tháng', N'Túi xách da'),
('P005', N'Laptop Dell', 1, 15000000, N'Dell Vietnam', N'2 năm', N'Laptop văn phòng'),
('P006', N'Tai nghe Bluetooth', 1, 500000, N'Sony', N'1 năm', N'Tai nghe không dây'),
('P007', N'Gạo ST25', 3, 25000, N'Nông trại XYZ', N'Không', N'Gạo thơm Sóc Trăng'),
('P008', N'Nồi cơm điện', 4, 800000, N'Panasonic', N'2 năm', N'Nồi cơm điện tử');

-- Warehouses (tạm thời chưa có ManagerId, sẽ cập nhật sau khi có Accounts)
INSERT INTO Warehouses (WarehouseName, Type, Address, ManagerId) VALUES
(N'Kho Trung Tâm', 'Central', N'123 Nguyễn Văn Cừ, Quận 1, TP.HCM', NULL),  -- Kho trung tâm không có Manager
(N'Cửa hàng Chi nhánh 1', 'Store', N'456 Lê Lợi, Quận 3, TP.HCM', NULL),  -- Sẽ cập nhật sau
(N'Cửa hàng Chi nhánh 2', 'Store', N'789 Võ Văn Tần, Quận 5, TP.HCM', NULL);  -- Sẽ cập nhật sau

-- Accounts - ⭐ CẢI THIỆN: Thêm WarehouseId
INSERT INTO Accounts (Username, [Password], FullName, DateOfBirth, Email, PhoneNumber, RoleId, WarehouseId)
VALUES
('admin', '123456', N'Nguyễn Văn Quản Trị', '1990-01-01', 'admin@supermarket.vn', '0909123456', 1, NULL),  -- Admin: WarehouseId = NULL
('manager1', '123456', N'Trần Thị Quản Lý', '1992-05-15', 'manager@supermarket.vn', '0909988776', 2, 2),  -- Manager: quản lý Store 1
('manager2', '123456', N'Lê Văn Quản Lý 2', '1991-03-20', 'manager2@supermarket.vn', '0909887766', 2, 3),  -- Manager: quản lý Store 2
('staff1', '123456', N'Lê Văn Nhân Viên', '1998-07-22', 'staff1@supermarket.vn', '0912345678', 3, 2),  -- Staff: làm việc tại Store 1
('staff2', '123456', N'Phạm Thị Bán Hàng', '1997-09-10', 'staff2@supermarket.vn', '0933221100', 3, 2),  -- Staff: làm việc tại Store 1
('staff3', '123456', N'Nguyễn Thị Nhân Viên', '1999-11-05', 'staff3@supermarket.vn', '0944556677', 3, 3);  -- Staff: làm việc tại Store 2

-- ⭐ CẬP NHẬT: Gán Manager cho các Store
UPDATE Warehouses SET ManagerId = 2 WHERE WarehouseId = 2;  -- Store 1 → Manager 1
UPDATE Warehouses SET ManagerId = 3 WHERE WarehouseId = 3;  -- Store 2 → Manager 2

-- Inventories
INSERT INTO Inventories (WarehouseId, ProductCode, Quantity) VALUES
(1, 'P001', 500),
(1, 'P002', 300),
(1, 'P003', 200),
(1, 'P004', 150),
(1, 'P005', 50),
(1, 'P006', 100),
(1, 'P007', 1000),
(1, 'P008', 80),
(2, 'P001', 50),
(2, 'P002', 30),
(2, 'P003', 20),
(2, 'P007', 100),
(3, 'P001', 40),
(3, 'P005', 5),
(3, 'P006', 15);

-- Sales (dữ liệu mẫu) - ⭐ CẢI THIỆN: Thêm WarehouseId, UnitPrice, TotalAmount
INSERT INTO Sales (AccountId, WarehouseId, ProductCode, QuantitySold, UnitPrice, TotalAmount, SaleDate)
VALUES
(4, 2, 'P001', 2, 150000, 300000, GETDATE()),  -- Staff1 bán 2 áo thun tại Store 1
(4, 2, 'P002', 1, 350000, 350000, GETDATE()),  -- Staff1 bán 1 quần jean tại Store 1
(5, 2, 'P007', 5, 25000, 125000, DATEADD(DAY, -1, GETDATE())),  -- Staff2 bán 5 gạo tại Store 1
(6, 3, 'P001', 1, 150000, 150000, DATEADD(DAY, -2, GETDATE()));  -- Staff3 bán 1 áo thun tại Store 2

-- ===================================
-- INDEXES
-- ===================================
CREATE INDEX IX_Product_Name ON Products(NameP);
CREATE INDEX IX_Inventory_Warehouse ON Inventories(WarehouseId);
CREATE INDEX IX_Sale_AccountDate ON Sales(AccountId, SaleDate);
CREATE INDEX IX_Sale_WarehouseDate ON Sales(WarehouseId, SaleDate);  -- ⭐ MỚI: Index cho truy vấn theo kho
CREATE INDEX IX_Account_Warehouse ON Accounts(WarehouseId);  -- ⭐ MỚI: Index cho truy vấn staff theo store

-- ===================================
-- STORED PROCEDURE: Chuyển kho
-- ===================================
GO
CREATE PROCEDURE sp_TransferStock
    @FromWarehouseId INT,
    @ToWarehouseId INT,
    @ProductCode NVARCHAR(50),
    @Quantity INT,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @CurrentStock INT;
        SELECT @CurrentStock = Quantity 
        FROM Inventories 
        WHERE WarehouseId = @FromWarehouseId AND ProductCode = @ProductCode;
        
        IF @CurrentStock IS NULL OR @CurrentStock < @Quantity
        BEGIN
            SET @Success = 0;
            SET @Message = N'Không đủ hàng trong kho nguồn';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        UPDATE Inventories 
        SET Quantity = Quantity - @Quantity
        WHERE WarehouseId = @FromWarehouseId AND ProductCode = @ProductCode;
        
        IF EXISTS (SELECT 1 FROM Inventories WHERE WarehouseId = @ToWarehouseId AND ProductCode = @ProductCode)
        BEGIN
            UPDATE Inventories 
            SET Quantity = Quantity + @Quantity
            WHERE WarehouseId = @ToWarehouseId AND ProductCode = @ProductCode;
        END
        ELSE
        BEGIN
            INSERT INTO Inventories (WarehouseId, ProductCode, Quantity)
            VALUES (@ToWarehouseId, @ProductCode, @Quantity);
        END
        
        COMMIT TRANSACTION;
        SET @Success = 1;
        SET @Message = N'Chuyển kho thành công';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END;
GO

-- ===================================
-- TRIGGER: Tự động trừ tồn kho khi bán hàng
-- ===================================
GO

-- Xóa trigger cũ nếu có
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_AfterSale_UpdateInventory')
    DROP TRIGGER trg_AfterSale_UpdateInventory;
GO

-- Tạo trigger mới với logic cải thiện
CREATE TRIGGER trg_AfterSale_UpdateInventory
ON Sales
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- ⭐ CẢI THIỆN: Check tồn kho TRƯỚC khi UPDATE
        -- Kiểm tra xem có record nào không đủ hàng không
        IF EXISTS (
            SELECT 1 
            FROM inserted i
            LEFT JOIN Inventories inv 
                ON inv.WarehouseId = i.WarehouseId 
                AND inv.ProductCode = i.ProductCode
            WHERE inv.Quantity IS NULL OR inv.Quantity < i.QuantitySold
        )
        BEGIN
            -- Nếu không đủ hàng, rollback và throw error
            RAISERROR(N'Không đủ hàng trong kho để bán', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- ⭐ Nếu đủ hàng, mới UPDATE tồn kho
        UPDATE inv
        SET inv.Quantity = inv.Quantity - i.QuantitySold
        FROM Inventories inv
        INNER JOIN inserted i 
            ON inv.WarehouseId = i.WarehouseId 
            AND inv.ProductCode = i.ProductCode;
        
    END TRY
    BEGIN CATCH
        -- Nếu có lỗi, rollback transaction
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ===================================
-- VIEW: Xem tồn kho theo kho (tiện cho truy vấn)
-- ===================================
GO
CREATE VIEW vw_InventoryByWarehouse
AS
SELECT 
    w.WarehouseId,
    w.WarehouseName,
    w.Type,
    p.ProductCode,
    p.NameP AS ProductName,
    p.Price,
    ISNULL(inv.Quantity, 0) AS Quantity
FROM Warehouses w
CROSS JOIN Products p
LEFT JOIN Inventories inv 
    ON inv.WarehouseId = w.WarehouseId 
    AND inv.ProductCode = p.ProductCode;
GO

-- ===================================
-- VIEW: Xem doanh thu theo kho
-- ===================================
GO
CREATE VIEW vw_SalesByWarehouse
AS
SELECT 
    w.WarehouseId,
    w.WarehouseName,
    s.SaleDate,
    s.ProductCode,
    p.NameP AS ProductName,
    s.QuantitySold,
    s.UnitPrice,
    s.TotalAmount,
    a.FullName AS StaffName
FROM Sales s
INNER JOIN Warehouses w ON s.WarehouseId = w.WarehouseId
INNER JOIN Products p ON s.ProductCode = p.ProductCode
INNER JOIN Accounts a ON s.AccountId = a.AccountId;
GO

