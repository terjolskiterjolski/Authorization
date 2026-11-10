INSERT INTO customer_favorites (email, product_id)
SELECT customer.email, product.product_id
FROM customers AS customer
CROSS JOIN products AS product
WHERE LOWER(customer.email) = LOWER(@email)
  AND product.product_id = @productId
ON CONFLICT (email, product_id) DO NOTHING
RETURNING email, product_id, created_at_utc;


