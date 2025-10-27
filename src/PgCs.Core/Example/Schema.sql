-- Пример схемы PostgreSQL 18 для демонстрации возможностей PgCs
-- Этот файл содержит примеры всех основных типов объектов PostgreSQL

-- Создание ENUM типов
-- ENUM для статуса пользователя
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');
COMMENT ON TYPE user_status IS 'Возможные статусы пользователя в системе';

-- ENUM для статуса заказа
CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');
COMMENT ON TYPE order_status IS 'Статусы жизненного цикла заказа';

-- ENUM для способов оплаты
CREATE TYPE payment_method AS ENUM ('credit_card', 'debit_card', 'paypal', 'bank_transfer', 'crypto', 'cash');
COMMENT ON TYPE payment_method IS 'Поддерживаемые методы оплаты';

-- ENUM для уровня приоритета
CREATE TYPE priority_level AS ENUM ('low', 'medium', 'high', 'urgent');
COMMENT ON TYPE priority_level IS 'Уровни приоритета для задач и уведомлений';

-- Создание композитного типа для адреса
CREATE TYPE address AS (
    street VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(50),
    zip_code VARCHAR(20),
    country VARCHAR(50)
    );
COMMENT ON TYPE address IS 'Структурированный тип данных для хранения адреса';

-- Создание композитного типа для контактной информации
CREATE TYPE contact_info AS (
    phone VARCHAR(20),
    email VARCHAR(255),
    telegram VARCHAR(50),
    preferred_method VARCHAR(20)
    );
COMMENT ON TYPE contact_info IS 'Контактная информация с указанием предпочитаемого способа связи';

-- Создание DOMAIN типов с ограничениями
-- Email с валидацией
CREATE DOMAIN email AS VARCHAR(255)
    CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');
COMMENT ON DOMAIN email IS 'Email адрес с валидацией формата';

-- Положительное число
CREATE DOMAIN positive_numeric AS NUMERIC(12, 2)
    CHECK (VALUE >= 0);
COMMENT ON DOMAIN positive_numeric IS 'Числовое значение больше или равно нулю';

-- Процент (0-100)
CREATE DOMAIN percentage AS NUMERIC(5, 2)
    CHECK (VALUE >= 0 AND VALUE <= 100);
COMMENT ON DOMAIN percentage IS 'Процентное значение от 0 до 100';

-- Телефонный номер
CREATE DOMAIN phone_number AS VARCHAR(20)
    CHECK (VALUE ~ '^\+?[1-9]\d{1,14}$');
COMMENT ON DOMAIN phone_number IS 'Телефонный номер в международном формате';

-- Таблица пользователей
CREATE TABLE users (
    -- Первичный ключ
                       id BIGSERIAL PRIMARY KEY,

                        -- Имя пользователя
                       username VARCHAR(50) NOT NULL UNIQUE,
                       -- Электронная почта
                       email email NOT NULL UNIQUE,
                       --- Хеш пароля
                       password_hash VARCHAR(255) NOT NULL,
                       full_name VARCHAR(255),
                       status user_status NOT NULL DEFAULT 'active',

    -- JSON данные для гибкости
                       preferences JSONB DEFAULT '{}',
                       metadata JSONB,

    -- Композитный тип
                       billing_address address,
                       contact contact_info,

    -- Массивы
                       phone_numbers phone_number[],
                       tags TEXT[],
                       roles VARCHAR(50)[],

    -- Временные метки
                       created_at TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                       updated_at TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                       last_login_at TIMESTAMP(6) WITH TIME ZONE,
                       deleted_at TIMESTAMP(6) WITH TIME ZONE,

    -- Числовые типы
                       balance positive_numeric DEFAULT 0.00,
                       loyalty_points INTEGER DEFAULT 0,
                       discount_percent percentage DEFAULT 0,

    -- Булевы флаги
                       is_verified BOOLEAN NOT NULL DEFAULT FALSE,
                       is_premium BOOLEAN NOT NULL DEFAULT FALSE,
                       is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    -- Бинарные данные
                       avatar BYTEA,

    -- UUID для внешней интеграции
                       external_id UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,

    -- Дата рождения
                       date_of_birth DATE,

    -- Интервал подписки
                       subscription_duration INTERVAL,

    -- Диапазон дат активности
                       active_period TSTZRANGE,

    -- Ограничения
                       CONSTRAINT check_balance CHECK (balance >= 0),
                       CONSTRAINT check_loyalty_points CHECK (loyalty_points >= 0),
                       CONSTRAINT check_age CHECK (
                           date_of_birth IS NULL OR
                           date_of_birth < CURRENT_DATE - INTERVAL '13 years'
),
    CONSTRAINT check_deleted_at CHECK (
        (is_deleted = FALSE AND deleted_at IS NULL) OR
        (is_deleted = TRUE AND deleted_at IS NOT NULL)
    )
);

