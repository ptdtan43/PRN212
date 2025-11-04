-- 1️⃣ Create the database
CREATE DATABASE SupermarketManager;
GO

-- 2️⃣ Use the database
USE SupermarketManager;
GO

-----------------------------------------------------
-- TABLE: Category
-----------------------------------------------------
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL
);
GO

-----------------------------------------------------
-- TABLE: Staff (with role-based restriction)
-----------------------------------------------------
CREATE TABLE Staff (
    StaffID INT IDENTITY(1,1) PRIMARY KEY,
    Role NVARCHAR(20) CHECK (Role IN ('Manager', 'Staff')), -- Only 2 roles allowed
    Username NVARCHAR(50) UNIQUE NOT NULL,
    [Password] NVARCHAR(255) NOT NULL
);
GO

-----------------------------------------------------
-- TABLE: Product
-----------------------------------------------------
CREATE TABLE Product (
    ProductCode NVARCHAR(50) PRIMARY KEY,
    NameP NVARCHAR(100) NOT NULL,
    CateID INT FOREIGN KEY REFERENCES Category(CategoryID),
    Price DECIMAL(10,2) CHECK (Price >= 0),
    SupplierName NVARCHAR(100),
    PublicationDay DATE,
    Quantity INT CHECK (Quantity >= 0),
    Warranty NVARCHAR(50),
    [Description] NVARCHAR(255)
);
GO

-----------------------------------------------------
-- TABLE: Sales (staff can sell, manager cannot)
-----------------------------------------------------
CREATE TABLE Sales (
    SaleID INT IDENTITY(1,1) PRIMARY KEY,
    StaffID INT NOT NULL FOREIGN KEY REFERENCES Staff(StaffID),
    ProductCode NVARCHAR(50) NOT NULL FOREIGN KEY REFERENCES Product(ProductCode),
    QuantitySold INT CHECK (QuantitySold > 0),
    SaleDate DATETIME DEFAULT GETDATE()
);
GO

