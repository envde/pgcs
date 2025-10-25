-- Test with ENUM

CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended');

CREATE TABLE users
(
    id       BIGSERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    status   user_status NOT NULL DEFAULT 'active'
);
