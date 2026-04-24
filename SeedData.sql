-- ============================================================
-- DELIVERY MANAGEMENT SYSTEM - DATABASE SEED DATA
-- SQL Server (T-SQL)  |  Engine: ASP.NET Core 8 + EF Core
-- ============================================================
--
-- IMPORTANT - PASSWORD HASHES
-- The PasswordHash column below contains a valid ASP.NET
-- Identity V3 hash format placeholder.
-- To generate a working hash for your own password, run
-- this snippet in a .NET 8 console / LINQPad:
--
--   var h = new Microsoft.AspNetCore.Identity.PasswordHasher<object>();
--   Console.WriteLine(h.HashPassword(null!, "Admin@123"));
--
-- Then replace every occurrence of the hash string in this
-- script with the output.
--
-- ENUM REFERENCE (stored as INT unless noted):
--   OrderStatus  : 0=Pending 1=Confirmed 2=InPreparation 3=PickedUp
--                  4=InTransit 5=OutForDelivery 6=Delivered 7=Cancelled
--   PaymentStatus: 0=Pending 1=Confirmed 2=Failed 3=Cancelled 4=Refunded
--   PaymentMethod: stored as NVARCHAR(50) → 'CreditCard' 'DebitCard'
--                  'Cash' 'MobilePayment' 'PayPal' 'ApplePay'
--   ReservationStatus: 0=Pending 1=Confirmed 2=Cancelled
--
-- INSERT ORDER (respects all FK constraints):
--   AspNetRoles → AspNetUsers → AspNetUserRoles →
--   Categories → Restaurants → RestaurantMenuCategories →
--   Tables → Meals → Inventories →
--   Orders → OrderItems → Payments →
--   Reservations → Reviews
-- ============================================================

BEGIN TRANSACTION;

-- ============================================================
-- 1. ROLES
-- ============================================================
SET IDENTITY_INSERT AspNetRoles ON;

INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES
    (1, 'Admin',           'ADMIN',           CAST(NEWID() AS NVARCHAR(MAX))),
    (2, 'RestaurantOwner', 'RESTAURANTOWNER', CAST(NEWID() AS NVARCHAR(MAX))),
    (3, 'Customer',        'CUSTOMER',        CAST(NEWID() AS NVARCHAR(MAX))),
    (4, 'DeliveryDriver',  'DELIVERYDRIVER',  CAST(NEWID() AS NVARCHAR(MAX)));

SET IDENTITY_INSERT AspNetRoles OFF;

-- ============================================================
-- 2. USERS
-- Replace the PasswordHash value with one generated for your
-- chosen password (see header instructions above).
-- ============================================================
SET IDENTITY_INSERT AspNetUsers ON;

INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName,
    Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled,
    LockoutEnd, LockoutEnabled, AccessFailedCount,
    Address, CreditCardNumber,
    IsActive, IsVerified, CreatedAt, ProfileImageUrl
)
VALUES
    -- 1 · Admin
    (1,
     'admin@delivery.com', 'ADMIN@DELIVERY.COM',
     'admin@delivery.com', 'ADMIN@DELIVERY.COM', 1,
     'AQAAAAIAAYagAAAAEHamOgBsuoVVSMOBxkNzmQ4d0eRJrZTDiRJpApkQKyXRbJCtO5oBMuaQQVxBaI5kXQ==',
     CAST(NEWID() AS NVARCHAR(MAX)), CAST(NEWID() AS NVARCHAR(MAX)),
     '+12125550100', 0, 0,
     NULL, 1, 0,
     '123 Admin St, New York, NY 10001', NULL,
     1, 1, '2024-01-01T00:00:00', '1.png'),

    -- 2 · Restaurant Owner
    (2,
     'owner@delivery.com', 'OWNER@DELIVERY.COM',
     'owner@delivery.com', 'OWNER@DELIVERY.COM', 1,
     'AQAAAAIAAYagAAAAEHamOgBsuoVVSMOBxkNzmQ4d0eRJrZTDiRJpApkQKyXRbJCtO5oBMuaQQVxBaI5kXQ==',
     CAST(NEWID() AS NVARCHAR(MAX)), CAST(NEWID() AS NVARCHAR(MAX)),
     '+12125550101', 0, 0,
     NULL, 1, 0,
     '456 Owner Ave, New York, NY 10002', NULL,
     1, 1, '2024-01-02T00:00:00', '1.png'),

    -- 3 · Customer – John
    (3,
     'john@example.com', 'JOHN@EXAMPLE.COM',
     'john@example.com', 'JOHN@EXAMPLE.COM', 1,
     'AQAAAAIAAYagAAAAEHamOgBsuoVVSMOBxkNzmQ4d0eRJrZTDiRJpApkQKyXRbJCtO5oBMuaQQVxBaI5kXQ==',
     CAST(NEWID() AS NVARCHAR(MAX)), CAST(NEWID() AS NVARCHAR(MAX)),
     '+12125550102', 0, 0,
     NULL, 1, 0,
     '789 Main Blvd, Brooklyn, NY 11201', '4111111111111111',
     1, 1, '2024-01-10T00:00:00', '1.png'),

    -- 4 · Customer – Jane
    (4,
     'jane@example.com', 'JANE@EXAMPLE.COM',
     'jane@example.com', 'JANE@EXAMPLE.COM', 1,
     'AQAAAAIAAYagAAAAEHamOgBsuoVVSMOBxkNzmQ4d0eRJrZTDiRJpApkQKyXRbJCtO5oBMuaQQVxBaI5kXQ==',
     CAST(NEWID() AS NVARCHAR(MAX)), CAST(NEWID() AS NVARCHAR(MAX)),
     '+12125550103', 0, 0,
     NULL, 1, 0,
     '321 Park Ln, Queens, NY 11354', '4111111111111112',
     1, 1, '2024-01-15T00:00:00', '1.png'),

    -- 5 · Delivery Driver
    (5,
     'driver@delivery.com', 'DRIVER@DELIVERY.COM',
     'driver@delivery.com', 'DRIVER@DELIVERY.COM', 1,
     'AQAAAAIAAYagAAAAEHamOgBsuoVVSMOBxkNzmQ4d0eRJrZTDiRJpApkQKyXRbJCtO5oBMuaQQVxBaI5kXQ==',
     CAST(NEWID() AS NVARCHAR(MAX)), CAST(NEWID() AS NVARCHAR(MAX)),
     '+12125550104', 0, 0,
     NULL, 1, 0,
     '999 Driver Rd, Bronx, NY 10451', NULL,
     1, 1, '2024-01-20T00:00:00', '1.png');

SET IDENTITY_INSERT AspNetUsers OFF;

-- ============================================================
-- 3. USER → ROLE ASSIGNMENTS
-- ============================================================
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES
    (1, 1),   -- admin   → Admin
    (2, 2),   -- owner   → RestaurantOwner
    (3, 3),   -- john    → Customer
    (4, 3),   -- jane    → Customer
    (5, 4);   -- driver  → DeliveryDriver

-- ============================================================
-- 4. CATEGORIES
-- ============================================================
SET IDENTITY_INSERT Categories ON;

INSERT INTO Categories (ID, Name, Description, ImageUrl)
VALUES
    (1, 'Fast Food', 'Quick-serve burgers, fries, and sandwiches',     'fastfood.png'),
    (2, 'Italian',   'Pasta, pizza, and classic Italian cuisine',       'italian.png'),
    (3, 'Asian',     'Chinese, Japanese, Thai, and other Asian dishes', 'asian.png'),
    (4, 'Seafood',   'Fresh fish, shrimp, and ocean delicacies',        'seafood.png'),
    (5, 'Desserts',  'Ice cream, cakes, pastries, and sweet treats',    'desserts.png');

SET IDENTITY_INSERT Categories OFF;

-- ============================================================
-- 5. RESTAURANTS
-- NOTE: the table is named "Restaurants" (typo kept from migration)
-- ============================================================
SET IDENTITY_INSERT Restaurants ON;

