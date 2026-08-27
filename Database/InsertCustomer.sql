INSERT INTO customers
    (customer_id, name, email, phone, address, password_hash, password_salt, created_at_utc)
VALUES
    (@customerId, @name, @email, @phone, @address, @passwordHash, @passwordSalt, @createdAtUtc);
