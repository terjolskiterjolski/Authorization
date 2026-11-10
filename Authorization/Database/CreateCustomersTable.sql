CREATE TABLE IF NOT EXISTS customers (
    email TEXT PRIMARY KEY,
    customer_id INTEGER NOT NULL UNIQUE,
    name TEXT NOT NULL,
    phone TEXT NOT NULL,
    address TEXT NOT NULL,
    password_hash BYTEA NOT NULL,
    password_salt BYTEA NOT NULL,
    created_at_utc TIMESTAMPTZ NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_customers_email_lower
    ON customers (LOWER(email));

-- CREATE TABLE IF NOT EXISTS does not add the primary key to databases that
-- already have an older customers table. A foreign key can reference a plain
-- unique index, but not the expression index on LOWER(email) above.
CREATE UNIQUE INDEX IF NOT EXISTS ux_customers_email
    ON customers (email);
