-- Таблица пользователей
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