-- Комментарии к таблице и колонкам
COMMENT ON TABLE users IS 'Основная таблица пользователей системы';
COMMENT ON COLUMN users.id IS 'Уникальный идентификатор пользователя';
COMMENT ON COLUMN users.username IS 'Уникальное имя пользователя для входа';
COMMENT ON COLUMN users.email IS 'Email адрес пользователя с валидацией';
COMMENT ON COLUMN users.password_hash IS 'Хеш пароля (bcrypt/argon2)';
COMMENT ON COLUMN users.full_name IS 'Полное имя пользователя';
COMMENT ON COLUMN users.status IS 'Текущий статус аккаунта';
COMMENT ON COLUMN users.preferences IS 'Пользовательские настройки в формате JSON';
COMMENT ON COLUMN users.metadata IS 'Дополнительные метаданные';
COMMENT ON COLUMN users.billing_address IS 'Адрес для выставления счетов';
COMMENT ON COLUMN users.contact IS 'Контактная информация пользователя';
COMMENT ON COLUMN users.phone_numbers IS 'Массив телефонных номеров';
COMMENT ON COLUMN users.tags IS 'Теги для категоризации пользователей';
COMMENT ON COLUMN users.roles IS 'Роли пользователя в системе';
COMMENT ON COLUMN users.created_at IS 'Дата и время регистрации';
COMMENT ON COLUMN users.updated_at IS 'Дата и время последнего обновления';
COMMENT ON COLUMN users.last_login_at IS 'Дата и время последнего входа';
COMMENT ON COLUMN users.deleted_at IS 'Дата и время мягкого удаления';
COMMENT ON COLUMN users.balance IS 'Баланс пользователя в основной валюте';
COMMENT ON COLUMN users.loyalty_points IS 'Накопленные баллы лояльности';
COMMENT ON COLUMN users.discount_percent IS 'Персональная скидка пользователя';
COMMENT ON COLUMN users.is_verified IS 'Подтвержден ли email адрес';
COMMENT ON COLUMN users.is_premium IS 'Является ли пользователь премиум';
COMMENT ON COLUMN users.is_deleted IS 'Флаг мягкого удаления';
COMMENT ON COLUMN users.avatar IS 'Аватар пользователя в бинарном формате';
COMMENT ON COLUMN users.external_id IS 'UUID для интеграции с внешними системами';
COMMENT ON COLUMN users.date_of_birth IS 'Дата рождения';
COMMENT ON COLUMN users.subscription_duration IS 'Продолжительность подписки';
COMMENT ON COLUMN users.active_period IS 'Период активности аккаунта';

-- Индексы для таблицы users
CREATE INDEX idx_users_email ON users (email) WHERE is_deleted = FALSE;
COMMENT ON INDEX idx_users_email IS 'Индекс для быстрого поиска по email активных пользователей';

CREATE INDEX idx_users_status ON users (status) WHERE is_deleted = FALSE;
COMMENT ON INDEX idx_users_status IS 'Индекс для фильтрации по статусу';

CREATE INDEX idx_users_created_at ON users (created_at DESC);
COMMENT ON INDEX idx_users_created_at IS 'Индекс для сортировки по дате создания';

CREATE INDEX idx_users_preferences ON users USING GIN (preferences);
COMMENT ON INDEX idx_users_preferences IS 'GIN индекс для поиска по JSON настройкам';

CREATE INDEX idx_users_tags ON users USING GIN (tags);
COMMENT ON INDEX idx_users_tags IS 'GIN индекс для поиска по тегам';

CREATE INDEX idx_users_external_id ON users (external_id) WHERE is_deleted = FALSE;
COMMENT ON INDEX idx_users_external_id IS 'Индекс для поиска по внешнему идентификатору';

CREATE INDEX idx_users_premium ON users (id) WHERE is_premium = TRUE AND is_deleted = FALSE;
COMMENT ON INDEX idx_users_premium IS 'Частичный индекс для премиум пользователей';

CREATE INDEX idx_users_deleted_at ON users (deleted_at) WHERE is_deleted = TRUE;
COMMENT ON INDEX idx_users_deleted_at IS 'Индекс для удаленных пользователей';

