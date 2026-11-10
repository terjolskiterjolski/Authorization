-- Sample content, linked to actual customers and products. Safe to rerun.
WITH selected_customer AS (
    SELECT customer_id, email
    FROM customers
    ORDER BY customer_id, email
    LIMIT 1
), selected_products AS (
    SELECT product_id, ROW_NUMBER() OVER (ORDER BY product_id) AS sample_number
    FROM products
    ORDER BY product_id
    LIMIT 5   
), sample_reviews (sample_number, rating, message) AS (
    VALUES
        (1, 5, 'Lovely fragrance with a fresh floral opening. Very happy with it.'),
        (2, 4, 'Pleasant scent for everyday use. Would prefer it to last a little longer.'),
        (3, 5, 'Warm and inviting scent. A great choice for the evening.'),
        (4, 4, 'Light and refreshing fragrance, especially nice during the day.'),
        (5, 3, 'Good quality, but the scent is stronger than I expected.')
)
INSERT INTO product_reviews (product_id, customer_id, customer_email, rating, message)
SELECT p.product_id, c.customer_id, c.email, r.rating, r.message
FROM selected_products p
JOIN sample_reviews r ON r.sample_number = p.sample_number
CROSS JOIN selected_customer c
ON CONFLICT (customer_id, customer_email, product_id) DO NOTHING;
