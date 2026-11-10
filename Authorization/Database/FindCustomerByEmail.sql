SELECT customer_id, name, email, phone, address, password_hash, password_salt
FROM customers
WHERE LOWER(email) = LOWER(@email);
