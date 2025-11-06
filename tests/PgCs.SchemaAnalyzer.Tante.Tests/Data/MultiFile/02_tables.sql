-- Файл 2: Таблицы

-- Таблица пользователей
CREATE TABLE users
(
    id               BIGSERIAL PRIMARY KEY,
    username         VARCHAR(50)                 NOT NULL UNIQUE,
    email            email                       NOT NULL UNIQUE,
    password_hash    VARCHAR(255)                NOT NULL,
    full_name        VARCHAR(255),
    status           user_status                 NOT NULL DEFAULT 'active',
    preferences      JSONB                                DEFAULT '{}',
    metadata         JSONB,
    billing_address  address,
    contact          contact_info,
    phone_numbers    phone_number[],
    tags             TEXT[],
    roles            VARCHAR(50)[],
    created_at       TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at       TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at    TIMESTAMP(6) WITH TIME ZONE,
    deleted_at       TIMESTAMP(6) WITH TIME ZONE,
    balance          positive_numeric                     DEFAULT 0.00,
    loyalty_points   INTEGER                              DEFAULT 0,
    discount_percent percentage                           DEFAULT 0,
    is_verified      BOOLEAN                     NOT NULL DEFAULT FALSE,
    is_premium       BOOLEAN                     NOT NULL DEFAULT FALSE,
    is_deleted       BOOLEAN                     NOT NULL DEFAULT FALSE,
    avatar           BYTEA,
    external_id      UUID                        NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    date_of_birth    DATE,
    subscription_duration INTERVAL,
    active_period    TSTZRANGE
);

-- Таблица категорий
CREATE TABLE categories
(
    id               SERIAL PRIMARY KEY,
    name             VARCHAR(100)                NOT NULL,
    slug             VARCHAR(100)                NOT NULL UNIQUE,
    description      TEXT,
    parent_id        INTEGER,
    level            INTEGER                     NOT NULL DEFAULT 0,
    path             LTREE,
    search_vector    TSVECTOR,
    is_active        BOOLEAN                     NOT NULL DEFAULT TRUE,
    sort_order       INTEGER                              DEFAULT 0,
    created_at       TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at       TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_categories_parent FOREIGN KEY (parent_id) REFERENCES categories (id)
        ON DELETE CASCADE
);

-- Таблица заказов
CREATE TABLE orders
(
    id                 BIGSERIAL PRIMARY KEY,
    user_id            BIGINT                      NOT NULL,
    order_number       VARCHAR(50)                 NOT NULL UNIQUE,
    status             order_status                NOT NULL DEFAULT 'pending',
    subtotal           NUMERIC(15, 2)              NOT NULL,
    tax                NUMERIC(15, 2)              NOT NULL DEFAULT 0,
    shipping_cost      NUMERIC(15, 2)              NOT NULL DEFAULT 0,
    discount           NUMERIC(15, 2)              NOT NULL DEFAULT 0,
    total              NUMERIC(15, 2)              NOT NULL,
    currency           VARCHAR(3)                  NOT NULL DEFAULT 'USD',
    payment_method     VARCHAR(50),
    shipping_address   address,
    billing_address    address,
    notes              TEXT,
    metadata           JSONB,
    created_at         TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at         TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paid_at            TIMESTAMP(6) WITH TIME ZONE,
    shipped_at         TIMESTAMP(6) WITH TIME ZONE,
    delivered_at       TIMESTAMP(6) WITH TIME ZONE,
    cancelled_at       TIMESTAMP(6) WITH TIME ZONE,
    expected_delivery  DATERANGE,
    delivery_window    TSTZRANGE,
    
    CONSTRAINT fk_orders_user FOREIGN KEY (user_id) REFERENCES users (id)
        ON DELETE RESTRICT
);

-- Таблица элементов заказа
CREATE TABLE order_items
(
    id               BIGSERIAL PRIMARY KEY,
    order_id         BIGINT                      NOT NULL,
    category_id      INTEGER,
    product_name     VARCHAR(255)                NOT NULL,
    product_sku      VARCHAR(100),
    quantity         INTEGER                     NOT NULL CHECK (quantity > 0),
    unit_price       NUMERIC(15, 2)              NOT NULL CHECK (unit_price >= 0),
    total_price      NUMERIC(15, 2)              NOT NULL CHECK (total_price >= 0),
    discount         NUMERIC(15, 2)                       DEFAULT 0,
    tax              NUMERIC(15, 2)                       DEFAULT 0,
    metadata         JSONB,
    created_at       TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at       TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_order_items_order FOREIGN KEY (order_id) REFERENCES orders (id)
        ON DELETE CASCADE,
    CONSTRAINT fk_order_items_category FOREIGN KEY (category_id) REFERENCES categories (id)
        ON DELETE SET NULL
);

-- Комментарии на таблицы
COMMENT ON TABLE users IS 'Основная таблица пользователей системы';
COMMENT ON TABLE categories IS 'Иерархия категорий товаров';
COMMENT ON TABLE orders IS 'Заказы пользователей';
COMMENT ON TABLE order_items IS 'Элементы заказов';

-- Комментарии на колонки users
COMMENT ON COLUMN users.id IS 'Уникальный идентификатор пользователя';
COMMENT ON COLUMN users.username IS 'Логин пользователя';
COMMENT ON COLUMN users.email IS 'Email адрес';
COMMENT ON COLUMN users.status IS 'Текущий статус аккаунта';

-- Комментарии на колонки categories
COMMENT ON COLUMN categories.id IS 'Идентификатор категории';
COMMENT ON COLUMN categories.name IS 'Название категории';
COMMENT ON COLUMN categories.slug IS 'URL-идентификатор';
COMMENT ON COLUMN categories.parent_id IS 'Родительская категория';

-- Комментарии на колонки orders
COMMENT ON COLUMN orders.id IS 'Идентификатор заказа';
COMMENT ON COLUMN orders.user_id IS 'Пользователь, сделавший заказ';
COMMENT ON COLUMN orders.order_number IS 'Номер заказа';
COMMENT ON COLUMN orders.status IS 'Текущий статус заказа';