-- Таблица категорий товаров с иерархией
CREATE TABLE categories (
    -- Первичный ключ
                            id SERIAL PRIMARY KEY,

    -- Основная информация
                            name VARCHAR(100) NOT NULL,
                            slug VARCHAR(100) NOT NULL UNIQUE,
                            description TEXT,

    -- Иерархия
                            parent_id INTEGER,
                            level INTEGER NOT NULL DEFAULT 0,
                            path INTEGER[] NOT NULL DEFAULT ARRAY[]::INTEGER[],

    -- Полнотекстовый поиск
                            search_vector TSVECTOR,

    -- Метаданные
                            metadata JSONB DEFAULT '{}',

    -- Сортировка и отображение
                            sort_order INTEGER NOT NULL DEFAULT 0,
                            is_active BOOLEAN NOT NULL DEFAULT TRUE,

    -- Временные метки
                            created_at TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Ограничения
                            CONSTRAINT fk_parent_category FOREIGN KEY (parent_id)
                                REFERENCES categories (id)
                                ON DELETE CASCADE
                                ON UPDATE CASCADE,
                            CONSTRAINT check_level CHECK (level >= 0),
                            CONSTRAINT check_sort_order CHECK (sort_order >= 0),
                            CONSTRAINT check_no_self_reference CHECK (id != parent_id)
    );

-- Комментарии к таблице categories
COMMENT ON TABLE categories IS 'Иерархическая структура категорий товаров';
COMMENT ON COLUMN categories.id IS 'Уникальный идентификатор категории';
COMMENT ON COLUMN categories.name IS 'Название категории';
COMMENT ON COLUMN categories.slug IS 'URL-friendly идентификатор';
COMMENT ON COLUMN categories.description IS 'Описание категории';
COMMENT ON COLUMN categories.parent_id IS 'Ссылка на родительскую категорию';
COMMENT ON COLUMN categories.level IS 'Уровень вложенности в иерархии';
COMMENT ON COLUMN categories.path IS 'Массив ID предков для быстрого обхода дерева';
COMMENT ON COLUMN categories.search_vector IS 'Вектор для полнотекстового поиска';
COMMENT ON COLUMN categories.metadata IS 'Дополнительные данные категории';
COMMENT ON COLUMN categories.sort_order IS 'Порядок сортировки';
COMMENT ON COLUMN categories.is_active IS 'Активна ли категория';
COMMENT ON COLUMN categories.created_at IS 'Дата создания';
COMMENT ON COLUMN categories.updated_at IS 'Дата последнего обновления';

-- Индексы для categories
CREATE INDEX idx_categories_parent_id ON categories (parent_id) WHERE is_active = TRUE;
COMMENT ON INDEX idx_categories_parent_id IS 'Индекс для поиска дочерних категорий';

CREATE INDEX idx_categories_path ON categories USING GIN (path);
COMMENT ON INDEX idx_categories_path IS 'GIN индекс для поиска по пути в иерархии';

CREATE INDEX idx_categories_search ON categories USING GIN (search_vector);
COMMENT ON INDEX idx_categories_search IS 'GIN индекс для полнотекстового поиска';

CREATE INDEX idx_categories_slug ON categories (slug) WHERE is_active = TRUE;
COMMENT ON INDEX idx_categories_slug IS 'Индекс для поиска по slug';

CREATE INDEX idx_categories_sort_order ON categories (parent_id, sort_order) WHERE is_active = TRUE;
COMMENT ON INDEX idx_categories_sort_order IS 'Индекс для сортировки категорий';

-- Функция для автоматического обновления search_vector
CREATE OR REPLACE FUNCTION update_category_search_vector()
RETURNS TRIGGER AS $$
BEGIN
    NEW.search_vector := 
        setweight(to_tsvector('english', COALESCE(NEW.name, '')), 'A') ||
        setweight(to_tsvector('english', COALESCE(NEW.description, '')), 'B');
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION update_category_search_vector() IS 'Автоматически обновляет поисковый вектор при изменении категории';

-- Триггер для обновления search_vector
CREATE TRIGGER trigger_update_category_search
    BEFORE INSERT OR UPDATE OF name, description
                     ON categories
                         FOR EACH ROW
                         EXECUTE FUNCTION update_category_search_vector();

COMMENT ON TRIGGER trigger_update_category_search ON categories IS 'Триггер для автоматического обновления поискового вектора';

-- Функция для обновления пути категории
CREATE OR REPLACE FUNCTION update_category_path()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.parent_id IS NULL THEN
        NEW.path := ARRAY[NEW.id];
        NEW.level := 0;
