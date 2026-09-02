CREATE TABLE IF NOT EXISTS products (
    product_id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT NOT NULL,
    price NUMERIC(10, 2) NOT NULL,
    stock_quantity INTEGER NOT NULL,
    created_at_utc TIMESTAMPTZ NOT NULL
);

-- Also upgrades an existing products table whose product_id was not a key.
CREATE UNIQUE INDEX IF NOT EXISTS ux_products_product_id
    ON products (product_id);
