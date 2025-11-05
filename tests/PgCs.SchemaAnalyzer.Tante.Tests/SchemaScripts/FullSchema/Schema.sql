-- Статусы пользователя
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');
COMMENT
    ON TYPE user_status IS 'Возможные статусы пользователя в системе';

-- Статусы заказа
CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');
COMMENT
    ON TYPE order_status IS 'Статусы жизненного цикла заказа';

-- Способы оплаты
CREATE TYPE payment_method AS ENUM ('credit_card', 'debit_card', 'paypal', 'bank_transfer', 'crypto', 'cash');
COMMENT
    ON TYPE payment_method IS 'Поддерживаемые методы оплаты';

-- Уровни приоритета
CREATE TYPE priority_level AS ENUM ('low', 'medium', 'high', 'urgent');
COMMENT
    ON TYPE priority_level IS 'Уровни приоритета для задач и уведомлений';

-- Структурированный адрес
CREATE TYPE address AS
(
    street   VARCHAR(255), -- comment: Улица; to_type: VARCHAR(255); to_name: StreetAddress
    city     VARCHAR(100), -- comment: Город; to_type: VARCHAR(100); to_name: CityName
    state    VARCHAR(50),  -- comment: Штат/область; to_type: VARCHAR(50); to_name: StateName
    zip_code VARCHAR(20),  -- comment: Почтовый индекс; to_type: VARCHAR(20); to_name: PostalCode
    country  VARCHAR(50)   -- comment: Страна; to_type: VARCHAR(50); to_name: CountryName
);
COMMENT
    ON TYPE address IS 'Структурированный тип данных для хранения адреса';

-- Информация о контакте
CREATE TYPE contact_info AS
(
    phone            VARCHAR(20),  -- comment: Телефонный номер; to_type: VARCHAR(20); to_name: MainPhone
    email            VARCHAR(255), -- comment: Почта; to_type: VARCHAR(255); to_name: EmailAddress
    telegram         VARCHAR(50),  -- comment: Телеграмм; to_type: VARCHAR(50); to_name: TelegramHandle
    preferred_method VARCHAR(20)   -- comment: Предпочитаемый способ связи; to_type: VARCHAR(20); to_name: PreferredContactMethod
);
COMMENT
    ON TYPE contact_info IS 'Контактная информация с указанием предпочитаемого способа связи';

-- Домен для email
CREATE DOMAIN email AS VARCHAR(255)
    CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');
COMMENT
    ON DOMAIN email IS 'Email адрес с валидацией формата';

-- Домен для положительных чисел
CREATE DOMAIN positive_numeric AS NUMERIC(12, 2)
    CHECK (VALUE >= 0);
COMMENT
    ON DOMAIN positive_numeric IS 'Числовое значение больше или равно нулю';

-- Домен для процентов
CREATE DOMAIN percentage AS NUMERIC(5, 2)
    CHECK (VALUE >= 0 AND VALUE <= 100);
COMMENT
    ON DOMAIN percentage IS 'Процентное значение от 0 до 100';

-- Домен для телефонных номеров
CREATE DOMAIN phone_number AS VARCHAR(20)
    CHECK (VALUE ~ '^\+?[1-9]\d{1,14}$');
COMMENT
    ON DOMAIN phone_number IS 'Телефонный номер в международном формате';