ELSE
SELECT path || NEW.id, level + 1
INTO NEW.path, NEW.level
FROM categories
WHERE id = NEW.parent_id;
END IF;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION update_category_path() IS 'Автоматически обновляет путь и уровень категории в иерархии';

-- Триггер для обновления пути
CREATE TRIGGER trigger_update_category_path
    BEFORE INSERT OR UPDATE OF parent_id
                     ON categories
                         FOR EACH ROW
                         EXECUTE FUNCTION update_category_path();

COMMENT ON TRIGGER trigger_update_category_path ON categories IS 'Триггер для автоматического обновления пути категории';

-- Таблица заказов
CREATE TABLE orders (
    -- Первичный ключ
                        id BIGSERIAL PRIMARY KEY,

    -- Связь с пользователем
                        user_id BIGINT NOT NULL,

    -- Уникальный номер заказа
                        order_number VARCHAR(50) NOT NULL UNIQUE,

    -- Статус заказа
                        status order_status NOT NULL DEFAULT 'pending',
                        priority priority_level NOT NULL DEFAULT 'medium',

    -- Денежные значения
                        subtotal positive_numeric NOT NULL,
                        tax positive_numeric NOT NULL DEFAULT 0,
                        shipping_cost positive_numeric NOT NULL DEFAULT 0,
                        discount positive_numeric NOT NULL DEFAULT 0,
                        total positive_numeric NOT NULL,

    -- Адреса в формате JSONB
                        shipping_address JSONB NOT NULL,
                        billing_address JSONB NOT NULL,

    -- Оплата
                        payment_method payment_method NOT NULL,
                        payment_details JSONB,
                        payment_status VARCHAR(20) NOT NULL DEFAULT 'pending',

    -- История изменений
                        status_history JSONB NOT NULL DEFAULT '[]',

    -- Временные метки
                        created_at TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        confirmed_at TIMESTAMP(6) WITH TIME ZONE,
                        shipped_at TIMESTAMP(6) WITH TIME ZONE,
                        delivered_at TIMESTAMP(6) WITH TIME ZONE,
                        cancelled_at TIMESTAMP(6) WITH TIME ZONE,

    -- Окно доставки
                        delivery_window TSTZRANGE,

    -- Дополнительная информация
                        notes TEXT,
                        customer_notes TEXT,
                        internal_notes TEXT,

    -- Внешний ID для интеграции
                        external_id UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,

    -- Ограничения
                        CONSTRAINT fk_orders_user FOREIGN KEY (user_id)
                            REFERENCES users (id)
                            ON DELETE RESTRICT
                            ON UPDATE CASCADE,
                        CONSTRAINT check_total CHECK (total >= 0),
                        CONSTRAINT check_dates CHECK (
                            (shipped_at IS NULL OR shipped_at >= created_at) AND
                            (delivered_at IS NULL OR delivered_at >= COALESCE(shipped_at, created_at)) AND
                            (cancelled_at IS NULL OR confirmed_at IS NULL OR cancelled_at >= confirmed_at)
                            ),
                        CONSTRAINT check_subtotal CHECK (subtotal > 0)
);

-- Комментарии к таблице orders
COMMENT ON TABLE orders IS 'Заказы пользователей с полной историей статусов';
COMMENT ON COLUMN orders.id IS 'Уникальный идентификатор заказа';
COMMENT ON COLUMN orders.user_id IS 'ID пользователя, создавшего заказ';
COMMENT ON COLUMN orders.order_number IS 'Уникальный номер заказа для отображения клиенту';
COMMENT ON COLUMN orders.status IS 'Текущий статус заказа';
COMMENT ON COLUMN orders.priority IS 'Приоритет обработки заказа';
COMMENT ON COLUMN orders.subtotal IS 'Сумма товаров без налогов и доставки';
COMMENT ON COLUMN orders.tax IS 'Сумма налога';
COMMENT ON COLUMN orders.shipping_cost IS 'Стоимость доставки';
COMMENT ON COLUMN orders.discount IS 'Сумма скидки';
COMMENT ON COLUMN orders.total IS 'Итоговая сумма заказа';
COMMENT ON COLUMN orders.shipping_address IS 'Адрес доставки в формате JSON';
COMMENT ON COLUMN orders.billing_address IS 'Адрес для выставления счета';
COMMENT ON COLUMN orders.payment_method IS 'Способ оплаты';
COMMENT ON COLUMN orders.payment_details IS 'Детали оплаты (зашифрованные)';
COMMENT ON COLUMN orders.payment_status IS 'Статус оплаты';
COMMENT ON COLUMN orders.status_history IS 'История изменений статуса [{status, timestamp, user_id, comment}]';
COMMENT ON COLUMN orders.created_at IS 'Дата создания заказа';
COMMENT ON COLUMN orders.updated_at IS 'Дата последнего обновления';
COMMENT ON COLUMN orders.confirmed_at IS 'Дата подтверждения заказа';
COMMENT ON COLUMN orders.shipped_at IS 'Дата отправки';
COMMENT ON COLUMN orders.delivered_at IS 'Дата доставки';
COMMENT ON COLUMN orders.cancelled_at IS 'Дата отмены';
COMMENT ON COLUMN orders.delivery_window IS 'Окно времени для доставки';
COMMENT ON COLUMN orders.notes IS 'Общие заметки к заказу';
COMMENT ON COLUMN orders.customer_notes IS 'Заметки клиента';
COMMENT ON COLUMN orders.internal_notes IS 'Внутренние заметки (не видны клиенту)';
COMMENT ON COLUMN orders.external_id IS 'Внешний идентификатор для интеграции';

