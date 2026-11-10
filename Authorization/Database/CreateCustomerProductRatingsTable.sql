CREATE TABLE IF NOT EXISTS customer_product_ratings (
    customer_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL REFERENCES products (product_id),
    rating INTEGER NOT NULL CHECK (rating BETWEEN 1 AND 5),
    PRIMARY KEY (customer_id, product_id)
);
