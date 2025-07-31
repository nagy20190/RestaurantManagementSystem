-- Sample Data for Delivery Management System
-- Execute this script to populate your database with test data

USE DeliveryManagementSystem_data;
GO

-- Clear existing data (optional - uncomment if you want to start fresh)
-- DELETE FROM Payments;
-- DELETE FROM OrderItems;
-- DELETE FROM Orders;
-- DELETE FROM Reservations;
-- DELETE FROM Reviews;
-- DELETE FROM Inventories;
-- DELETE FROM Meals;
-- DELETE FROM Tables;
-- DELETE FROM RestaurantMenuCategories;
-- DELETE FROM Resturants;
-- DELETE FROM Categories;
-- DELETE FROM AspNetUserRoles;
-- DELETE FROM AspNetUsers;
-- DELETE FROM AspNetRoles;

-- 1. Insert Roles
INSERT INTO AspNetRoles (Name, NormalizedName, ConcurrencyStamp)
VALUES 
    ('SuperAdmin', 'SUPERADMIN', NEWID()),
    ('Admin', 'ADMIN', NEWID()),
    ('User', 'USER', NEWID()),
    ('RestaurantOwner', 'RESTAURANTOWNER', NEWID());

-- 2. Insert Users
INSERT INTO AspNetUsers (UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, Address, CreditCardNumber, IsActive, IsVerified, CreatedAt, ProfileImageUrl)
VALUES 
    ('admin@delivery.com', 'ADMIN@DELIVERY.COM', 'admin@delivery.com', 'ADMIN@DELIVERY.COM', 1, 'AQAAAAIAAYagAAAAELbHvL25z3h+VSIK9KxVEC1xPhEVkMpLZ3g5eKjSLpKjw9wLsQrGmlYJN+acf1GzSw==', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ123456', NEWID(), '+1234567890', 1, 0, 1, 0, '123 Admin Street, City', '4111111111111111', 1, 1, GETUTCDATE(), 'admin.png'),
    ('john.doe@email.com', 'JOHN.DOE@EMAIL.COM', 'john.doe@email.com', 'JOHN.DOE@EMAIL.COM', 1, 'AQAAAAIAAYagAAAAELbHvL25z3h+VSIK9KxVEC1xPhEVkMpLZ3g5eKjSLpKjw9wLsQrGmlYJN+acf1GzSw==', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ123457', NEWID(), '+1234567891', 1, 0, 1, 0, '456 Main Street, Downtown', '4222222222222222', 1, 1, GETUTCDATE(), 'john.png'),
    ('jane.smith@email.com', 'JANE.SMITH@EMAIL.COM', 'jane.smith@email.com', 'JANE.SMITH@EMAIL.COM', 1, 'AQAAAAIAAYagAAAAELbHvL25z3h+VSIK9KxVEC1xPhEVkMpLZ3g5eKjSLpKjw9wLsQrGmlYJN+acf1GzSw==', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ123458', NEWID(), '+1234567892', 1, 0, 1, 0, '789 Oak Avenue, Suburb', '4333333333333333', 1, 1, GETUTCDATE(), 'jane.png'),
    ('mike.wilson@email.com', 'MIKE.WILSON@EMAIL.COM', 'mike.wilson@email.com', 'MIKE.WILSON@EMAIL.COM', 1, 'AQAAAAIAAYagAAAAELbHvL25z3h+VSIK9KxVEC1xPhEVkMpLZ3g5eKjSLpKjw9wLsQrGmlYJN+acf1GzSw==', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ123459', NEWID(), '+1234567893', 1, 0, 1, 0, '321 Pine Road, Village', '4444444444444444', 1, 1, GETUTCDATE(), 'mike.png'),
    ('sarah.jones@email.com', 'SARAH.JONES@EMAIL.COM', 'sarah.jones@email.com', 'SARAH.JONES@EMAIL.COM', 1, 'AQAAAAIAAYagAAAAELbHvL25z3h+VSIK9KxVEC1xPhEVkMpLZ3g5eKjSLpKjw9wLsQrGmlYJN+acf1GzSw==', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ123460', NEWID(), '+1234567894', 1, 0, 1, 0, '654 Elm Street, Town', '4555555555555555', 1, 1, GETUTCDATE(), 'sarah.png');

-- 3. Assign Roles to Users
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.UserName = 'admin@delivery.com' AND r.Name = 'SuperAdmin'
UNION ALL
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.UserName = 'john.doe@email.com' AND r.Name = 'User'
UNION ALL
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.UserName = 'jane.smith@email.com' AND r.Name = 'User'
UNION ALL
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.UserName = 'mike.wilson@email.com' AND r.Name = 'User'
UNION ALL
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.UserName = 'sarah.jones@email.com' AND r.Name = 'User';

-- 4. Insert Categories
INSERT INTO Categories (Name, Description, ImageUrl)
VALUES 
    ('Italian', 'Authentic Italian cuisine with pasta, pizza, and traditional dishes', 'italian.jpg'),
    ('Chinese', 'Traditional Chinese dishes with authentic flavors and ingredients', 'chinese.jpg'),
    ('Mexican', 'Spicy and flavorful Mexican cuisine with tacos, burritos, and more', 'mexican.jpg'),
    ('American', 'Classic American comfort food and fast food favorites', 'american.jpg'),
    ('Japanese', 'Fresh sushi, ramen, and traditional Japanese dishes', 'japanese.jpg'),
    ('Indian', 'Spicy and aromatic Indian cuisine with curry and naan bread', 'indian.jpg'),
    ('Mediterranean', 'Healthy Mediterranean dishes with olive oil and fresh ingredients', 'mediterranean.jpg'),
    ('Thai', 'Spicy and sweet Thai cuisine with rice and noodle dishes', 'thai.jpg');

-- 5. Insert Restaurants
INSERT INTO Resturants (Name, CategoryID, Address, Phone, URLPhoto, AverageRating)
VALUES 
    ('Pizza Palace', 1, '123 Italian Street, Downtown', '+1234567890', 'pizza_palace.jpg', 4),
    ('Golden Dragon', 2, '456 Chinatown Avenue', '+1234567891', 'golden_dragon.jpg', 4),
    ('Taco Fiesta', 3, '789 Mexican Boulevard', '+1234567892', 'taco_fiesta.jpg', 4),
    ('Burger House', 4, '321 American Road', '+1234567893', 'burger_house.jpg', 4),
    ('Sushi Master', 5, '654 Japanese Lane', '+1234567894', 'sushi_master.jpg', 5),
    ('Spice Garden', 6, '987 Indian Street', '+1234567895', 'spice_garden.jpg', 4),
    ('Olive Grove', 7, '147 Mediterranean Way', '+1234567896', 'olive_grove.jpg', 4),
    ('Thai Delight', 8, '258 Thai Avenue', '+1234567897', 'thai_delight.jpg', 4);

-- 6. Insert Restaurant Menu Categories
INSERT INTO RestaurantMenuCategories (Name, ResturantID, URLPhoto)
VALUES 
    ('Appetizers', 1, 'appetizers.jpg'),
    ('Main Courses', 1, 'main_courses.jpg'),
    ('Desserts', 1, 'desserts.jpg'),
    ('Beverages', 1, 'beverages.jpg'),
    ('Starters', 2, 'starters.jpg'),
    ('Entrees', 2, 'entrees.jpg'),
    ('Soups', 2, 'soups.jpg'),
    ('Tacos', 3, 'tacos.jpg'),
    ('Burritos', 3, 'burritos.jpg'),
    ('Burgers', 4, 'burgers.jpg'),
    ('Sides', 4, 'sides.jpg'),
    ('Sushi Rolls', 5, 'sushi_rolls.jpg'),
    ('Sashimi', 5, 'sashimi.jpg'),
    ('Curries', 6, 'curries.jpg'),
    ('Breads', 6, 'breads.jpg'),
    ('Salads', 7, 'salads.jpg'),
    ('Grilled Items', 7, 'grilled.jpg'),
    ('Noodles', 8, 'noodles.jpg'),
    ('Rice Dishes', 8, 'rice_dishes.jpg');

-- 7. Insert Meals
INSERT INTO Meals (Name, Description, Price, URLPhoto, ResturantID)
VALUES 
    -- Pizza Palace Meals
    ('Margherita Pizza', 'Classic tomato sauce with mozzarella cheese', 15.99, 'margherita.jpg', 1),
    ('Pepperoni Pizza', 'Spicy pepperoni with melted cheese', 17.99, 'pepperoni.jpg', 1),
    ('Garlic Bread', 'Fresh baked bread with garlic butter', 5.99, 'garlic_bread.jpg', 1),
    ('Caesar Salad', 'Fresh romaine lettuce with Caesar dressing', 8.99, 'caesar_salad.jpg', 1),
    
    -- Golden Dragon Meals
    ('Kung Pao Chicken', 'Spicy chicken with peanuts and vegetables', 16.99, 'kung_pao.jpg', 2),
    ('Sweet and Sour Pork', 'Crispy pork with sweet and sour sauce', 15.99, 'sweet_sour.jpg', 2),
    ('Fried Rice', 'Steamed rice with vegetables and soy sauce', 9.99, 'fried_rice.jpg', 2),
    ('Wonton Soup', 'Clear broth with wonton dumplings', 6.99, 'wonton_soup.jpg', 2),
    
    -- Taco Fiesta Meals
    ('Beef Tacos', 'Three soft tacos with seasoned beef', 12.99, 'beef_tacos.jpg', 3),
    ('Chicken Burrito', 'Large burrito with grilled chicken', 14.99, 'chicken_burrito.jpg', 3),
    ('Guacamole', 'Fresh avocado dip with chips', 4.99, 'guacamole.jpg', 3),
    ('Mexican Rice', 'Seasoned rice with tomatoes and spices', 5.99, 'mexican_rice.jpg', 3),
    
    -- Burger House Meals
    ('Classic Burger', 'Beef patty with lettuce, tomato, and cheese', 13.99, 'classic_burger.jpg', 4),
    ('Bacon Cheeseburger', 'Burger with bacon and extra cheese', 16.99, 'bacon_burger.jpg', 4),
    ('French Fries', 'Crispy golden fries', 4.99, 'french_fries.jpg', 4),
    ('Onion Rings', 'Breaded and fried onion rings', 5.99, 'onion_rings.jpg', 4),
    
    -- Sushi Master Meals
    ('California Roll', 'Crab, avocado, and cucumber roll', 12.99, 'california_roll.jpg', 5),
    ('Salmon Nigiri', 'Fresh salmon over seasoned rice', 8.99, 'salmon_nigiri.jpg', 5),
    ('Spicy Tuna Roll', 'Spicy tuna with cucumber', 14.99, 'spicy_tuna.jpg', 5),
    ('Miso Soup', 'Traditional Japanese soup', 3.99, 'miso_soup.jpg', 5),
    
    -- Spice Garden Meals
    ('Butter Chicken', 'Creamy tomato-based curry with chicken', 18.99, 'butter_chicken.jpg', 6),
    ('Naan Bread', 'Soft and fluffy Indian bread', 2.99, 'naan.jpg', 6),
    ('Biryani Rice', 'Fragrant rice with spices and vegetables', 16.99, 'biryani.jpg', 6),
    ('Samosa', 'Crispy pastry with spiced potatoes', 4.99, 'samosa.jpg', 6),
    
    -- Olive Grove Meals
    ('Greek Salad', 'Fresh vegetables with feta cheese', 10.99, 'greek_salad.jpg', 7),
    ('Grilled Chicken', 'Herb-marinated grilled chicken', 19.99, 'grilled_chicken.jpg', 7),
    ('Hummus', 'Chickpea dip with olive oil', 6.99, 'hummus.jpg', 7),
    ('Falafel', 'Crispy chickpea fritters', 8.99, 'falafel.jpg', 7),
    
    -- Thai Delight Meals
    ('Pad Thai', 'Stir-fried rice noodles with shrimp', 15.99, 'pad_thai.jpg', 8),
    ('Green Curry', 'Spicy green curry with coconut milk', 17.99, 'green_curry.jpg', 8),
    ('Thai Fried Rice', 'Jasmine rice with vegetables and egg', 12.99, 'thai_rice.jpg', 8),
    ('Tom Yum Soup', 'Spicy and sour soup with shrimp', 9.99, 'tom_yum.jpg', 8);

-- 8. Insert Tables
INSERT INTO Tables (ResturantID, Capacity, IsAvailable)
VALUES 
    -- Pizza Palace Tables
    (1, 4, 1), (1, 4, 1), (1, 6, 1), (1, 2, 1), (1, 8, 1),
    -- Golden Dragon Tables
    (2, 4, 1), (2, 4, 1), (2, 6, 1), (2, 2, 1), (2, 10, 1),
    -- Taco Fiesta Tables
    (3, 4, 1), (3, 4, 1), (3, 6, 1), (3, 2, 1), (3, 8, 1),
    -- Burger House Tables
    (4, 4, 1), (4, 4, 1), (4, 6, 1), (4, 2, 1), (4, 8, 1),
    -- Sushi Master Tables
    (5, 4, 1), (5, 4, 1), (5, 6, 1), (5, 2, 1), (5, 8, 1),
    -- Spice Garden Tables
    (6, 4, 1), (6, 4, 1), (6, 6, 1), (6, 2, 1), (6, 8, 1),
    -- Olive Grove Tables
    (7, 4, 1), (7, 4, 1), (7, 6, 1), (7, 2, 1), (7, 8, 1),
    -- Thai Delight Tables
    (8, 4, 1), (8, 4, 1), (8, 6, 1), (8, 2, 1), (8, 8, 1);

-- 9. Insert Inventory
INSERT INTO Inventories (MealID, Quantity, LastUpdated)
VALUES 
    (1, 50, GETDATE()), (2, 45, GETDATE()), (3, 30, GETDATE()), (4, 25, GETDATE()),
    (5, 40, GETDATE()), (6, 35, GETDATE()), (7, 60, GETDATE()), (8, 20, GETDATE()),
    (9, 55, GETDATE()), (10, 40, GETDATE()), (11, 30, GETDATE()), (12, 50, GETDATE()),
    (13, 45, GETDATE()), (14, 35, GETDATE()), (15, 80, GETDATE()), (16, 25, GETDATE()),
    (17, 30, GETDATE()), (18, 25, GETDATE()), (19, 35, GETDATE()), (20, 40, GETDATE()),
    (21, 30, GETDATE()), (22, 100, GETDATE()), (23, 40, GETDATE()), (24, 35, GETDATE()),
    (25, 30, GETDATE()), (26, 25, GETDATE()), (27, 20, GETDATE()), (28, 30, GETDATE()),
    (29, 35, GETDATE()), (30, 30, GETDATE()), (31, 45, GETDATE()), (32, 25, GETDATE());

-- 10. Insert Orders
INSERT INTO Orders (OrderNumber, UserID, TotalAmount, Status, OrderDate, DeliveryAddress, DeliveryFee, PaymentMethod, IsPaid, CreatedAt)
VALUES 
    ('ORD-001', 2, 25.98, 1, DATEADD(day, -5, GETDATE()), '456 Main Street, Downtown', 3.99, 'CreditCard', 1, DATEADD(day, -5, GETDATE())),
    ('ORD-002', 3, 32.97, 2, DATEADD(day, -4, GETDATE()), '789 Oak Avenue, Suburb', 3.99, 'PayPal', 1, DATEADD(day, -4, GETDATE())),
    ('ORD-003', 4, 18.98, 3, DATEADD(day, -3, GETDATE()), '321 Pine Road, Village', 3.99, 'Cash', 0, DATEADD(day, -3, GETDATE())),
    ('ORD-004', 5, 45.96, 4, DATEADD(day, -2, GETDATE()), '654 Elm Street, Town', 3.99, 'CreditCard', 1, DATEADD(day, -2, GETDATE())),
    ('ORD-005', 2, 28.97, 5, DATEADD(day, -1, GETDATE()), '456 Main Street, Downtown', 3.99, 'DebitCard', 1, DATEADD(day, -1, GETDATE())),
    ('ORD-006', 3, 22.98, 6, GETDATE(), '789 Oak Avenue, Suburb', 3.99, 'CreditCard', 0, GETDATE()),
    ('ORD-007', 4, 35.97, 1, GETDATE(), '321 Pine Road, Village', 3.99, 'PayPal', 0, GETDATE()),
    ('ORD-008', 5, 19.98, 2, GETDATE(), '654 Elm Street, Town', 3.99, 'Cash', 0, GETDATE());

-- 11. Insert Order Items
INSERT INTO OrderItems (OrderID, MealID, Quantity, UnitPrice)
VALUES 
    (1, 1, 1, 15.99), (1, 3, 1, 5.99), (1, 4, 1, 8.99),
    (2, 5, 1, 16.99), (2, 7, 1, 9.99), (2, 8, 1, 6.99),
    (3, 9, 1, 12.99), (3, 11, 1, 4.99),
    (4, 13, 1, 13.99), (4, 14, 1, 16.99), (4, 15, 1, 4.99), (4, 16, 1, 5.99),
    (5, 17, 1, 12.99), (5, 18, 1, 8.99), (5, 20, 1, 3.99),
    (6, 21, 1, 18.99), (6, 22, 1, 2.99),
    (7, 25, 1, 10.99), (7, 26, 1, 19.99), (7, 28, 1, 8.99),
    (8, 29, 1, 15.99), (8, 32, 1, 9.99);

-- 12. Insert Payments
INSERT INTO Payments (OrderID, Amount, PaymentDate, PaymentMethod, Status, TransactionID, PaymentToken, BillingAddress, GatewayResponse, GatewayTransactionId, CreatedAt, ProcessedAt, CreatedBy)
VALUES 
    (1, 25.98, DATEADD(day, -5, GETDATE()), 0, 1, 'TXN-001', 'tok_123456', '456 Main Street, Downtown', 'Payment successful', 'gw_001', DATEADD(day, -5, GETDATE()), DATEADD(day, -5, GETDATE()), 2),
    (2, 32.97, DATEADD(day, -4, GETDATE()), 4, 1, 'TXN-002', 'tok_123457', '789 Oak Avenue, Suburb', 'Payment successful', 'gw_002', DATEADD(day, -4, GETDATE()), DATEADD(day, -4, GETDATE()), 3),
    (4, 45.96, DATEADD(day, -2, GETDATE()), 0, 1, 'TXN-003', 'tok_123458', '654 Elm Street, Town', 'Payment successful', 'gw_003', DATEADD(day, -2, GETDATE()), DATEADD(day, -2, GETDATE()), 5),
    (5, 28.97, DATEADD(day, -1, GETDATE()), 1, 1, 'TXN-004', 'tok_123459', '456 Main Street, Downtown', 'Payment successful', 'gw_004', DATEADD(day, -1, GETDATE()), DATEADD(day, -1, GETDATE()), 2);

-- 13. Insert Reviews
INSERT INTO Reviews (UserID, ResturantID, Comment, Rating, CreatedAt)
VALUES 
    (2, 1, 'Great pizza! The crust was perfect and the toppings were fresh.', 5, DATEADD(day, -4, GETDATE())),
    (3, 2, 'Authentic Chinese food. The Kung Pao chicken was amazing!', 4, DATEADD(day, -3, GETDATE())),
    (4, 3, 'Best tacos in town! Fresh ingredients and great service.', 5, DATEADD(day, -2, GETDATE())),
    (5, 4, 'Classic burgers done right. Fries were crispy and delicious.', 4, DATEADD(day, -1, GETDATE())),
    (2, 5, 'Fresh sushi and excellent presentation. Highly recommended!', 5, GETDATE()),
    (3, 6, 'Spicy and flavorful Indian food. The butter chicken was perfect.', 4, GETDATE()),
    (4, 7, 'Healthy Mediterranean options. The Greek salad was refreshing.', 4, GETDATE()),
    (5, 8, 'Authentic Thai flavors. The Pad Thai was delicious!', 5, GETDATE());

-- 14. Insert Reservations
INSERT INTO Reservations (ReservationDate, TableID, UserID, NumberOfPeople, QRCode, Status, StartDate, EndDate)
VALUES 
    (DATEADD(day, 1, GETDATE()), 1, 2, 4, 'QR-001', 1, DATEADD(day, 1, GETDATE()), DATEADD(day, 1, DATEADD(hour, 2, GETDATE()))),
    (DATEADD(day, 2, GETDATE()), 6, 3, 2, 'QR-002', 1, DATEADD(day, 2, GETDATE()), DATEADD(day, 2, DATEADD(hour, 1, GETDATE()))),
    (DATEADD(day, 3, GETDATE()), 11, 4, 6, 'QR-003', 1, DATEADD(day, 3, GETDATE()), DATEADD(day, 3, DATEADD(hour, 3, GETDATE()))),
    (DATEADD(day, 4, GETDATE()), 16, 5, 2, 'QR-004', 1, DATEADD(day, 4, GETDATE()), DATEADD(day, 4, DATEADD(hour, 1, GETDATE()))),
    (DATEADD(day, 5, GETDATE()), 21, 2, 4, 'QR-005', 1, DATEADD(day, 5, GETDATE()), DATEADD(day, 5, DATEADD(hour, 2, GETDATE()))),
    (DATEADD(day, 6, GETDATE()), 26, 3, 8, 'QR-006', 1, DATEADD(day, 6, GETDATE()), DATEADD(day, 6, DATEADD(hour, 3, GETDATE()))),
    (DATEADD(day, 7, GETDATE()), 31, 4, 2, 'QR-007', 1, DATEADD(day, 7, GETDATE()), DATEADD(day, 7, DATEADD(hour, 1, GETDATE()))),
    (DATEADD(day, 8, GETDATE()), 36, 5, 4, 'QR-008', 1, DATEADD(day, 8, GETDATE()), DATEADD(day, 8, DATEADD(hour, 2, GETDATE())));

PRINT 'Sample data has been successfully inserted into the database!';
PRINT 'Total records inserted:';
PRINT '- Roles: 4';
PRINT '- Users: 5';
PRINT '- Categories: 8';
PRINT '- Restaurants: 8';
PRINT '- Restaurant Menu Categories: 19';
PRINT '- Meals: 32';
PRINT '- Tables: 40';
PRINT '- Inventory: 32';
PRINT '- Orders: 8';
PRINT '- Order Items: 21';
PRINT '- Payments: 4';
PRINT '- Reviews: 8';
PRINT '- Reservations: 8'; 