-- Индексы для orders
CREATE INDEX idx_orders_user_id ON orders (user_id, created_at DESC);
COMMENT ON INDEX idx_orders_user_id IS 'Индекс для поиска заказов пользователя';

CREATE INDEX idx_orders_status ON orders (status, created_at DESC);
COMMENT ON INDEX idx_orders_status IS 'Индекс для фильтрации по статусу';

CREATE INDEX idx_orders_created_at ON orders (created_at DESC);
COMMENT ON INDEX idx_orders_created_at IS 'Индекс для сортировки по дате';

CREATE INDEX idx_orders_order_number ON orders (order_number);
COMMENT ON INDEX idx_orders_order_number IS 'Уникальный индекс по номеру заказа';

CREATE INDEX idx_orders_shipping_address ON orders USING GIN (shipping_address);
COMMENT ON INDEX idx_orders_shipping_address IS 'GIN индекс для поиска по адресу доставки';

CREATE INDEX idx_orders_user_status ON orders (user_id, status) WHERE cancelled_at IS NULL;
COMMENT ON INDEX idx_orders_user_status IS 'Составной индекс для активных заказов пользователя';

CREATE INDEX idx_orders_delivery_window ON orders USING GIST (delivery_window);
COMMENT ON INDEX idx_orders_delivery_window IS 'GIST индекс для поиска по окну доставки';

CREATE INDEX idx_orders_payment_status ON orders (payment_status) WHERE payment_status != 'paid';
COMMENT ON INDEX idx_orders_payment_status IS 'Частичный индекс для неоплаченных заказов';

-- Таблица элементов заказа
CREATE TABLE order_items (
    -- Первичный ключ
                             id BIGSERIAL PRIMARY KEY,

    -- Связи
                             order_id BIGINT NOT NULL,
                             category_id INTEGER NOT NULL,

    -- Информация о продукте
                             product_name VARCHAR(255) NOT NULL,
                             product_description TEXT,
                             sku VARCHAR(100),
                             barcode VARCHAR(50),

    -- Количество и цены
                             quantity INTEGER NOT NULL,
                             unit_price positive_numeric NOT NULL,
                             discount_percent percentage DEFAULT 0,
                             discount_amount positive_numeric DEFAULT 0,
                             tax_percent percentage DEFAULT 0,
                             tax_amount positive_numeric DEFAULT 0,
                             total_price positive_numeric NOT NULL,

    -- Атрибуты продукта
                             attributes JSONB DEFAULT '{}',

    -- Вес и размеры
                             weight_grams INTEGER,
                             dimensions JSONB,

    -- Временные метки
                             created_at TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Ограничения
                             CONSTRAINT fk_order_items_order FOREIGN KEY (order_id)
                                 REFERENCES orders (id)
                                 ON DELETE CASCADE
                                 ON UPDATE CASCADE,
                             CONSTRAINT fk_order_items_category FOREIGN KEY (category_id)
                                 REFERENCES categories (id)
                                 ON DELETE RESTRICT
                                 ON UPDATE CASCADE,
                             CONSTRAINT check_quantity CHECK (quantity > 0),
                             CONSTRAINT check_unit_price CHECK (unit_price >= 0),
                             CONSTRAINT check_total_price CHECK (total_price >= 0)
);

