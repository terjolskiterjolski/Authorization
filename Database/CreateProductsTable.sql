create TABLE IF NOT EXISTS Products (
    product_id INTEGER NOT NULL,
    name TEXT NOT NULL,
    description TEXT NOT NULL,
    price NUMERIC(10, 2) NOT NULL,
    stock_quantity INTEGER NOT NULL,
    created_at_utc TIMESTAMPTZ NOT NULL
);