-- OwnerID (new): points to AspNetUsers.Id of the restaurant owner (UserId=2)
INSERT INTO Restaurants (
    ID, Name, CategoryID, OwnerID, Address, Phone, URLPhoto,
    AverageRating, DeliveryFee, MinimumOrderAmount, PreparationTime,
    OpeningTime, ClosingTime
)
VALUES
    (1, 'Burger Palace', 1, 2, '100 Broadway, New York, NY 10001',  '+1-555-1001', 'burger_palace.png', 4, 2.99, 10, 15, '08:00:00', '23:00:00'),
    (2, 'La Trattoria',  2, 2, '200 Fifth Ave, New York, NY 10010', '+1-555-1002', 'la_trattoria.png',  5, 3.50, 20, 25, '11:00:00', '22:00:00'),
    (3, 'Dragon Wok',    3, 2, '300 Canal St, New York, NY 10013',  '+1-555-1003', 'dragon_wok.png',    4, 1.99, 15, 20, '10:00:00', '21:30:00'),
    (4, 'Ocean Basket',  4, 2, '400 Pier Ave, New York, NY 10004',  '+1-555-1004', 'ocean_basket.png',  4, 4.00, 25, 30, '11:00:00', '22:00:00');

SET IDENTITY_INSERT Restaurants OFF;

-- ============================================================
-- 6. RESTAURANT MENU CATEGORIES
-- ============================================================
SET IDENTITY_INSERT RestaurantMenuCategories ON;

INSERT INTO RestaurantMenuCategories (ID, Name, RestaurantID, URLPhoto)
VALUES
    -- Burger Palace (RestaurantID = 1)
    (1,  'Burgers',     1, 'burgers.png'),
    (2,  'Sides',       1, 'sides.png'),
    (3,  'Drinks',      1, 'drinks.png'),
    -- La Trattoria (RestaurantID = 2)
    (4,  'Pasta',       2, 'pasta.png'),
    (5,  'Pizza',       2, 'pizza.png'),
    (6,  'Starters',    2, 'starters.png'),
    -- Dragon Wok (RestaurantID = 3)
    (7,  'Noodles',     3, 'noodles.png'),
    (8,  'Rice Dishes', 3, 'rice.png'),
    (9,  'Dim Sum',     3, 'dimsum.png'),
    -- Ocean Basket (RestaurantID = 4)
    (10, 'Grilled',     4, 'grilled.png'),
    (11, 'Fried',       4, 'fried.png'),
    (12, 'Salads',      4, 'salads.png');

SET IDENTITY_INSERT RestaurantMenuCategories OFF;

-- ============================================================
-- 7. TABLES
-- ============================================================
SET IDENTITY_INSERT Tables ON;

INSERT INTO Tables (ID, RestaurantID, TableNumber, Capacity, IsAvailable)
VALUES
    -- Burger Palace
    (1,  1, 'T01', 2, 1),
    (2,  1, 'T02', 4, 1),
    (3,  1, 'T03', 6, 1),
    -- La Trattoria
    (4,  2, 'T01', 2, 1),
    (5,  2, 'T02', 4, 1),
    (6,  2, 'T03', 8, 1),
    -- Dragon Wok
    (7,  3, 'T01', 4, 1),
    (8,  3, 'T02', 6, 1),
    -- Ocean Basket
    (9,  4, 'T01', 2, 1),
    (10, 4, 'T02', 4, 1),
    (11, 4, 'T03', 4, 0);   -- currently occupied

SET IDENTITY_INSERT Tables OFF;

-- ============================================================
-- 8. MEALS
-- Each meal must reference a RestaurantMenuCategory that belongs
-- to the same restaurant (FK: Meals.RestaurantID = MenuCat.RestaurantID
-- is not enforced by DB but is required by the app logic).
-- ============================================================
SET IDENTITY_INSERT Meals ON;

