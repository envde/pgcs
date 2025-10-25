-- Test with arrays

CREATE TABLE users
(
    id            BIGSERIAL PRIMARY KEY,
    username      VARCHAR(50) NOT NULL,
    phone_numbers VARCHAR(20)[],
    tags          TEXT[]
);