-----------------------------------------------------
-- TRIGGER: Prevent Managers from selling
-----------------------------------------------------
CREATE TRIGGER trg_BlockManagerSales
ON Sales
FOR INSERT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN Staff s ON i.StaffID = s.StaffID
        WHERE s.Role = 'Manager'
    )
    BEGIN
        RAISERROR('Managers cannot sell products!', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

-----------------------------------------------------
-- INSERT: Category data
-----------------------------------------------------
INSERT INTO Category (CategoryName)
VALUES 
('Beverages'),
('Snacks'),
('Household'),
('Personal Care'),
('Electronics');
GO

-----------------------------------------------------
-- INSERT: Staff data
-----------------------------------------------------
INSERT INTO Staff (Role, Username, [Password]) VALUES
('Manager', 'admin', 'admin123'),
('Staff', 'john', 'john123'),
('Staff', 'mary', 'mary123');
GO

-----------------------------------------------------
-- INSERT: Product data (55 records)
-----------------------------------------------------
INSERT INTO Product (ProductCode, NameP, CateID, Price, SupplierName, PublicationDay, Quantity, Warranty, [Description])
VALUES
('P001', 'Coca-Cola 1.5L', 1, 1.25, 'Coca-Cola Company', '2024-11-15', 120, NULL, 'Soft drink'),
('P002', 'Lays Chips 100g', 2, 1.10, 'PepsiCo', '2024-12-10', 80, NULL, 'Crispy potato chips'),
('P003', 'Detergent Powder 2kg', 3, 3.99, 'Unilever', '2024-10-01', 50, NULL, 'Laundry detergent'),
('P004', 'Shampoo 500ml', 4, 5.25, 'P&G', '2024-09-20', 60, NULL, 'Hair care product'),
('P005', 'Rice Cooker', 5, 35.50, 'Philips', '2024-08-05', 15, '1 year', 'Electric rice cooker'),

-- 🥤 Beverages
('P006', 'Pepsi 1.5L', 1, 1.20, 'PepsiCo', '2024-11-10', 100, NULL, 'Soft drink'),
('P007', 'Sprite 1.5L', 1, 1.25, 'Coca-Cola Company', '2024-10-05', 110, NULL, 'Lemon-lime soda'),
('P008', 'Aquafina 500ml', 1, 0.50, 'PepsiCo', '2024-09-18', 200, NULL, 'Pure drinking water'),
('P009', 'Dasani 500ml', 1, 0.55, 'Coca-Cola Company', '2024-09-25', 180, NULL, 'Mineral water'),
('P010', 'Red Bull 250ml', 1, 2.20, 'Red Bull GmbH', '2024-08-20', 60, NULL, 'Energy drink'),
('P011', 'Nescafe Instant Coffee 100g', 1, 4.50, 'Nestlé', '2024-07-01', 90, NULL, 'Instant coffee powder'),
('P012', 'Lipton Tea 50 Bags', 1, 3.25, 'Unilever', '2024-06-18', 70, NULL, 'Black tea'),
('P013', 'Oolong Tea 500ml', 1, 1.10, 'Suntory', '2024-10-22', 130, NULL, 'Bottled tea'),
('P014', 'Fanta Orange 1.5L', 1, 1.25, 'Coca-Cola Company', '2024-11-02', 95, NULL, 'Orange soft drink'),
('P015', 'Milo 240ml Can', 1, 1.35, 'Nestlé', '2024-09-15', 140, NULL, 'Chocolate malt drink'),

-- 🍪 Snacks
('P016', 'Oreo 133g', 2, 1.75, 'Mondelez', '2024-09-01', 120, NULL, 'Chocolate sandwich cookies'),
('P017', 'Pringles Original 110g', 2, 2.50, 'Kellogg’s', '2024-07-28', 85, NULL, 'Potato crisps'),
('P018', 'KitKat 4 Finger', 2, 1.20, 'Nestlé', '2024-11-11', 100, NULL, 'Chocolate wafer bar'),
('P019', 'Snickers Bar 50g', 2, 1.15, 'Mars Inc.', '2024-10-15', 95, NULL, 'Chocolate bar'),
('P020', 'ChocoPie 6pcs', 2, 2.75, 'Orion', '2024-08-12', 80, NULL, 'Chocolate marshmallow pie'),
('P021', 'Corn Chips 100g', 2, 1.05, 'Frito-Lay', '2024-09-07', 140, NULL, 'Crunchy corn snack'),
('P022', 'Pocky Chocolate', 2, 1.50, 'Glico', '2024-10-22', 150, NULL, 'Chocolate biscuit sticks'),
('P023', 'Ritz Crackers 200g', 2, 2.20, 'Mondelez', '2024-09-03', 70, NULL, 'Salted crackers'),
('P024', 'M&M’s Peanut 100g', 2, 2.50, 'Mars Inc.', '2024-08-17', 60, NULL, 'Candy-coated peanuts'),
('P025', 'Lotte Gummies 80g', 2, 1.00, 'Lotte', '2024-11-19', 90, NULL, 'Fruit-flavored gummies'),

-- 🧽 Household
('P026', 'Toilet Paper 10 Rolls', 3, 4.20, 'Kimberly-Clark', '2024-07-20', 75, NULL, 'Soft tissue rolls'),
('P027', 'Dishwashing Liquid 1L', 3, 2.80, 'Unilever', '2024-06-14', 85, NULL, 'Cleans grease effectively'),
('P028', 'Floor Cleaner 2L', 3, 3.50, 'P&G', '2024-08-30', 60, NULL, 'Lavender scented cleaner'),
('P029', 'Garbage Bags (20pcs)', 3, 2.10, 'SC Johnson', '2024-09-05', 150, NULL, 'Heavy-duty garbage bags'),
('P030', 'Tissue Box 200 Sheets', 3, 1.80, 'Kimberly-Clark', '2024-09-12', 95, NULL, 'Facial tissues'),
('P031', 'Air Freshener 300ml', 3, 2.90, 'Glade', '2024-07-01', 80, NULL, 'Floral fragrance'),
('P032', 'Glass Cleaner 500ml', 3, 3.10, 'SC Johnson', '2024-08-22', 70, NULL, 'Cleans glass and mirrors'),
('P033', 'Mop Set', 3, 7.99, '3M', '2024-09-28', 30, '6 months', 'Includes bucket and mop'),
('P034', 'Laundry Basket', 3, 5.50, 'HomePro', '2024-07-15', 45, '1 year', 'Plastic laundry basket'),
('P035', 'Iron 1000W', 3, 18.90, 'Philips', '2024-05-25', 20, '2 years', 'Steam dry iron'),

-- 🧴 Personal Care
('P036', 'Colgate Toothpaste 200g', 4, 1.95, 'Colgate-Palmolive', '2024-09-10', 120, NULL, 'Whitening toothpaste'),
('P037', 'Lifebuoy Soap 100g', 4, 0.80, 'Unilever', '2024-08-15', 180, NULL, 'Antibacterial soap'),
('P038', 'Head & Shoulders Shampoo 400ml', 4, 4.25, 'P&G', '2024-06-25', 75, NULL, 'Anti-dandruff shampoo'),
('P039', 'Nivea Body Lotion 400ml', 4, 5.10, 'Beiersdorf', '2024-07-19', 65, NULL, 'Moisturizing lotion'),
('P040', 'Gillette Razor 2pcs', 4, 3.25, 'P&G', '2024-05-30', 100, NULL, 'Disposable razor blades'),
('P041', 'Sunsilk Conditioner 400ml', 4, 4.00, 'Unilever', '2024-06-10', 70, NULL, 'Hair softening conditioner'),
('P042', 'Vaseline Lip Balm 10g', 4, 2.10, 'Unilever', '2024-07-08', 90, NULL, 'Lip moisturizer'),
('P043', 'Dettol Hand Sanitizer 50ml', 4, 1.50, 'Reckitt', '2024-06-05', 150, NULL, 'Kills 99.9% of germs'),
('P044', 'Pond’s Face Cream 50g', 4, 3.75, 'Unilever', '2024-08-21', 60, NULL, 'Moisturizing face cream'),
('P045', 'Oral-B Toothbrush 2pcs', 4, 2.60, 'P&G', '2024-05-15', 110, NULL, 'Soft bristles toothbrush'),

-- ⚡ Electronics
('P046', 'Electric Kettle 1.8L', 5, 22.90, 'Sharp', '2024-07-18', 25, '1 year', 'Stainless steel kettle'),
('P047', 'Microwave Oven 20L', 5, 69.99, 'Panasonic', '2024-08-03', 12, '2 years', 'Compact microwave'),
('P048', 'Blender 600W', 5, 35.25, 'Philips', '2024-07-12', 18, '1 year', 'Glass jar blender'),
('P049', 'Smartphone Charger', 5, 7.99, 'Anker', '2024-09-08', 100, '6 months', 'Fast charging USB-C'),
('P050', 'Bluetooth Speaker', 5, 24.50, 'JBL', '2024-08-28', 22, '1 year', 'Portable wireless speaker'),
('P051', 'LED Bulb 12W', 5, 2.75, 'Philips', '2024-07-30', 160, '6 months', 'Energy-saving bulb'),
('P052', 'USB Flash Drive 32GB', 5, 9.99, 'SanDisk', '2024-10-05', 85, '1 year', 'High-speed storage'),
('P053', 'Wireless Mouse', 5, 12.50, 'Logitech', '2024-07-09', 45, '1 year', 'Ergonomic design'),
('P054', 'Laptop Stand', 5, 18.20, 'Baseus', '2024-06-20', 30, '1 year', 'Adjustable aluminum stand'),
('P055', 'Power Bank 10000mAh', 5, 21.75, 'Xiaomi', '2024-09-27', 35, '1 year', 'Portable battery pack');
GO

-----------------------------------------------------
-- ✅ Verification Queries
-----------------------------------------------------
SELECT * FROM Category;
SELECT * FROM Staff;
SELECT * FROM Product;
GO