-- Комментарии к таблице order_items
COMMENT ON TABLE order_items IS 'Позиции (товары) в заказах';
COMMENT ON COLUMN order_items.id IS 'Уникальный идентификатор позиции';
COMMENT ON COLUMN order_items.order_id IS 'ID заказа';
COMMENT ON COLUMN order_items.category_id IS 'ID категории товара';
COMMENT ON COLUMN order_items.product_name IS 'Название товара';
COMMENT ON COLUMN order_items.product_description IS 'Описание товара';
COMMENT ON COLUMN order_items.sku IS 'Артикул товара';
COMMENT ON COLUMN order_items.barcode IS 'Штрих-код товара';
COMMENT ON COLUMN order_items.quantity IS 'Количество единиц товара';
COMMENT ON COLUMN order_items.unit_price IS 'Цена за единицу';
COMMENT ON COLUMN order_items.discount_percent IS 'Процент скидки';
COMMENT ON COLUMN order_items.discount_amount IS 'Сумма скидки';
COMMENT ON COLUMN order_items.tax_percent IS 'Процент налога';
COMMENT ON COLUMN order_items.tax_amount IS 'Сумма налога';
COMMENT ON COLUMN order_items.total_price IS 'Итоговая стоимость позиции';
COMMENT ON COLUMN order_items.attributes IS 'Дополнительные атрибуты (цвет, размер и т.д.)';
COMMENT ON COLUMN order_items.weight_grams IS 'Вес товара в граммах';
COMMENT ON COLUMN order_items.dimensions IS 'Габариты товара {length, width, height}';
COMMENT ON COLUMN order_items.created_at IS 'Дата добавления в заказ';

-- Индексы для order_items
CREATE INDEX idx_order_items_order_id ON order_items (order_id);
COMMENT ON INDEX idx_order_items_order_id IS 'Индекс для поиска позиций заказа';

CREATE INDEX idx_order_items_category_id ON order_items (category_id);
COMMENT ON INDEX idx_order_items_category_id IS 'Индекс для поиска по категории';

CREATE INDEX idx_order_items_sku ON order_items (sku) WHERE sku IS NOT NULL;
COMMENT ON INDEX idx_order_items_sku IS 'Индекс для поиска по артикулу';

CREATE INDEX idx_order_items_attributes ON order_items USING GIN (attributes);
COMMENT ON INDEX idx_order_items_attributes IS 'GIN индекс для поиска по атрибутам';

-- VIEW: Активные пользователи с заказами
CREATE VIEW active_users_with_orders AS
SELECT
    u.id,
    u.username,
    u.email,
    u.full_name,
    u.status,
    u.is_premium,
    u.balance,
    u.loyalty_points,
    COUNT(DISTINCT o.id) AS total_orders,
    COALESCE(SUM(o.total), 0) AS total_spent,
    COALESCE(AVG(o.total), 0) AS average_order_value,
    MAX(o.created_at) AS last_order_date,
    ARRAY_AGG(DISTINCT o.status ORDER BY o.status) FILTER (WHERE o.status IS NOT NULL) AS order_statuses,
    COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'pending') AS pending_orders,
    COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'delivered') AS completed_orders
FROM users u
         LEFT JOIN orders o ON u.id = o.user_id AND o.cancelled_at IS NULL
WHERE u.status = 'active' AND u.is_deleted = FALSE
GROUP BY u.id, u.username, u.email, u.full_name, u.status, u.is_premium, u.balance, u.loyalty_points;

COMMENT ON VIEW active_users_with_orders IS 'Представление активных пользователей с агрегированной статистикой по заказам';

-- Материализованное представление: Статистика по категориям
CREATE MATERIALIZED VIEW category_statistics AS
SELECT
    c.id,
    c.name,
    c.slug,
    c.level,
    c.parent_id,
    COUNT(oi.id) AS total_items_sold,
    COALESCE(SUM(oi.quantity), 0) AS total_quantity,
    COALESCE(SUM(oi.total_price), 0) AS total_revenue,
    COALESCE(AVG(oi.unit_price), 0) AS avg_price,
    COALESCE(MIN(oi.unit_price), 0) AS min_price,
    COALESCE(MAX(oi.unit_price), 0) AS max_price,
    COUNT(DISTINCT oi.order_id) AS unique_orders,
    COUNT(DISTINCT o.user_id) AS unique_customers,
    MAX(o.created_at) AS last_order_date
FROM categories c
         LEFT JOIN order_items oi ON c.id = oi.category_id
         LEFT JOIN orders o ON oi.order_id = o.id AND o.cancelled_at IS NULL
WHERE c.is_active = TRUE
GROUP BY c.id, c.name, c.slug, c.level, c.parent_id;