INSERT INTO Meals (ID, Name, Description, Price, URLPhoto, RestaurantID, RestaurantMenuCategoryID)
VALUES
    -- Burger Palace → Burgers (MenuCategoryID = 1)
    (1,  'Classic Burger',      'Beef patty, lettuce, tomato, cheddar',          8.99,  'classic_burger.png',  1, 1),
    (2,  'BBQ Bacon Burger',    'Double beef, crispy bacon, smoky BBQ sauce',   11.99,  'bbq_burger.png',      1, 1),
    (3,  'Veggie Burger',       'Plant-based patty, avocado, fresh sprouts',     9.99,  'veggie_burger.png',   1, 1),
    -- Burger Palace → Sides (MenuCategoryID = 2)
    (4,  'French Fries',        'Crispy golden fries with sea salt',             3.49,  'fries.png',           1, 2),
    (5,  'Onion Rings',         'Beer-battered onion rings with dip',            3.99,  'onion_rings.png',     1, 2),
    -- Burger Palace → Drinks (MenuCategoryID = 3)
    (6,  'Cola',                'Chilled cola drink 500 ml',                     1.99,  'cola.png',            1, 3),
    -- La Trattoria → Pasta (MenuCategoryID = 4)
    (7,  'Spaghetti Bolognese', 'Classic slow-cooked meat ragu on spaghetti',   13.99,  'spaghetti.png',       2, 4),
    (8,  'Fettuccine Alfredo',  'Creamy parmesan white-sauce fettuccine',       12.99,  'alfredo.png',         2, 4),
    -- La Trattoria → Pizza (MenuCategoryID = 5)
    (9,  'Margherita Pizza',    'San Marzano tomato, mozzarella, fresh basil',  14.99,  'margherita.png',      2, 5),
    (10, 'Pepperoni Pizza',     'Loaded with premium spicy pepperoni',          15.99,  'pepperoni.png',       2, 5),
    -- La Trattoria → Starters (MenuCategoryID = 6)
    (11, 'Bruschetta',          'Grilled bread, diced tomato, garlic, basil',    6.99,  'bruschetta.png',      2, 6),
    -- Dragon Wok → Noodles (MenuCategoryID = 7)
    (12, 'Pad Thai',            'Stir-fried rice noodles, shrimp, peanuts',     12.99,  'pad_thai.png',        3, 7),
    (13, 'Ramen',               'Rich pork broth, soft-boiled egg, chashu',     13.99,  'ramen.png',           3, 7),
    -- Dragon Wok → Rice Dishes (MenuCategoryID = 8)
    (14, 'Fried Rice',          'Wok-fried jasmine rice, egg, vegetables',       9.99,  'fried_rice.png',      3, 8),
    (15, 'Chicken Teriyaki',    'Glazed chicken thigh over steamed rice',       11.99,  'teriyaki.png',        3, 8),
    -- Ocean Basket → Grilled (MenuCategoryID = 10)
    (16, 'Grilled Salmon',      'Atlantic salmon fillet, lemon butter sauce',   19.99,  'grilled_salmon.png',  4, 10),
    (17, 'Grilled King Prawns', '6 king prawns, garlic butter, lemon wedge',    17.99,  'grilled_prawns.png',  4, 10),
    -- Ocean Basket → Fried (MenuCategoryID = 11)
    (18, 'Fish & Chips',        'Beer-battered cod, thick-cut chips, tartare',  15.99,  'fish_chips.png',      4, 11),
    -- Ocean Basket → Salads (MenuCategoryID = 12)
    (19, 'Prawn Caesar Salad',  'Romaine, prawns, caesar dressing, croutons',   14.99,  'prawn_caesar.png',    4, 12);

SET IDENTITY_INSERT Meals OFF;

-- ============================================================
-- 9. INVENTORIES
-- ============================================================
SET IDENTITY_INSERT Inventories ON;

