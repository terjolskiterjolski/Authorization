CREATE TABLE IF NOT EXISTS customer_favorites (
    email TEXT NOT NULL,
    product_id INTEGER NOT NULL,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT pk_customer_favorites
        PRIMARY KEY (email, product_id),
    CONSTRAINT fk_customer_favorites_customer
        FOREIGN KEY (email) REFERENCES customers (email) ON DELETE CASCADE,
    CONSTRAINT fk_customer_favorites_product
        FOREIGN KEY (product_id) REFERENCES products (product_id) ON DELETE CASCADE
);

-- Upgrade a table created with favorite_id as its primary key.
ALTER TABLE customer_favorites
    DROP CONSTRAINT IF EXISTS customer_favorites_pkey;

ALTER TABLE customer_favorites
    DROP COLUMN IF EXISTS favorite_id;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conrelid = 'customer_favorites'::regclass
          AND contype = 'p'
    ) THEN
        ALTER TABLE customer_favorites
            ADD CONSTRAINT pk_customer_favorites PRIMARY KEY (email, product_id);
    END IF;
END $$;
