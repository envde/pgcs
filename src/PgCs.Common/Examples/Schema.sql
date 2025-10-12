-- ==================================================================
-- Пример схемы PostgreSQL для демонстрации возможностей PgCs
-- ==================================================================

-- Создание ENUM типов
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');
CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');
CREATE TYPE payment_method AS ENUM ('credit_card', 'paypal', 'bank_transfer', 'crypto');

-- Создание композитного типа
CREATE TYPE address AS (
    street VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(50),
    zip_code VARCHAR(20),
    country VARCHAR(50)
    );

-- Создание DOMAIN (пользовательский тип с ограничением)
CREATE DOMAIN email AS VARCHAR(255)
    CHECK (VALUE ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');

-- ==================================================================
-- Таблица пользователей
-- ==================================================================
CREATE TABLE users
(
    id              BIGSERIAL PRIMARY KEY,
    username        VARCHAR(50)              NOT NULL UNIQUE,
    email           email                    NOT NULL UNIQUE,
    password_hash   VARCHAR(255)             NOT NULL,
    full_name       VARCHAR(255),
    status          user_status              NOT NULL DEFAULT 'active',

    -- JSON данные
    preferences     JSONB                             DEFAULT '{}',
    metadata        JSON,

    -- Композитный тип
    billing_address address,

    -- Массивы
    phone_numbers   VARCHAR(20)[],
    tags            TEXT[],

    -- Geo данные (требуется PostGIS расширение)
    -- location GEOGRAPHY(POINT, 4326),

    -- Временные метки
    created_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_login_at   TIMESTAMP WITH TIME ZONE,

    -- Числовые типы
    balance         NUMERIC(12, 2)                    DEFAULT 0.00,
    loyalty_points  INTEGER                           DEFAULT 0,

    -- Булево
    is_verified     BOOLEAN                           DEFAULT FALSE,
    is_premium      BOOLEAN                           DEFAULT FALSE,

    -- Бинарные данные
    avatar          BYTEA,

    -- UUID
    external_id     UUID                              DEFAULT gen_random_uuid(),

    -- Дата без времени
    date_of_birth   DATE,

    -- Интервал
    subscription_duration INTERVAL,

    -- CHECK constraint
    CONSTRAINT check_balance CHECK (balance >= 0),
    CONSTRAINT check_age CHECK (date_of_birth IS NULL OR date_of_birth < CURRENT_DATE - INTERVAL '13 years'
)
    );

-- Комментарии к таблице и колонкам
COMMENT
ON TABLE users IS 'Таблица пользователей системы';
COMMENT
ON COLUMN users.preferences IS 'JSON объект с пользовательскими настройками';
COMMENT
ON COLUMN users.balance IS 'Баланс пользователя в основной валюте';

-- Индексы
CREATE INDEX idx_users_email ON users (email);
CREATE INDEX idx_users_status ON users (status) WHERE status = 'active';
CREATE INDEX idx_users_created_at ON users (created_at DESC);
CREATE INDEX idx_users_preferences ON users USING gin(preferences);
CREATE UNIQUE INDEX idx_users_external_id ON users (external_id);

-- Частичный индекс
CREATE INDEX idx_users_premium ON users (id) WHERE is_premium = TRUE;

-- ==================================================================
-- Таблица категорий товаров
-- ==================================================================
CREATE TABLE categories
(
    id            SERIAL PRIMARY KEY,
    name          VARCHAR(100)             NOT NULL,
    slug          VARCHAR(100)             NOT NULL UNIQUE,
    description   TEXT,
    parent_id     INTEGER,

    -- Для древовидной структуры (ltree требует расширение)
    -- path LTREE,

    -- Полнотекстовый поиск
    search_vector TSVECTOR,

    created_at    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),

    -- Self-referencing foreign key
    CONSTRAINT fk_parent_category FOREIGN KEY (parent_id)
        REFERENCES categories (id)
        ON DELETE CASCADE
);

