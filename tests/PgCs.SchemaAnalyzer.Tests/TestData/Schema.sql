-- ==================================================================
-- Пример схемы для тестирования SchemaAnalyzer
-- ==================================================================

-- ENUM типы
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended');
CREATE TYPE order_status AS ENUM ('pending', 'completed', 'cancelled');

-- Композитный тип
CREATE TYPE address AS (
    street VARCHAR(255),
    city VARCHAR(100),
    zip_code VARCHAR(20)
);

-- DOMAIN тип
CREATE DOMAIN email AS VARCHAR(255)
    CHECK (VALUE ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');

-- ==================================================================
-- Таблица пользователей
-- ==================================================================
CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email email NOT NULL,
    full_name VARCHAR(255),
    status user_status NOT NULL DEFAULT 'active',
    preferences JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    balance NUMERIC(12, 2) DEFAULT 0.00,
    
    CONSTRAINT check_balance CHECK (balance >= 0)
);

-- Комментарии
COMMENT ON TABLE users IS 'Таблица пользователей';
COMMENT ON COLUMN users.balance IS 'Баланс пользователя';

-- Индексы
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_status ON users(status) WHERE status = 'active';
CREATE INDEX idx_users_preferences ON users USING gin(preferences);

-- ==================================================================
-- Таблица заказов
-- ==================================================================
CREATE TABLE orders (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    status order_status NOT NULL DEFAULT 'pending',
    total_amount NUMERIC(10, 2) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_orders_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_orders_user_id ON users(id);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_created_at ON orders(created_at DESC);

-- ==================================================================
-- Представление активных пользователей
-- ==================================================================
CREATE VIEW active_users AS
SELECT id, username, email, created_at
FROM users
WHERE status = 'active';

-- Материализованное представление
CREATE MATERIALIZED VIEW user_statistics AS
SELECT 
    status,
    COUNT(*) as user_count,
    AVG(balance) as avg_balance
FROM users
GROUP BY status;

-- ==================================================================
-- Функции
-- ==================================================================
CREATE FUNCTION get_user_order_count(user_id_param BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    order_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO order_count
    FROM orders
    WHERE user_id = user_id_param;
    
    RETURN order_count;
END;
$$;

-- Триггерная функция
CREATE FUNCTION update_updated_at()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

-- ==================================================================
-- Триггеры
-- ==================================================================
CREATE TRIGGER trg_users_update
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION update_updated_at();
