INSERT INTO Products
    (product_id, name, description, price, stock_quantity, created_at_utc)
SELECT seed.*
FROM (VALUES
    (1, 'Smartphone',    'A modern smartphone with a bright OLED display.', 799.99::numeric, 12, NOW()),
    (2, 'Smartwatch',    'A fitness-focused smartwatch with GPS tracking.', 249.99::numeric, 20, NOW()),
    (3, 'Smart Speaker', 'A compact smart speaker with clear room-filling sound.', 99.99::numeric, 15, NOW()),
    (4, 'Smart Lamp',    'An adjustable smart lamp with app-controlled colors.', 59.99::numeric, 8, NOW()),
    (5, 'Smart Camera',  'A wireless smart camera with night vision.', 129.99::numeric, 10, NOW()),
    (6, 'Smart Display', 'A touchscreen smart display for your home.', 189.99::numeric, 6, NOW())
) AS seed(product_id, name, description, price, stock_quantity, created_at_utc)
WHERE NOT EXISTS (
    SELECT 1 FROM Products product WHERE product.product_id = seed.product_id
);