COMMENT
ON TABLE categories IS 'Иерархическая структура категорий товаров';

-- GIN индекс для полнотекстового поиска
CREATE INDEX idx_categories_search ON categories USING gin(search_vector);

-- Триггер для автоматического обновления search_vector
CREATE
OR REPLACE FUNCTION update_category_search_vector()
RETURNS TRIGGER AS $$
BEGIN
    NEW.search_vector
:= 
        setweight(to_tsvector('english', COALESCE(NEW.name, '')), 'A') ||
        setweight(to_tsvector('english', COALESCE(NEW.description, '')), 'B');
RETURN NEW;
END;
$$
LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_category_search
    BEFORE INSERT OR
UPDATE OF name, description
ON categories
    FOR EACH ROW
    EXECUTE FUNCTION update_category_search_vector();

-- ==================================================================
-- Таблица заказов
-- ==================================================================
CREATE TABLE orders
(
    id               BIGSERIAL PRIMARY KEY,
    user_id          BIGINT                   NOT NULL,
    order_number     VARCHAR(50)              NOT NULL UNIQUE,
    status           order_status             NOT NULL DEFAULT 'pending',

    -- Денежные значения
    subtotal         NUMERIC(12, 2)           NOT NULL,
    tax              NUMERIC(12, 2)           NOT NULL DEFAULT 0,
    shipping_cost    NUMERIC(12, 2)           NOT NULL DEFAULT 0,
    discount         NUMERIC(12, 2)           NOT NULL DEFAULT 0,
    total            NUMERIC(12, 2)           NOT NULL,

    -- JSONB для гибкости
    shipping_address JSONB                    NOT NULL,
    billing_address  JSONB                    NOT NULL,

    payment_method   payment_method           NOT NULL,
    payment_details  JSONB,

    -- Массив для хранения истории статусов
    status_history   JSONB                             DEFAULT '[]',

    -- Временные метки
    created_at       TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    shipped_at       TIMESTAMP WITH TIME ZONE,
    delivered_at     TIMESTAMP WITH TIME ZONE,

    -- Диапазон дат (для даты доставки)
    delivery_window  TSTZRANGE,

    notes            TEXT,

    -- Foreign key
    CONSTRAINT fk_orders_user FOREIGN KEY (user_id)
        REFERENCES users (id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,

    -- Constraints
    CONSTRAINT check_total CHECK (total >= 0),
    CONSTRAINT check_dates CHECK (
        (shipped_at IS NULL OR shipped_at >= created_at) AND
        (delivered_at IS NULL OR delivered_at >= shipped_at)
        )
);

COMMENT
ON TABLE orders IS 'Заказы пользователей';
COMMENT
ON COLUMN orders.status_history IS 'История изменений статуса заказа в формате [{status, timestamp, user_id}]';

-- Индексы
CREATE INDEX idx_orders_user_id ON orders (user_id);
CREATE INDEX idx_orders_status ON orders (status);
CREATE INDEX idx_orders_created_at ON orders (created_at DESC);
CREATE INDEX idx_orders_order_number ON orders (order_number);
CREATE INDEX idx_orders_shipping_address ON orders USING gin(shipping_address);

-- Составной индекс
CREATE INDEX idx_orders_user_status ON orders (user_id, status);

-- ==================================================================
-- Таблица элементов заказа
-- ==================================================================
CREATE TABLE order_items
(
    id               BIGSERIAL PRIMARY KEY,
    order_id         BIGINT                   NOT NULL,
    category_id      INTEGER                  NOT NULL,

    product_name     VARCHAR(255)             NOT NULL,
    sku              VARCHAR(100),
    quantity         INTEGER                  NOT NULL,
    unit_price       NUMERIC(10, 2)           NOT NULL,
    discount_percent NUMERIC(5, 2)                     DEFAULT 0,
    total_price      NUMERIC(12, 2)           NOT NULL,

    -- JSONB для атрибутов продукта (размер, цвет и т.д.)
    attributes       JSONB,

    created_at       TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),

    -- Foreign keys
    CONSTRAINT fk_order_items_order FOREIGN KEY (order_id)
        REFERENCES orders (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    CONSTRAINT fk_order_items_category FOREIGN KEY (category_id)
        REFERENCES categories (id)
        ON DELETE RESTRICT,

    -- Constraints
    CONSTRAINT check_quantity CHECK (quantity > 0),
    CONSTRAINT check_unit_price CHECK (unit_price >= 0),
    CONSTRAINT check_discount CHECK (discount_percent >= 0 AND discount_percent <= 100)
);

COMMENT
ON TABLE order_items IS 'Позиции в заказах';

-- Индексы
CREATE INDEX idx_order_items_order_id ON order_items (order_id);
CREATE INDEX idx_order_items_category_id ON order_items (category_id);
CREATE INDEX idx_order_items_sku ON order_items (sku);

-- ==================================================================
-- VIEW: Активные пользователи с заказами
-- ==================================================================
CREATE VIEW active_users_with_orders AS
SELECT u.id,
       u.username,
       u.email,
       u.status,
       u.is_premium,
       u.balance,
       COUNT(o.id)                  AS total_orders,
       SUM(o.total)                 AS total_spent,
       MAX(o.created_at)            AS last_order_date,
       ARRAY_AGG(DISTINCT o.status) AS order_statuses
FROM users u
         LEFT JOIN orders o ON u.id = o.user_id
WHERE u.status = 'active'
GROUP BY u.id, u.username, u.email, u.status, u.is_premium, u.balance;

COMMENT
ON VIEW active_users_with_orders IS 'Активные пользователи с агрегированной информацией о заказах';

-- ==================================================================
-- Материализованное представление: Статистика по категориям
-- ==================================================================
CREATE
MATERIALIZED VIEW category_statistics AS
SELECT c.id,
       c.name,
       c.slug,
       COUNT(oi.id)                AS total_items_sold,
       SUM(oi.quantity)            AS total_quantity,
       SUM(oi.total_price)         AS total_revenue,
       AVG(oi.unit_price)          AS avg_price,
       COUNT(DISTINCT oi.order_id) AS unique_orders
FROM categories c
         LEFT JOIN order_items oi ON c.id = oi.category_id
GROUP BY c.id, c.name, c.slug;

CREATE UNIQUE INDEX idx_category_statistics_id ON category_statistics (id);

COMMENT
ON MATERIALIZED VIEW category_statistics IS 'Статистика продаж по категориям (требует REFRESH)';

-- ==================================================================
-- Функции
-- ==================================================================

-- Функция для получения полного пути категории
CREATE
OR REPLACE FUNCTION get_category_path(category_id INTEGER)
RETURNS TEXT AS $$
DECLARE
result TEXT := '';
    current_id
INTEGER := category_id;
    current_name
VARCHAR(100);
    parent
INTEGER;
BEGIN
    LOOP
SELECT name, parent_id
INTO current_name, parent
FROM categories
WHERE id = current_id;

EXIT
WHEN current_name IS NULL;
        
        result
:= current_name || 
            CASE WHEN result = '' THEN '' ELSE ' > ' || result
END;
        
        EXIT
WHEN parent IS NULL;
        current_id
:= parent;
END LOOP;

RETURN result;
END;
$$
LANGUAGE plpgsql IMMUTABLE;

COMMENT
ON FUNCTION get_category_path IS 'Возвращает полный путь категории от корня';

-- Функция для обновления updated_at
CREATE
OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at
= NOW();
RETURN NEW;
END;
$$
LANGUAGE plpgsql;

-- Триггеры для автоматического обновления updated_at
CREATE TRIGGER trigger_users_updated_at
    BEFORE UPDATE
    ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trigger_orders_updated_at
    BEFORE UPDATE
    ON orders
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- ==================================================================
-- Партиционирование (пример для больших таблиц)
-- ==================================================================

-- Партиционированная таблица для логов (по датам)
CREATE TABLE audit_logs
(
    id          BIGSERIAL,
    user_id     BIGINT,
    action      VARCHAR(50)              NOT NULL,
    entity_type VARCHAR(50),
    entity_id   BIGINT,
    changes     JSONB,
    ip_address  INET,
    user_agent  TEXT,
    created_at  TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    PRIMARY KEY (id, created_at)
) PARTITION BY RANGE (created_at);

-- Создание партиций
CREATE TABLE audit_logs_2024_q1 PARTITION OF audit_logs
    FOR VALUES FROM
(
    '2024-01-01'
) TO
(
    '2024-04-01'
);

CREATE TABLE audit_logs_2024_q2 PARTITION OF audit_logs
    FOR VALUES FROM
(
    '2024-04-01'
) TO
(
    '2024-07-01'
);

-- Индекс на партиционированной таблице
CREATE INDEX idx_audit_logs_user_id ON audit_logs (user_id);
CREATE INDEX idx_audit_logs_created_at ON audit_logs (created_at);

COMMENT
ON TABLE audit_logs IS 'Партиционированная таблица аудита действий пользователей';

-- ==================================================================
-- Тестовые данные
-- ==================================================================

-- Вставка пользователей
INSERT INTO users (username, email, full_name, status, preferences, phone_numbers, tags, balance, is_verified,
                   is_premium, date_of_birth)
VALUES ('john_doe', 'john@example.com', 'John Doe', 'active', '{
  "theme": "dark",
  "language": "en"
}', ARRAY['+1234567890', '+1987654321'], ARRAY['vip', 'early_adopter'], 1500.50, TRUE, TRUE, '1990-05-15'),
       ('jane_smith', 'jane@example.com', 'Jane Smith', 'active', '{
         "theme": "light",
         "notifications": true
       }', ARRAY['+1555666777'], ARRAY['premium'], 500.00, TRUE, FALSE, '1985-08-22'),
       ('bob_wilson', 'bob@example.com', 'Bob Wilson', 'inactive', '{}', NULL, ARRAY['standard'], 0.00, FALSE, FALSE,
        '2000-12-01');

-- Вставка категорий
INSERT INTO categories (name, slug, description)
VALUES ('Electronics', 'electronics', 'Electronic devices and accessories'),
       ('Books', 'books', 'Books and e-books'),
       ('Clothing', 'clothing', 'Apparel and fashion');

INSERT INTO categories (name, slug, description, parent_id)
VALUES ('Laptops', 'laptops', 'Laptop computers', 1),
       ('Smartphones', 'smartphones', 'Mobile phones', 1);

-- Вставка заказов
INSERT INTO orders (user_id, order_number, status, subtotal, tax, shipping_cost, total, shipping_address,
                    billing_address, payment_method)
VALUES (1, 'ORD-2024-001', 'delivered', 999.99, 80.00, 15.00, 1094.99,
        '{
          "street": "123 Main St",
          "city": "New York",
          "zip": "10001"
        }',
        '{
          "street": "123 Main St",
          "city": "New York",
          "zip": "10001"
        }',
        'credit_card'),
       (2, 'ORD-2024-002', 'processing', 299.99, 24.00, 10.00, 333.99,
        '{
          "street": "456 Oak Ave",
          "city": "Los Angeles",
          "zip": "90001"
        }',
        '{
          "street": "456 Oak Ave",
          "city": "Los Angeles",
          "zip": "90001"
        }',
        'paypal');

-- Вставка элементов заказа
INSERT INTO order_items (order_id, category_id, product_name, sku, quantity, unit_price, total_price, attributes)
VALUES (1, 4, 'MacBook Pro 14"', 'MBP-14-2024', 1, 999.99, 999.99, '{
  "color": "Space Gray",
  "ram": "16GB",
  "storage": "512GB"
}'),
       (2, 5, 'iPhone 15 Pro', 'IP15P-128', 1, 299.99, 299.99, '{
         "color": "Titanium",
         "storage": "128GB"
       }');