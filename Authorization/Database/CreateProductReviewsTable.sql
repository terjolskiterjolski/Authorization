-- Legacy customer data contains duplicate customer_id values. The email
-- disambiguates the customer without changing existing customer records.
CREATE UNIQUE INDEX IF NOT EXISTS ux_customers_customer_id_email
    ON customers (customer_id, email);

CREATE TABLE IF NOT EXISTS product_reviews (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES products (product_id),
    customer_id INTEGER NOT NULL,
    customer_email TEXT NOT NULL,
    rating INTEGER NOT NULL CHECK (rating BETWEEN 1 AND 5),
    message TEXT NOT NULL CHECK (LENGTH(TRIM(message)) > 0),
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (customer_id, customer_email)
        REFERENCES customers (customer_id, email),
    UNIQUE (customer_id, customer_email, product_id)
);

CREATE INDEX IF NOT EXISTS ix_product_reviews_product_id
    ON product_reviews (product_id);
