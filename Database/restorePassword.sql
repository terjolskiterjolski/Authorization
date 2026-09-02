UPDATE customers
SET password_hash = @passwordHash,
    password_salt = @passwordSalt
WHERE LOWER(email) = LOWER(@email);