-- Уникальный индекс для materialized view
CREATE UNIQUE INDEX idx_category_statistics_id ON category_statistics (id);
COMMENT ON INDEX idx_category_statistics_id IS 'Уникальный индекс для материализованного представления';

CREATE INDEX idx_category_statistics_revenue ON category_statistics (total_revenue DESC);
COMMENT ON INDEX idx_category_statistics_revenue ON category_statistics IS 'Индекс для сортировки по выручке';

COMMENT ON MATERIALIZED VIEW category_statistics IS 'Статистика продаж по категориям (требует периодического REFRESH MATERIALIZED VIEW)';

-- Функция для получения полного пути категории
CREATE OR REPLACE FUNCTION get_category_path(category_id INTEGER)
RETURNS TEXT AS $$
DECLARE
result TEXT := '';
    current_id INTEGER := category_id;
    current_name VARCHAR(100);
    parent INTEGER;
BEGIN
    LOOP
SELECT name, parent_id
INTO current_name, parent
FROM categories
WHERE id = current_id;

EXIT WHEN current_name IS NULL;
        
        result := current_name || 
            CASE WHEN result = '' THEN '' ELSE ' > ' || result END;
        
        EXIT WHEN parent IS NULL;
        current_id := parent;
END LOOP;

RETURN result;
END;
$$ LANGUAGE plpgsql STABLE;

COMMENT ON FUNCTION get_category_path(INTEGER) IS 'Возвращает полный путь категории от корня в формате "Parent > Child > Grandchild"';

-- Функция для получения всех дочерних категорий
CREATE OR REPLACE FUNCTION get_child_categories(parent_category_id INTEGER)
RETURNS TABLE(id INTEGER, name VARCHAR, level INTEGER) AS $$
BEGIN
RETURN QUERY
    WITH RECURSIVE subcategories AS (
        SELECT c.id, c.name, c.level, c.parent_id
        FROM categories c
        WHERE c.parent_id = parent_category_id
        
        UNION ALL
        
        SELECT c.id, c.name, c.level, c.parent_id
        FROM categories c
        INNER JOIN subcategories s ON c.parent_id = s.id
    )
SELECT sc.id, sc.name, sc.level
FROM subcategories sc
ORDER BY sc.level, sc.name;
END;
$$ LANGUAGE plpgsql STABLE;

COMMENT ON FUNCTION get_child_categories(INTEGER) IS 'Возвращает все дочерние категории рекурсивно';

-- Функция для обновления updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION update_updated_at_column() IS 'Автоматически обновляет колонку updated_at при изменении записи';

-- Триггеры для автоматического обновления updated_at
CREATE TRIGGER trigger_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

COMMENT ON TRIGGER trigger_users_updated_at ON users IS 'Автоматически обновляет updated_at при изменении пользователя';

CREATE TRIGGER trigger_categories_updated_at
    BEFORE UPDATE ON categories
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

COMMENT ON TRIGGER trigger_categories_updated_at ON categories IS 'Автоматически обновляет updated_at при изменении категории';

CREATE TRIGGER trigger_orders_updated_at
    BEFORE UPDATE ON orders
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

COMMENT ON TRIGGER trigger_orders_updated_at ON orders IS 'Автоматически обновляет updated_at при изменении заказа';

-- Функция для добавления записи в историю статусов заказа
CREATE OR REPLACE FUNCTION add_order_status_history()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.status IS DISTINCT FROM OLD.status THEN
        NEW.status_history := OLD.status_history || jsonb_build_object(
            'status', NEW.status,
            'previous_status', OLD.status,
            'timestamp', CURRENT_TIMESTAMP,
            'changed_by', current_user
        );
END IF;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION add_order_status_history() IS 'Автоматически добавляет запись в историю при изменении статуса заказа';

CREATE TRIGGER trigger_order_status_history
    BEFORE UPDATE OF status ON orders
    FOR EACH ROW
    WHEN (OLD.status IS DISTINCT FROM NEW.status)
    EXECUTE FUNCTION add_order_status_history();

COMMENT ON TRIGGER trigger_order_status_history ON orders IS 'Триггер для автоматического ведения истории изменений статуса';

-- Партиционированная таблица для логов
CREATE TABLE audit_logs (
                            id BIGSERIAL,
                            user_id BIGINT,
                            action VARCHAR(50) NOT NULL,
                            entity_type VARCHAR(50) NOT NULL,
                            entity_id BIGINT,
                            old_values JSONB,
                            new_values JSONB,
                            ip_address INET,
                            user_agent TEXT,
                            created_at TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            PRIMARY KEY (id, created_at)
) PARTITION BY RANGE (created_at);

