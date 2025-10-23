-- ENUM типы
CREATE TYPE color AS ENUM ('red', 'green', 'blue');
CREATE TYPE size AS ENUM ('small', 'medium', 'large');

-- Композитный тип
CREATE TYPE product_info AS (
    name VARCHAR(255),
    price NUMERIC(10, 2),
    in_stock BOOLEAN
);
