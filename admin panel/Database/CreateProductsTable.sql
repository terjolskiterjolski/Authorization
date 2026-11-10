-- For new databases; the existing products table does not need to be recreated.
CREATE TABLE IF NOT EXISTS public.products (
    product_id INT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT NOT NULL,
    price NUMERIC NOT NULL,
    stock_quantity INT NOT NULL,
    created_at_utc TIMESTAMPTZ NOT NULL,
    sex TEXT NOT NULL CHECK (sex IN ('Men', 'Women', 'Unisex')),
    rating INT NOT NULL DEFAULT 0,
    inquiry_frequency INT NOT NULL DEFAULT 0
);