COMMENT ON TABLE audit_logs IS 'Партиционированная таблица для аудита всех действий в системе';
COMMENT ON COLUMN audit_logs.id IS 'Уникальный идентификатор записи лога';
COMMENT ON COLUMN audit_logs.user_id IS 'ID пользователя, совершившего действие';
COMMENT ON COLUMN audit_logs.action IS 'Тип действия (CREATE, UPDATE, DELETE и т.д.)';
COMMENT ON COLUMN audit_logs.entity_type IS 'Тип сущности (users, orders, categories и т.д.)';
COMMENT ON COLUMN audit_logs.entity_id IS 'ID измененной сущности';
COMMENT ON COLUMN audit_logs.old_values IS 'Старые значения полей (для UPDATE и DELETE)';
COMMENT ON COLUMN audit_logs.new_values IS 'Новые значения полей (для CREATE и UPDATE)';
COMMENT ON COLUMN audit_logs.ip_address IS 'IP адрес пользователя';
COMMENT ON COLUMN audit_logs.user_agent IS 'User Agent браузера/приложения';
COMMENT ON COLUMN audit_logs.created_at IS 'Время совершения действия';

-- Создание партиций для audit_logs
CREATE TABLE audit_logs_2024_q1 PARTITION OF audit_logs
    FOR VALUES FROM ('2024-01-01') TO ('2024-04-01');
COMMENT ON TABLE audit_logs_2024_q1 IS 'Партиция логов за Q1 2024';

CREATE TABLE audit_logs_2024_q2 PARTITION OF audit_logs
    FOR VALUES FROM ('2024-04-01') TO ('2024-07-01');
COMMENT ON TABLE audit_logs_2024_q2 IS 'Партиция логов за Q2 2024';

CREATE TABLE audit_logs_2024_q3 PARTITION OF audit_logs
    FOR VALUES FROM ('2024-07-01') TO ('2024-10-01');
COMMENT ON TABLE audit_logs_2024_q3 IS 'Партиция логов за Q3 2024';

CREATE TABLE audit_logs_2024_q4 PARTITION OF audit_logs
    FOR VALUES FROM ('2024-10-01') TO ('2025-01-01');
COMMENT ON TABLE audit_logs_2024_q4 IS 'Партиция логов за Q4 2024';

-- Индексы на партиционированной таблице
CREATE INDEX idx_audit_logs_user_id ON audit_logs (user_id, created_at DESC);
COMMENT ON INDEX idx_audit_logs_user_id IS 'Индекс для поиска действий пользователя';

CREATE INDEX idx_audit_logs_entity ON audit_logs (entity_type, entity_id, created_at DESC);
COMMENT ON INDEX idx_audit_logs_entity IS 'Индекс для поиска изменений конкретной сущности';

CREATE INDEX idx_audit_logs_action ON audit_logs (action, created_at DESC);
COMMENT ON INDEX idx_audit_logs_action IS 'Индекс для фильтрации по типу действия';

-- Тестовые данные
INSERT INTO users (username, email, full_name, status, preferences, phone_numbers, tags, roles, balance, loyalty_points, is_verified, is_premium, date_of_birth)
VALUES
    ('john_doe', 'john@example.com', 'John Doe', 'active',
     '{"theme": "dark", "language": "en", "notifications": {"email": true, "push": true}}'::jsonb,
     ARRAY['+12025550123', '+12025550124']::phone_number[],
     ARRAY['vip', 'early_adopter'],
     ARRAY['customer', 'premium'],
     1500.50, 1200, TRUE, TRUE, '1990-05-15'),

    ('jane_smith', 'jane@example.com', 'Jane Smith', 'active',
     '{"theme": "light", "language": "en", "notifications": {"email": true, "push": false}}'::jsonb,
     ARRAY['+12025550125']::phone_number[],
     ARRAY['premium'],
     ARRAY['customer'],
     500.00, 300, TRUE, FALSE, '1985-08-22'),

    ('bob_wilson', 'bob@example.com', 'Bob Wilson', 'inactive',
     '{}'::jsonb,
     NULL,
     ARRAY['standard'],
     ARRAY['customer'],
     0.00, 0, FALSE, FALSE, '2000-12-01');

COMMENT ON CONSTRAINT users_pkey ON users IS 'Первичный ключ таблицы users';
COMMENT ON CONSTRAINT users_username_key ON users IS 'Уникальное ограничение на username';
COMMENT ON CONSTRAINT users_email_key ON users IS 'Уникальное ограничение на email';