INSERT INTO Inventories (ID, MealID, RestaurantID, Quantity, LastUpdated)
VALUES
    -- Burger Palace (RestaurantID = 1) meals: 1..6
    (1,  1,  1,  50, GETDATE()),
    (2,  2,  1,  30, GETDATE()),
    (3,  3,  1,  25, GETDATE()),
    (4,  4,  1, 100, GETDATE()),
    (5,  5,  1,  80, GETDATE()),
    (6,  6,  1, 200, GETDATE()),
    -- La Trattoria (RestaurantID = 2) meals: 7..11
    (7,  7,  2,  40, GETDATE()),
    (8,  8,  2,  35, GETDATE()),
    (9,  9,  2,  20, GETDATE()),
    (10, 10, 2,  20, GETDATE()),
    (11, 11, 2,  60, GETDATE()),
    -- Dragon Wok (RestaurantID = 3) meals: 12..15
    (12, 12, 3,  45, GETDATE()),
    (13, 13, 3,  30, GETDATE()),
    (14, 14, 3,  90, GETDATE()),
    (15, 15, 3,  55, GETDATE()),
    -- Ocean Basket (RestaurantID = 4) meals: 16..19
    (16, 16, 4,  25, GETDATE()),
    (17, 17, 4,  30, GETDATE()),
    (18, 18, 4,  35, GETDATE()),
    (19, 19, 4,  20, GETDATE());

SET IDENTITY_INSERT Inventories OFF;

-- ============================================================
-- 10. ORDERS
-- TotalAmounts are verified against the OrderItems below.
--   Order 1: (8.99×2)+(3.49×2)+(1.99×2) = 28.94
--   Order 2: 13.99+(14.99)+(6.99×2)      = 42.96
--   Order 3: 12.99+9.99+13.99            = 36.97
--   Order 4: 19.99+14.99                 = 34.98
-- ============================================================
SET IDENTITY_INSERT Orders ON;

INSERT INTO Orders (
    ID, OrderNumber, UserID, RestaurantID,
    TotalAmount, Status,
    OrderDate, DeliveryAddress, DeliveryFee,
    PaymentMethod, IsPaid, CreatedAt
)
VALUES
    (1, 'ORD-20240301-001', 3, 1,
     28.94, 6,
     '2024-03-01T12:30:00', '789 Main Blvd, Brooklyn, NY 11201', 2.99,
     'CreditCard', 1, '2024-03-01T12:25:00'),

    (2, 'ORD-20240302-001', 4, 2,
     42.96, 6,
     '2024-03-02T19:00:00', '321 Park Ln, Queens, NY 11354', 3.50,
     'Cash', 1, '2024-03-02T18:55:00'),

    (3, 'ORD-20240315-001', 3, 3,
     36.97, 1,
     '2024-03-15T13:00:00', '789 Main Blvd, Brooklyn, NY 11201', 1.99,
     'PayPal', 0, '2024-03-15T12:58:00'),

    (4, 'ORD-20240316-001', 4, 4,
     34.98, 2,
     '2024-03-16T20:00:00', '321 Park Ln, Queens, NY 11354', 4.00,
     'DebitCard', 0, '2024-03-16T19:58:00');

SET IDENTITY_INSERT Orders OFF;

-- ============================================================
-- 11. ORDER ITEMS
-- NOTE: TotalPrice is a computed C# property, NOT stored in DB.
-- ============================================================
SET IDENTITY_INSERT OrderItems ON;

INSERT INTO OrderItems (ID, Name, UnitPrice, Quantity, MealId, OrderId)
VALUES
    -- Order 1 – John @ Burger Palace
    (1,  'Classic Burger',      8.99, 2,  1, 1),
    (2,  'French Fries',        3.49, 2,  4, 1),
    (3,  'Cola',                1.99, 2,  6, 1),
    -- Order 2 – Jane @ La Trattoria
    (4,  'Spaghetti Bolognese', 13.99, 1,  7, 2),
    (5,  'Margherita Pizza',    14.99, 1,  9, 2),
    (6,  'Bruschetta',           6.99, 2, 11, 2),
    -- Order 3 – John @ Dragon Wok
    (7,  'Pad Thai',            12.99, 1, 12, 3),
    (8,  'Fried Rice',           9.99, 1, 14, 3),
    (9,  'Ramen',               13.99, 1, 13, 3),
    -- Order 4 – Jane @ Ocean Basket
    (10, 'Grilled Salmon',      19.99, 1, 16, 4),
    (11, 'Prawn Caesar Salad',  14.99, 1, 19, 4);

SET IDENTITY_INSERT OrderItems OFF;