-- Таблица пользователей
CREATE TABLE users
(
    id                    BIGSERIAL PRIMARY KEY,                                                 -- comment: Уникальный идентификатор; to_type: BIGSERIAL; to_name: UserId

    username              VARCHAR(50)                 NOT NULL UNIQUE,                           -- comment: Логин пользователя; to_type: VARCHAR(50); to_name: UserName
    email                 email                       NOT NULL UNIQUE,                           -- comment: Email адрес; to_type: email; to_name: EmailAddress
    password_hash         VARCHAR(255)                NOT NULL,                                  -- to_name: PasswordHash
    full_name             VARCHAR(255),                                                          -- comment: Полное имя; to_type: VARCHAR(255); to_name: FullName
    status                user_status                 NOT NULL DEFAULT 'active',                 -- to_name: AccountStatus; to_type: user_status;

    preferences           JSONB                                DEFAULT '{}',                     -- comment: Настройки пользователя; to_type: JSONB; to_name: UserPreferences
    metadata              JSONB,                                                                 -- comment: Дополнительные метаданные; to_type: JSONB; to_name: Metadata

    billing_address       address,                                                               -- comment: Адрес выставления счетов; to_type: address; to_name: BillingAddress
    contact               contact_info,                                                          -- comment: Контактная информация; to_type: contact_info; to_name: ContactInfo

    phone_numbers         phone_number[],                                                        -- comment: Массив телефонных номеров; to_type: phone_number[]; to_name: PhoneNumbers
    tags                  TEXT[],                                                                -- comment: Теги пользователя; to_type: TEXT[]; to_name: UserTags
    roles                 VARCHAR(50)[],                                                         -- comment: Роли пользователя; to_type: VARCHAR(50)[]; to_name: UserRoles

    created_at            TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,        -- comment: Дата создания; to_type: TIMESTAMP(6) WITH TIME ZONE; to_name: CreatedAt
    updated_at            TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,        -- comment: Дата обновления; to_type: TIMESTAMP(6) WITH TIME ZONE; to_name: UpdatedAt
    last_login_at         TIMESTAMP(6) WITH TIME ZONE,                                           -- comment: Последний вход; to_type: TIMESTAMP(6) WITH TIME ZONE; to_name: LastLoginAt
    deleted_at            TIMESTAMP(6) WITH TIME ZONE,                                           -- comment: Дата удаления; to_type: TIMESTAMP(6) WITH TIME ZONE; to_name: DeletedAt

    balance               positive_numeric                     DEFAULT 0.00,                     -- comment: Баланс пользователя; to_type: positive_numeric; to_name: UserBalance
    loyalty_points        INTEGER                              DEFAULT 0,                        -- comment: Баллы лояльности; to_type: INTEGER; to_name: LoyaltyPoints
    discount_percent      percentage                           DEFAULT 0,                        -- comment: Процент скидки; to_type: percentage; to_name: DiscountPercent

    is_verified           BOOLEAN                     NOT NULL DEFAULT FALSE,                    -- comment: Email подтвержден; to_type: BOOLEAN; to_name: IsVerified
    is_premium            BOOLEAN                     NOT NULL DEFAULT FALSE,                    -- comment: Премиум аккаунт; to_type: BOOLEAN; to_name: IsPremium
    is_deleted            BOOLEAN                     NOT NULL DEFAULT FALSE,                    -- comment: Удален; to_type: BOOLEAN; to_name: IsDeleted

    avatar                BYTEA,                                                                 -- comment: Аватар пользователя; to_type: BYTEA; to_name: AvatarImage

    external_id           UUID                        NOT NULL DEFAULT gen_random_uuid() UNIQUE, -- comment: Внешний идентификатор; to_type: UUID; to_name: ExternalId

    date_of_birth         DATE,                                                                  -- comment: Дата рождения; to_type: DATE; to_name: BirthDate

    subscription_duration INTERVAL,                                                              -- comment: Длительность подписки; to_type: INTERVAL; to_name: SubscriptionDuration

    active_period         TSTZRANGE,                                                             -- comment: Период активности; to_type: TSTZRANGE; to_name: ActivePeriod

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

COMMENT
    ON TABLE users IS 'Основная таблица пользователей системы';
COMMENT
    ON COLUMN users.id IS 'Уникальный идентификатор пользователя';
COMMENT
    ON COLUMN users.username IS 'Уникальное имя пользователя для входа';
COMMENT
    ON COLUMN users.email IS 'Email адрес пользователя с валидацией';
COMMENT
    ON COLUMN users.password_hash IS 'Хеш пароля (bcrypt/argon2)';
COMMENT
    ON COLUMN users.full_name IS 'Полное имя пользователя';
COMMENT
    ON COLUMN users.status IS 'Текущий статус аккаунта';
COMMENT
    ON COLUMN users.preferences IS 'Пользовательские настройки в формате JSON';
COMMENT
    ON COLUMN users.metadata IS 'Дополнительные метаданные';
COMMENT
    ON COLUMN users.billing_address IS 'Адрес для выставления счетов';
COMMENT
    ON COLUMN users.contact IS 'Контактная информация пользователя';
COMMENT
    ON COLUMN users.phone_numbers IS 'Массив телефонных номеров';
COMMENT
    ON COLUMN users.tags IS 'Теги для категоризации пользователей';
COMMENT
    ON COLUMN users.roles IS 'Роли пользователя в системе';
COMMENT
    ON COLUMN users.created_at IS 'Дата и время регистрации';
COMMENT
    ON COLUMN users.updated_at IS 'Дата и время последнего обновления';
COMMENT
    ON COLUMN users.last_login_at IS 'Дата и время последнего входа';
COMMENT
    ON COLUMN users.deleted_at IS 'Дата и время мягкого удаления';
COMMENT
    ON COLUMN users.balance IS 'Баланс пользователя в основной валюте';
COMMENT
    ON COLUMN users.loyalty_points IS 'Накопленные баллы лояльности';
COMMENT
    ON COLUMN users.discount_percent IS 'Персональная скидка пользователя';
COMMENT
    ON COLUMN users.is_verified IS 'Подтвержден ли email адрес';
COMMENT
    ON COLUMN users.is_premium IS 'Является ли пользователь премиум';
COMMENT
    ON COLUMN users.is_deleted IS 'Флаг мягкого удаления';
COMMENT
    ON COLUMN users.avatar IS 'Аватар пользователя в бинарном формате';
COMMENT
    ON COLUMN users.external_id IS 'UUID для интеграции с внешними системами';
COMMENT
    ON COLUMN users.date_of_birth IS 'Дата рождения';
COMMENT
    ON COLUMN users.subscription_duration IS 'Продолжительность подписки';
COMMENT
    ON COLUMN users.active_period IS 'Период активности аккаунта';

-- Индекс для поиска по email
CREATE INDEX idx_users_email ON users (email) WHERE is_deleted = FALSE;
COMMENT
    ON INDEX idx_users_email IS 'Индекс для быстрого поиска по email активных пользователей';

-- Индекс по статусу
CREATE INDEX idx_users_status ON users (status) WHERE is_deleted = FALSE;
COMMENT
    ON INDEX idx_users_status IS 'Индекс для фильтрации по статусу';

-- Индекс по дате создания
CREATE INDEX idx_users_created_at ON users (created_at DESC);
COMMENT
    ON INDEX idx_users_created_at IS 'Индекс для сортировки по дате создания';

-- GIN индекс для preferences
CREATE INDEX idx_users_preferences ON users USING GIN (preferences);
COMMENT
    ON INDEX idx_users_preferences IS 'GIN индекс для поиска по JSON настройкам';

-- GIN индекс для tags
CREATE INDEX idx_users_tags ON users USING GIN (tags);
COMMENT
    ON INDEX idx_users_tags IS 'GIN индекс для поиска по тегам';

-- Индекс по external_id
CREATE INDEX idx_users_external_id ON users (external_id) WHERE is_deleted = FALSE;
COMMENT
    ON INDEX idx_users_external_id IS 'Индекс для поиска по внешнему идентификатору';

-- Частичный индекс для премиум
CREATE INDEX idx_users_premium ON users (id) WHERE is_premium = TRUE AND is_deleted = FALSE;
COMMENT
    ON INDEX idx_users_premium IS 'Частичный индекс для премиум пользователей';

-- Индекс для удаленных
CREATE INDEX idx_users_deleted_at ON users (deleted_at) WHERE is_deleted = TRUE;
COMMENT
    ON INDEX idx_users_deleted_at IS 'Индекс для удаленных пользователей';

-- Таблица категорий товаров
CREATE TABLE categories
(
    id            SERIAL PRIMARY KEY,                                               -- comment: Идентификатор категории; to_type: SERIAL; to_name: CategoryId

    name          VARCHAR(100)                NOT NULL,                             -- comment: Название категории; to_type: VARCHAR(100); to_name: CategoryName
    slug          VARCHAR(100)                NOT NULL UNIQUE,                      -- comment: URL-идентификатор; to_type: VARCHAR(100); to_name: UrlSlug
    description   TEXT,                                                             -- comment: Описание категории; to_type: TEXT; to_name: Description

    parent_id     INTEGER,                                                          -- comment: Родительская категория; to_type: INTEGER; to_name: ParentCategoryId
    level         INTEGER                     NOT NULL DEFAULT 0,                   -- comment: Уровень вложенности; to_type: INTEGER; to_name: HierarchyLevel
    path          INTEGER[]                   NOT NULL DEFAULT ARRAY []::INTEGER[], -- comment: Путь в иерархии; to_type: INTEGER[]; to_name: HierarchyPath

    search_vector TSVECTOR,                                                         -- comment: Вектор для поиска; to_type: TSVECTOR; to_name: SearchVector

    metadata      JSONB                                DEFAULT '{}',                -- comment: Метаданные категории; to_type: JSONB; to_name: CategoryMetadata

    sort_order    INTEGER                     NOT NULL DEFAULT 0,                   -- comment: Порядок сортировки; to_type: INTEGER; to_name: SortOrder
    is_active     BOOLEAN                     NOT NULL DEFAULT TRUE,                -- comment: Активна ли категория; to_type: BOOLEAN; to_name: IsActive

    created_at    TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,   -- comment: Дата создания; to_type: TIMESTAMP(6) WITH TIME ZONE; to_name: CreatedAt
    updated_at    TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,   -- comment: Дата обновления; to_type: TIMESTAMP(6) WITH TIME ZONE; to_name: UpdatedAt

    CONSTRAINT fk_parent_category FOREIGN KEY (parent_id)
        REFERENCES categories (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT check_level CHECK (level >= 0),
    CONSTRAINT check_sort_order CHECK (sort_order >= 0),
    CONSTRAINT check_no_self_reference CHECK (id != parent_id
        )
);

COMMENT
    ON TABLE categories IS 'Иерархическая структура категорий товаров';
COMMENT
    ON COLUMN categories.id IS 'Уникальный идентификатор категории';
COMMENT
    ON COLUMN categories.name IS 'Название категории';
COMMENT
    ON COLUMN categories.slug IS 'URL-friendly идентификатор';
COMMENT
    ON COLUMN categories.description IS 'Описание категории';
COMMENT
    ON COLUMN categories.parent_id IS 'Ссылка на родительскую категорию';
COMMENT
    ON COLUMN categories.level IS 'Уровень вложенности в иерархии';
COMMENT
    ON COLUMN categories.path IS 'Массив ID предков для быстрого обхода дерева';
COMMENT
    ON COLUMN categories.search_vector IS 'Вектор для полнотекстового поиска';
COMMENT
    ON COLUMN categories.metadata IS 'Дополнительные данные категории';
COMMENT
    ON COLUMN categories.sort_order IS 'Порядок сортировки';
COMMENT
    ON COLUMN categories.is_active IS 'Активна ли категория';
COMMENT
    ON COLUMN categories.created_at IS 'Дата создания';
COMMENT
    ON COLUMN categories.updated_at IS 'Дата последнего обновления';

-- Индекс для поиска дочерних категорий
CREATE INDEX idx_categories_parent_id ON categories (parent_id) WHERE is_active = TRUE;
COMMENT
    ON INDEX idx_categories_parent_id IS 'Индекс для поиска дочерних категорий';

-- GIN индекс для поиска по пути в иерархии
CREATE INDEX idx_categories_path ON categories USING GIN (path);
COMMENT
    ON INDEX idx_categories_path IS 'GIN индекс для поиска по пути в иерархии';

-- GIN индекс для полнотекстового поиска
CREATE INDEX idx_categories_search ON categories USING GIN (search_vector);
COMMENT
    ON INDEX idx_categories_search IS 'GIN индекс для полнотекстового поиска';

-- Индекс для поиска по slug
CREATE INDEX idx_categories_slug ON categories (slug) WHERE is_active = TRUE;
COMMENT
    ON INDEX idx_categories_slug IS 'Индекс для поиска по slug';

-- Индекс для сортировки категорий
CREATE INDEX idx_categories_sort_order ON categories (parent_id, sort_order) WHERE is_active = TRUE;
COMMENT
    ON INDEX idx_categories_sort_order IS 'Индекс для сортировки категорий';

-- Функция для обновления поискового вектора категории
CREATE
    OR REPLACE FUNCTION update_category_search_vector()
    RETURNS TRIGGER AS
$$
BEGIN
    NEW.search_vector
        :=
            setweight(to_tsvector('english', COALESCE(NEW.name, '')), 'A') ||
            setweight(to_tsvector('english', COALESCE(NEW.description, '')), 'B');
    RETURN NEW;
END;
$$
    LANGUAGE plpgsql;

COMMENT
    ON FUNCTION update_category_search_vector() IS 'Автоматически обновляет поисковый вектор при изменении категории';

-- Триггер для автообновления поискового вектора
CREATE TRIGGER trigger_update_category_search_vector
    BEFORE INSERT OR
        UPDATE
    ON categories
    FOR EACH ROW
EXECUTE FUNCTION update_category_search_vector();

-- Функция для обновления пути в иерархии категорий
CREATE
    OR REPLACE FUNCTION update_category_path()
    RETURNS TRIGGER AS
$$
BEGIN
    IF
        NEW.parent_id IS NULL THEN
        NEW.path := ARRAY [NEW.id];
        NEW.level
            := 0;
    ELSE
        SELECT path || NEW.id, level + 1
        INTO NEW.path, NEW.level
        FROM categories
        WHERE id = NEW.parent_id;
    END IF;
    RETURN NEW;
END;
$$
    LANGUAGE plpgsql;

COMMENT
    ON FUNCTION update_category_path() IS 'Автоматически обновляет путь и уровень категории в иерархии';

CREATE TRIGGER trigger_update_category_path
    BEFORE INSERT OR
        UPDATE OF parent_id
    ON categories
    FOR EACH ROW
EXECUTE FUNCTION update_category_path();

COMMENT
    ON TRIGGER trigger_update_category_path ON categories IS 'Триггер для автоматического обновления пути категории';

-- Таблица заказов
CREATE TABLE orders
(
    id               BIGSERIAL PRIMARY KEY,                                                 -- comment: Идентификатор заказа; type: BIGSERIAL; rename: OrderId

    user_id          BIGINT                      NOT NULL,                                  -- comment: Пользователь; type: BIGINT; rename: UserId

    order_number     VARCHAR(50)                 NOT NULL UNIQUE,                           -- comment: Номер заказа; type: VARCHAR(50); rename: OrderNumber

    status           order_status                NOT NULL DEFAULT 'pending',                -- comment: Статус заказа; type: order_status; rename: OrderStatus
    priority         priority_level              NOT NULL DEFAULT 'medium',                 -- comment: Приоритет; type: priority_level; rename: PriorityLevel

    subtotal         positive_numeric            NOT NULL,                                  -- comment: Сумма без налогов; type: positive_numeric; rename: SubtotalAmount
    tax              positive_numeric            NOT NULL DEFAULT 0,                        -- comment: Налог; type: positive_numeric; rename: TaxAmount
    shipping_cost    positive_numeric            NOT NULL DEFAULT 0,                        -- comment: Стоимость доставки; type: positive_numeric; rename: ShippingCost
    discount         positive_numeric            NOT NULL DEFAULT 0,                        -- comment: Скидка; type: positive_numeric; rename: DiscountAmount
    total            positive_numeric            NOT NULL,                                  -- comment: Итоговая сумма; type: positive_numeric; rename: TotalAmount

    shipping_address JSONB                       NOT NULL,                                  -- comment: Адрес доставки; type: JSONB; rename: ShippingAddress
    billing_address  JSONB                       NOT NULL,                                  -- comment: Адрес для счета; type: JSONB; rename: BillingAddress

    payment_method   payment_method              NOT NULL,                                  -- comment: Способ оплаты; type: payment_method; rename: PaymentMethod
    payment_details  JSONB,                                                                 -- comment: Детали оплаты; type: JSONB; rename: PaymentDetails
    payment_status   VARCHAR(20)                 NOT NULL DEFAULT 'pending',                -- comment: Статус оплаты; type: VARCHAR(20); rename: PaymentStatus

    status_history   JSONB                       NOT NULL DEFAULT '[]',                     -- comment: История статусов; type: JSONB; rename: StatusHistory

    created_at       TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,        -- comment: Дата создания; type: TIMESTAMP(6) WITH TIME ZONE; rename: CreatedAt
    updated_at       TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,        -- comment: Дата обновления; type: TIMESTAMP(6) WITH TIME ZONE; rename: UpdatedAt
    confirmed_at     TIMESTAMP(6) WITH TIME ZONE,                                           -- comment: Дата подтверждения; type: TIMESTAMP(6) WITH TIME ZONE; rename: ConfirmedAt
    shipped_at       TIMESTAMP(6) WITH TIME ZONE,                                           -- comment: Дата отправки; type: TIMESTAMP(6) WITH TIME ZONE; rename: ShippedAt
    delivered_at     TIMESTAMP(6) WITH TIME ZONE,                                           -- comment: Дата доставки; type: TIMESTAMP(6) WITH TIME ZONE; rename: DeliveredAt
    cancelled_at     TIMESTAMP(6) WITH TIME ZONE,                                           -- comment: Дата отмены; type: TIMESTAMP(6) WITH TIME ZONE; rename: CancelledAt

    delivery_window  TSTZRANGE,                                                             -- comment: Окно доставки; type: TSTZRANGE; rename: DeliveryWindow

    notes            TEXT,                                                                  -- comment: Заметки; type: TEXT; rename: Notes
    customer_notes   TEXT,                                                                  -- comment: Заметки клиента; type: TEXT; rename: CustomerNotes
    internal_notes   TEXT,                                                                  -- comment: Внутренние заметки; type: TEXT; rename: InternalNotes

    external_id      UUID                        NOT NULL DEFAULT gen_random_uuid() UNIQUE, -- comment: Внешний идентификатор; type: UUID; rename: ExternalId

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

COMMENT
    ON TABLE orders IS 'Заказы пользователей с полной историей статусов';
COMMENT
    ON COLUMN orders.id IS 'Уникальный идентификатор заказа';
COMMENT
    ON COLUMN orders.user_id IS 'ID пользователя, создавшего заказ';
COMMENT
    ON COLUMN orders.order_number IS 'Уникальный номер заказа для отображения клиенту';
COMMENT
    ON COLUMN orders.status IS 'Текущий статус заказа';
COMMENT
    ON COLUMN orders.priority IS 'Приоритет обработки заказа';
COMMENT
    ON COLUMN orders.subtotal IS 'Сумма товаров без налогов и доставки';
COMMENT
    ON COLUMN orders.tax IS 'Сумма налога';
COMMENT
    ON COLUMN orders.shipping_cost IS 'Стоимость доставки';
COMMENT
    ON COLUMN orders.discount IS 'Сумма скидки';
COMMENT
    ON COLUMN orders.total IS 'Итоговая сумма заказа';
COMMENT
    ON COLUMN orders.shipping_address IS 'Адрес доставки в формате JSON';
COMMENT
    ON COLUMN orders.billing_address IS 'Адрес для выставления счета';
COMMENT
    ON COLUMN orders.payment_method IS 'Способ оплаты';
COMMENT
    ON COLUMN orders.payment_details IS 'Детали оплаты (зашифрованные)';
COMMENT
    ON COLUMN orders.payment_status IS 'Статус оплаты';
COMMENT
    ON COLUMN orders.status_history IS 'История изменений статуса [{status, timestamp, user_id, comment}]';
COMMENT
    ON COLUMN orders.created_at IS 'Дата создания заказа';
COMMENT
    ON COLUMN orders.updated_at IS 'Дата последнего обновления';
COMMENT
    ON COLUMN orders.confirmed_at IS 'Дата подтверждения заказа';
COMMENT
    ON COLUMN orders.shipped_at IS 'Дата отправки';
COMMENT
    ON COLUMN orders.delivered_at IS 'Дата доставки';
COMMENT
    ON COLUMN orders.cancelled_at IS 'Дата отмены';
COMMENT
    ON COLUMN orders.delivery_window IS 'Окно времени для доставки';
COMMENT
    ON COLUMN orders.notes IS 'Общие заметки к заказу';
COMMENT
    ON COLUMN orders.customer_notes IS 'Заметки клиента';
COMMENT
    ON COLUMN orders.internal_notes IS 'Внутренние заметки (не видны клиенту)';
COMMENT
    ON COLUMN orders.external_id IS 'Внешний идентификатор для интеграции';

-- Индекс для поиска заказов пользователя
CREATE INDEX idx_orders_user_id ON orders (user_id, created_at DESC);
COMMENT
    ON INDEX idx_orders_user_id IS 'Индекс для поиска заказов пользователя';

-- Индекс для фильтрации по статусу
CREATE INDEX idx_orders_status ON orders (status, created_at DESC);
COMMENT
    ON INDEX idx_orders_status IS 'Индекс для фильтрации по статусу';

-- Индекс для сортировки по дате
CREATE INDEX idx_orders_created_at ON orders (created_at DESC);
COMMENT
    ON INDEX idx_orders_created_at IS 'Индекс для сортировки по дате';

-- Уникальный индекс по номеру заказа
CREATE INDEX idx_orders_order_number ON orders (order_number);
COMMENT
    ON INDEX idx_orders_order_number IS 'Уникальный индекс по номеру заказа';

-- GIN индекс для поиска по адресу доставки
CREATE INDEX idx_orders_shipping_address ON orders USING GIN (shipping_address);
COMMENT
    ON INDEX idx_orders_shipping_address IS 'GIN индекс для поиска по адресу доставки';

-- Составной индекс для активных заказов пользователя
CREATE INDEX idx_orders_user_status ON orders (user_id, status) WHERE cancelled_at IS NULL;
COMMENT
    ON INDEX idx_orders_user_status IS 'Составной индекс для активных заказов пользователя';

-- GIST индекс для поиска по окну доставки
CREATE INDEX idx_orders_delivery_window ON orders USING GIST (delivery_window);
COMMENT
    ON INDEX idx_orders_delivery_window IS 'GIST индекс для поиска по окну доставки';

-- Частичный индекс для неоплаченных заказов
CREATE INDEX idx_orders_payment_status ON orders (payment_status) WHERE payment_status != 'paid';
COMMENT
    ON INDEX idx_orders_payment_status IS 'Частичный индекс для неоплаченных заказов';

-- Таблица позиций заказов
CREATE TABLE order_items
(
    id                  BIGSERIAL PRIMARY KEY,                                          -- comment: Идентификатор позиции; type: BIGSERIAL; rename: OrderItemId

    order_id            BIGINT                      NOT NULL,                           -- comment: Заказ; type: BIGINT; rename: OrderId
    category_id         INTEGER                     NOT NULL,                           -- comment: Категория товара; type: INTEGER; rename: CategoryId

    product_name        VARCHAR(255)                NOT NULL,                           -- comment: Название товара; type: VARCHAR(255); rename: ProductName
    product_description TEXT,                                                           -- comment: Описание товара; type: TEXT; rename: ProductDescription
    sku                 VARCHAR(100),                                                   -- comment: Артикул; type: VARCHAR(100); rename: Sku
    barcode             VARCHAR(50),                                                    -- comment: Штрих-код; type: VARCHAR(50); rename: Barcode

    quantity            INTEGER                     NOT NULL,                           -- comment: Количество; type: INTEGER; rename: Quantity
    unit_price          positive_numeric            NOT NULL,                           -- comment: Цена за единицу; type: positive_numeric; rename: UnitPrice
    discount_percent    percentage                           DEFAULT 0,                 -- comment: Процент скидки; type: percentage; rename: DiscountPercent
    discount_amount     positive_numeric                     DEFAULT 0,                 -- comment: Сумма скидки; type: positive_numeric; rename: DiscountAmount
    tax_percent         percentage                           DEFAULT 0,                 -- comment: Процент налога; type: percentage; rename: TaxPercent
    tax_amount          positive_numeric                     DEFAULT 0,                 -- comment: Сумма налога; type: positive_numeric; rename: TaxAmount
    total_price         positive_numeric            NOT NULL,                           -- comment: Итоговая цена; type: positive_numeric; rename: TotalPrice

    attributes          JSONB                                DEFAULT '{}',              -- comment: Атрибуты товара; type: JSONB; rename: ProductAttributes

    weight_grams        INTEGER,                                                        -- comment: Вес в граммах; type: INTEGER; rename: WeightGrams
    dimensions          JSONB,                                                          -- comment: Размеры; type: JSONB; rename: Dimensions

    created_at          TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP, -- comment: Дата создания; type: TIMESTAMP(6) WITH TIME ZONE; rename: CreatedAt

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

COMMENT
    ON TABLE order_items IS 'Позиции (товары) в заказах';
COMMENT
    ON COLUMN order_items.id IS 'Уникальный идентификатор позиции';
COMMENT
    ON COLUMN order_items.order_id IS 'ID заказа';
COMMENT
    ON COLUMN order_items.category_id IS 'ID категории товара';
COMMENT
    ON COLUMN order_items.product_name IS 'Название товара';
COMMENT
    ON COLUMN order_items.product_description IS 'Описание товара';
COMMENT
    ON COLUMN order_items.sku IS 'Артикул товара';
COMMENT
    ON COLUMN order_items.barcode IS 'Штрих-код товара';
COMMENT
    ON COLUMN order_items.quantity IS 'Количество единиц товара';
COMMENT
    ON COLUMN order_items.unit_price IS 'Цена за единицу';
COMMENT
    ON COLUMN order_items.discount_percent IS 'Процент скидки';
COMMENT
    ON COLUMN order_items.discount_amount IS 'Сумма скидки';
COMMENT
    ON COLUMN order_items.tax_percent IS 'Процент налога';
COMMENT
    ON COLUMN order_items.tax_amount IS 'Сумма налога';
COMMENT
    ON COLUMN order_items.total_price IS 'Итоговая стоимость позиции';
COMMENT
    ON COLUMN order_items.attributes IS 'Дополнительные атрибуты (цвет, размер и т.д.)';
COMMENT
    ON COLUMN order_items.weight_grams IS 'Вес товара в граммах';
COMMENT
    ON COLUMN order_items.dimensions IS 'Габариты товара {length, width, height}';
COMMENT
    ON COLUMN order_items.created_at IS 'Дата добавления в заказ';

-- Индекс для поиска позиций заказа
CREATE INDEX idx_order_items_order_id ON order_items (order_id);
COMMENT
    ON INDEX idx_order_items_order_id IS 'Индекс для поиска позиций заказа';

-- Индекс для поиска по категории
CREATE INDEX idx_order_items_category_id ON order_items (category_id);
COMMENT
    ON INDEX idx_order_items_category_id IS 'Индекс для поиска по категории';

-- Индекс для поиска по артикулу
CREATE INDEX idx_order_items_sku ON order_items (sku) WHERE sku IS NOT NULL;
COMMENT
    ON INDEX idx_order_items_sku IS 'Индекс для поиска по артикулу';

-- GIN индекс для поиска по атрибутам
CREATE INDEX idx_order_items_attributes ON order_items USING GIN (attributes);
COMMENT
    ON INDEX idx_order_items_attributes IS 'GIN индекс для поиска по атрибутам';

-- Представление активных пользователей с агрегированной статистикой по заказам
CREATE VIEW active_users_with_orders AS
SELECT u.id,                                                       -- comment: Идентификатор пользователя; type: BIGINT; rename: UserId
       u.username,                                                 -- comment: Логин; type: VARCHAR(50); rename: UserName
       u.email,                                                    -- comment: Email; type: email; rename: EmailAddress
       u.full_name,                                                -- comment: Полное имя; type: VARCHAR(150); rename: FullName
       u.status,                                                   -- comment: Статус; type: user_status; rename: UserStatus
       u.is_premium,                                               -- comment: Премиум статус; type: BOOLEAN; rename: IsPremium
       u.balance,                                                  -- comment: Баланс; type: positive_numeric; rename: Balance
       u.loyalty_points,                                           -- comment: Баллы лояльности; type: INTEGER; rename: LoyaltyPoints
       COUNT(DISTINCT o.id)                AS total_orders,        -- comment: Всего заказов; type: BIGINT; rename: TotalOrders
       COALESCE(SUM(o.total), 0)           AS total_spent,         -- comment: Всего потрачено; type: NUMERIC; rename: TotalSpent
       COALESCE(AVG(o.total), 0)           AS average_order_value, -- comment: Средний чек; type: NUMERIC; rename: AverageOrderValue
       MAX(o.created_at)                   AS last_order_date,     -- comment: Дата последнего заказа; type: TIMESTAMP(6) WITH TIME ZONE; rename: LastOrderDate
       ARRAY_AGG(DISTINCT o.status ORDER BY o.status)
       FILTER (WHERE o.status IS NOT NULL) AS order_statuses       -- comment: Статусы заказов; type: order_status[]; rename: OrderStatuses COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'pending') AS pending_orders, -- comment: Заказов в ожидании; type: BIGINT; rename: PendingOrders COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'delivered') AS completed_orders -- comment: Выполненных заказов; type: BIGINT; rename: CompletedOrders
FROM users u
         LEFT JOIN orders o ON u.id = o.user_id AND o.cancelled_at IS NULL
WHERE u.status = 'active'
  AND u.is_deleted = FALSE
GROUP BY u.id, u.username, u.email, u.full_name, u.status, u.is_premium, u.balance, u.loyalty_points;

COMMENT
    ON VIEW active_users_with_orders IS 'Представление активных пользователей с агрегированной статистикой по заказам';

-- Материализованное представление статистики продаж по категориям
CREATE MATERIALIZED VIEW category_statistics AS
SELECT c.id,                                                 -- comment: Идентификатор категории; type: INTEGER; rename: CategoryId
       c.name,                                               -- comment: Название категории; type: VARCHAR(100); rename: CategoryName
       c.slug,                                               -- comment: URL-идентификатор; type: VARCHAR(100); rename: UrlSlug
       c.level,                                              -- comment: Уровень вложенности; type: INTEGER; rename: HierarchyLevel
       c.parent_id,                                          -- comment: Родительская категория; type: INTEGER; rename: ParentCategoryId
       COUNT(oi.id)                     AS total_items_sold, -- comment: Всего позиций продано; type: BIGINT; rename: TotalItemsSold
       COALESCE(SUM(oi.quantity), 0)    AS total_quantity,   -- comment: Общее количество; type: NUMERIC; rename: TotalQuantity
       COALESCE(SUM(oi.total_price), 0) AS total_revenue,    -- comment: Общая выручка; type: NUMERIC; rename: TotalRevenue
       COALESCE(AVG(oi.unit_price), 0)  AS avg_price,        -- comment: Средняя цена; type: NUMERIC; rename: AveragePrice
       COALESCE(MIN(oi.unit_price), 0)  AS min_price,        -- comment: Минимальная цена; type: NUMERIC; rename: MinPrice
       COALESCE(MAX(oi.unit_price), 0)  AS max_price,        -- comment: Максимальная цена; type: NUMERIC; rename: MaxPrice
       COUNT(DISTINCT oi.order_id)      AS unique_orders,    -- comment: Уникальных заказов; type: BIGINT; rename: UniqueOrders
       COUNT(DISTINCT o.user_id)        AS unique_customers, -- comment: Уникальных покупателей; type: BIGINT; rename: UniqueCustomers
       MAX(o.created_at)                AS last_order_date   -- comment: Дата последнего заказа; type: TIMESTAMP(6) WITH TIME ZONE; rename: LastOrderDate
FROM categories c
         LEFT JOIN order_items oi ON c.id = oi.category_id
         LEFT JOIN orders o ON oi.order_id = o.id AND o.cancelled_at IS NULL
WHERE c.is_active = TRUE
GROUP BY c.id, c.name, c.slug, c.level, c.parent_id;

-- Уникальный индекс для материализованного представления
CREATE UNIQUE INDEX idx_category_statistics_id ON category_statistics (id);
COMMENT
    ON INDEX idx_category_statistics_id IS 'Уникальный индекс для материализованного представления';

-- Индекс для сортировки по выручке
CREATE INDEX idx_category_statistics_revenue ON category_statistics (total_revenue DESC);
COMMENT
    ON INDEX idx_category_statistics_revenue IS 'Индекс для сортировки по выручке';

COMMENT
    ON MATERIALIZED VIEW category_statistics IS 'Статистика продаж по категориям (требует периодического REFRESH MATERIALIZED VIEW)';

-- Функция для получения полного пути категории
CREATE
    OR REPLACE FUNCTION get_category_path(category_id INTEGER) -- comment: ID категории; type: INTEGER; rename: CategoryId
    RETURNS TEXT AS
$$
DECLARE
    result TEXT    := '';
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
               CASE
                   WHEN result = '' THEN ''
                   ELSE ' > ' || result
                   END;

        EXIT
            WHEN parent IS NULL;
        current_id
            := parent;
    END LOOP;

    RETURN result;
END;
$$
    LANGUAGE plpgsql STABLE;

COMMENT
    ON FUNCTION get_category_path(INTEGER) IS 'Возвращает полный путь категории от корня в формате "Parent > Child > Grandchild"';

-- Функция для получения всех дочерних категорий
CREATE
    OR REPLACE FUNCTION get_child_categories(parent_category_id INTEGER) -- comment: ID родительской категории; type: INTEGER; rename: ParentCategoryId
    RETURNS TABLE
            (
                id    INTEGER,
                name  VARCHAR,
                level INTEGER
            )
AS
$$
BEGIN
    RETURN QUERY WITH RECURSIVE subcategories AS (SELECT c.id, c.name, c.level, c.parent_id
                                                  FROM categories c
                                                  WHERE c.parent_id = parent_category_id

                                                  UNION ALL

                                                  SELECT c.id, c.name, c.level, c.parent_id
                                                  FROM categories c
                                                           INNER JOIN subcategories s ON c.parent_id = s.id)
                 SELECT sc.id, sc.name, sc.level
                 FROM subcategories sc
                 ORDER BY sc.level, sc.name;
END;
$$
    LANGUAGE plpgsql STABLE;

COMMENT
    ON FUNCTION get_child_categories(INTEGER) IS 'Возвращает все дочерние категории рекурсивно';

-- Функция для автообновления колонки updated_at
CREATE
    OR REPLACE FUNCTION update_updated_at_column()
    RETURNS TRIGGER AS
$$
BEGIN
    NEW.updated_at
        = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$
    LANGUAGE plpgsql;

COMMENT
    ON FUNCTION update_updated_at_column() IS 'Автоматически обновляет колонку updated_at при изменении записи';

-- Триггер для автообновления updated_at в users
CREATE TRIGGER trigger_users_updated_at
    BEFORE UPDATE
    ON users
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

COMMENT
    ON TRIGGER trigger_users_updated_at ON users IS 'Автоматически обновляет updated_at при изменении пользователя';

-- Триггер для автообновления updated_at в categories
CREATE TRIGGER trigger_categories_updated_at
    BEFORE UPDATE
    ON categories
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

COMMENT
    ON TRIGGER trigger_categories_updated_at ON categories IS 'Автоматически обновляет updated_at при изменении категории';

-- Триггер для автообновления updated_at в orders
CREATE TRIGGER trigger_orders_updated_at
    BEFORE UPDATE
    ON orders
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

COMMENT
    ON TRIGGER trigger_orders_updated_at ON orders IS 'Автоматически обновляет updated_at при изменении заказа';

-- Функция для автодобавления записи в историю статуса заказа
CREATE
    OR REPLACE FUNCTION add_order_status_history()
    RETURNS TRIGGER AS
$$
BEGIN
    IF
        NEW.status IS DISTINCT FROM OLD.status THEN
        NEW.status_history := OLD.status_history || jsonb_build_object(
                'status', NEW.status,
                'previous_status', OLD.status,
                'timestamp', CURRENT_TIMESTAMP,
                'changed_by', current_user
                                                    );
    END IF;
    RETURN NEW;
END;
$$
    LANGUAGE plpgsql;

COMMENT
    ON FUNCTION add_order_status_history() IS 'Автоматически добавляет запись в историю при изменении статуса заказа';

-- Триггер для автодобавления в историю статуса
CREATE TRIGGER trigger_order_status_history
    BEFORE UPDATE OF status
    ON orders
    FOR EACH ROW
    WHEN (OLD.status IS DISTINCT FROM NEW.status)
EXECUTE FUNCTION add_order_status_history();

COMMENT
    ON TRIGGER trigger_order_status_history ON orders IS 'Триггер для автоматического ведения истории изменений статуса';

-- Партиционированная таблица для аудита
CREATE TABLE audit_logs
(
    id          BIGSERIAL,                                                      -- comment: Идентификатор записи; type: BIGSERIAL; rename: AuditLogId
    user_id     BIGINT,                                                         -- comment: Пользователь; type: BIGINT; rename: UserId
    action      VARCHAR(50)                 NOT NULL,                           -- comment: Тип действия; type: VARCHAR(50); rename: Action
    entity_type VARCHAR(50)                 NOT NULL,                           -- comment: Тип сущности; type: VARCHAR(50); rename: EntityType
    entity_id   BIGINT,                                                         -- comment: ID сущности; type: BIGINT; rename: EntityId
    old_values  JSONB,                                                          -- comment: Старые значения; type: JSONB; rename: OldValues
    new_values  JSONB,                                                          -- comment: Новые значения; type: JSONB; rename: NewValues
    ip_address  INET,                                                           -- comment: IP адрес; type: INET; rename: IpAddress
    user_agent  TEXT,                                                           -- comment: User Agent; type: TEXT; rename: UserAgent
    created_at  TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP, -- comment: Дата создания; type: TIMESTAMP(6) WITH TIME ZONE; rename: CreatedAt
    PRIMARY KEY (id, created_at)
) PARTITION BY RANGE (created_at);

COMMENT
    ON TABLE audit_logs IS 'Партиционированная таблица для аудита всех действий в системе';
COMMENT
    ON COLUMN audit_logs.id IS 'Уникальный идентификатор записи лога';
COMMENT
    ON COLUMN audit_logs.user_id IS 'ID пользователя, совершившего действие';
COMMENT
    ON COLUMN audit_logs.action IS 'Тип действия (CREATE, UPDATE, DELETE и т.д.)';
COMMENT
    ON COLUMN audit_logs.entity_type IS 'Тип сущности (users, orders, categories и т.д.)';
COMMENT
    ON COLUMN audit_logs.entity_id IS 'ID измененной сущности';
COMMENT
    ON COLUMN audit_logs.old_values IS 'Старые значения полей (для UPDATE и DELETE)';
COMMENT
    ON COLUMN audit_logs.new_values IS 'Новые значения полей (для CREATE и UPDATE)';
COMMENT
    ON COLUMN audit_logs.ip_address IS 'IP адрес пользователя';
COMMENT
    ON COLUMN audit_logs.user_agent IS 'User Agent браузера/приложения';
COMMENT
    ON COLUMN audit_logs.created_at IS 'Время совершения действия';

-- Партиция логов за Q1 2024
CREATE TABLE audit_logs_2024_q1 PARTITION OF audit_logs
    FOR VALUES FROM
        (
        '2024-01-01'
        ) TO
        (
        '2024-04-01'
        );
COMMENT
    ON TABLE audit_logs_2024_q1 IS 'Партиция логов за Q1 2024';

-- Партиция логов за Q2 2024
CREATE TABLE audit_logs_2024_q2 PARTITION OF audit_logs
    FOR VALUES FROM
        (
        '2024-04-01'
        ) TO
        (
        '2024-07-01'
        );
COMMENT
    ON TABLE audit_logs_2024_q2 IS 'Партиция логов за Q2 2024';

-- Партиция логов за Q3 2024
CREATE TABLE audit_logs_2024_q3 PARTITION OF audit_logs
    FOR VALUES FROM
        (
        '2024-07-01'
        ) TO
        (
        '2024-10-01'
        );
COMMENT
    ON TABLE audit_logs_2024_q3 IS 'Партиция логов за Q3 2024';

-- Партиция логов за Q4 2024
CREATE TABLE audit_logs_2024_q4 PARTITION OF audit_logs
    FOR VALUES FROM
        (
        '2024-10-01'
        ) TO
        (
        '2025-01-01'
        );
COMMENT
    ON TABLE audit_logs_2024_q4 IS 'Партиция логов за Q4 2024';

-- Индекс для поиска действий пользователя
CREATE INDEX idx_audit_logs_user_id ON audit_logs (user_id, created_at DESC);
COMMENT
    ON INDEX idx_audit_logs_user_id IS 'Индекс для поиска действий пользователя';

-- Индекс для поиска изменений конкретной сущности
CREATE INDEX idx_audit_logs_entity ON audit_logs (entity_type, entity_id, created_at DESC);
COMMENT
    ON INDEX idx_audit_logs_entity IS 'Индекс для поиска изменений конкретной сущности';

-- Индекс для фильтрации по типу действия
CREATE INDEX idx_audit_logs_action ON audit_logs (action, created_at DESC);
COMMENT
    ON INDEX idx_audit_logs_action IS 'Индекс для фильтрации по типу действия';
