-- Файл 4: Функции и триггеры

-- Функция обновления поискового вектора
CREATE OR REPLACE FUNCTION update_category_search_vector()
RETURNS TRIGGER AS $$
BEGIN
    NEW.search_vector := 
        setweight(to_tsvector('english', COALESCE(NEW.name, '')), 'A') ||
        setweight(to_tsvector('english', COALESCE(NEW.description, '')), 'B');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Функция обновления updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Функция получения пути категории
CREATE OR REPLACE FUNCTION get_category_path(category_id INTEGER)
RETURNS TEXT AS $$
DECLARE
    result TEXT := '';
    current_id INTEGER := category_id;
    current_name VARCHAR(100);
BEGIN
    WHILE current_id IS NOT NULL LOOP
        SELECT name, parent_id INTO current_name, current_id
        FROM categories
        WHERE id = current_id;
        
        IF current_name IS NOT NULL THEN
            result := current_name || CASE WHEN result = '' THEN '' ELSE ' > ' || result END;
        END IF;
    END LOOP;
    
    RETURN result;
END;
$$ LANGUAGE plpgsql;

-- Триггер для обновления поискового вектора категорий
CREATE TRIGGER trigger_update_category_search_vector
    BEFORE INSERT OR UPDATE ON categories
    FOR EACH ROW
    EXECUTE FUNCTION update_category_search_vector();

-- Триггер для автоматического обновления updated_at в users
CREATE TRIGGER trigger_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Триггер для автоматического обновления updated_at в categories
CREATE TRIGGER trigger_categories_updated_at
    BEFORE UPDATE ON categories
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Триггер для автоматического обновления updated_at в orders
CREATE TRIGGER trigger_orders_updated_at
    BEFORE UPDATE ON orders
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Комментарии на функции
COMMENT ON FUNCTION update_category_search_vector() IS 'Автоматически обновляет поисковый вектор для категории';
COMMENT ON FUNCTION update_updated_at_column() IS 'Обновляет timestamp updated_at при изменении записи';
COMMENT ON FUNCTION get_category_path(INTEGER) IS 'Возвращает полный путь категории';

-- Комментарии на триггеры
COMMENT ON TRIGGER trigger_update_category_search_vector ON categories IS 'Триггер для автоматического обновления поискового вектора';
COMMENT ON TRIGGER trigger_users_updated_at ON users IS 'Автообновление времени изменения';
COMMENT ON TRIGGER trigger_categories_updated_at ON categories IS 'Автообновление времени изменения';
COMMENT ON TRIGGER trigger_orders_updated_at ON orders IS 'Автообновление времени изменения';
-- Файл 1: Типы данных и домены

-- ENUM типы
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');

CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');

-- Composite типы
CREATE TYPE address AS
(
    street   VARCHAR(200),
    city     VARCHAR(100),
    state    VARCHAR(100),
    zip_code VARCHAR(20),
    country  VARCHAR(100)
);

CREATE TYPE contact_info AS
(
    phone            VARCHAR(50),
    email            VARCHAR(255),
    telegram         VARCHAR(100),
    preferred_method VARCHAR(20)
);

-- Домены
CREATE DOMAIN email AS VARCHAR(255)
    CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');

CREATE DOMAIN positive_numeric AS NUMERIC(15, 2)
    CHECK (VALUE > 0);

CREATE DOMAIN percentage AS NUMERIC(5, 2)
    CHECK (VALUE >= 0 AND VALUE <= 100);

CREATE DOMAIN phone_number AS VARCHAR(20)
    CHECK (VALUE ~ '^\+?[0-9\s\-()]+$');

-- Комментарии
COMMENT ON TYPE user_status IS 'Возможные статусы пользователя в системе';
COMMENT ON TYPE order_status IS 'Статусы заказов';
COMMENT ON TYPE address IS 'Структурированный адрес';
COMMENT ON TYPE contact_info IS 'Контактная информация';
COMMENT ON DOMAIN email IS 'Email адрес с валидацией';
COMMENT ON DOMAIN positive_numeric IS 'Положительное число';
COMMENT ON DOMAIN percentage IS 'Процентное значение от 0 до 100';
COMMENT ON DOMAIN phone_number IS 'Номер телефона';