-- ============================================================
-- 12. PAYMENTS
-- Only orders with IsPaid=1 receive a payment record.
-- Payments.OrderID has a UNIQUE index (one payment per order).
-- PaymentMethod stored as string; PaymentStatus stored as int.
-- CreatedAt has a DB default (GETUTCDATE()) but we supply it
-- explicitly for reproducible seed data.
-- ============================================================
SET IDENTITY_INSERT Payments ON;

INSERT INTO Payments (
    ID, Amount, PaymentDate, PaymentMethod, Status,
    OrderID, TransactionID,
    PaymentToken, BillingAddress, GatewayResponse, GatewayTransactionId,
    CreatedAt, ProcessedAt, CreatedBy, UpdatedAt
)
VALUES
    -- Payment for Order 1 (john, CreditCard, Confirmed=1)
    (1,
     28.94, '2024-03-01T12:35:00', 'CreditCard', 1,
     1, 'TXN-20240301-001',
     NULL, '789 Main Blvd, Brooklyn, NY 11201', 'APPROVED', 'GW-TXN-001',
     '2024-03-01T12:30:00', '2024-03-01T12:35:00', 3, NULL),

    -- Payment for Order 2 (jane, Cash, Confirmed=1)
    (2,
     42.96, '2024-03-02T19:10:00', 'Cash', 1,
     2, 'TXN-20240302-001',
     NULL, '321 Park Ln, Queens, NY 11354', 'APPROVED', 'GW-TXN-002',
     '2024-03-02T19:00:00', '2024-03-02T19:10:00', 4, NULL);

SET IDENTITY_INSERT Payments OFF;

-- ============================================================
-- 13. RESERVATIONS
-- TableID FK → Tables (no cascade), UserID FK → AspNetUsers (cascade),
-- RestaurantID FK → Restaurants (cascade)
-- Tables 5 & 6 belong to Restaurant 2 (La Trattoria).
-- Table 7 belongs to Restaurant 3 (Dragon Wok).
-- ============================================================
SET IDENTITY_INSERT Reservations ON;

INSERT INTO Reservations (
    ID, ReservationDate, TableID, UserID, RestaurantID,
    NumberOfPeople, QRCode, Status,
    StartDate, EndDate
)
VALUES
    -- John reserves Table 5 at La Trattoria – Confirmed
    (1, '2024-04-01T00:00:00', 5, 3, 2,
     2, 'QR-RES-001-20240401', 1,
     '2024-04-01T19:00:00', '2024-04-01T21:00:00'),

    -- Jane reserves Table 6 at La Trattoria – Confirmed
    (2, '2024-04-05T00:00:00', 6, 4, 2,
     6, 'QR-RES-002-20240405', 1,
     '2024-04-05T20:00:00', '2024-04-05T22:30:00'),

    -- John reserves Table 7 at Dragon Wok – Pending
    (3, '2024-04-10T00:00:00', 7, 3, 3,
     4, 'QR-RES-003-20240410', 0,
     '2024-04-10T18:00:00', '2024-04-10T20:00:00');

SET IDENTITY_INSERT Reservations OFF;

-- ============================================================
-- 14. REVIEWS
-- UserID and RestaurantID both have cascade-delete rules.
-- ============================================================
SET IDENTITY_INSERT Reviews ON;

INSERT INTO Reviews (ID, UserID, RestaurantID, Comment, Rating, CreatedAt)
VALUES
    (1, 3, 1, 'Amazing burgers! The BBQ Bacon is a must-try. Super-fast delivery too.', 5, '2024-03-02T14:00:00'),
    (2, 4, 2, 'Authentic Italian flavors. The pasta was cooked to absolute perfection!', 5, '2024-03-03T10:00:00'),
    (3, 3, 3, 'Good food but delivery took a bit longer than estimated. Pad Thai is 10/10.', 4, '2024-03-16T09:00:00'),
    (4, 4, 4, 'Freshest seafood in the city. The grilled king prawns were outstanding.', 4, '2024-03-17T11:00:00'),
    (5, 3, 2, 'The Margherita pizza was heavenly! Perfect thin crust every time.', 5, '2024-03-20T13:00:00');

SET IDENTITY_INSERT Reviews OFF;

-- ============================================================
-- Done.
-- ============================================================
COMMIT TRANSACTION;
