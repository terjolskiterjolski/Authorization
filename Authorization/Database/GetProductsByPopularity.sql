SELECT product_id, name, description, price, stock_quantity, created_at_utc, rating, inquiry_frequency, sex, photo_path
FROM products ORDER BY inquiry_frequency DESC;
