SELECT product_id, name, description, price, stock_quantity, created_at_utc, rating, inquiry_frequency, sex, photo_path
FROM Products
WHERE name ILIKE @prefix || '%'
