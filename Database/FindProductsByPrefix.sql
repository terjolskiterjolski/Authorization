SELECT product_id, name, description, price, stock_quantity, created_at_utc
FROM Products
WHERE name ILIKE @prefix || '%'
