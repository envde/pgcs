-- Таблица пользователей
CREATE TABLE users
(
    id               BIGSERIAL PRIMARY KEY,                                 -- Первичный ключ
    username         VARCHAR(50)                 NOT NULL UNIQUE,           -- Имя пользователя
    email            email                       NOT NULL UNIQUE,           -- Электронная почта
    password_hash    VARCHAR(255)                NOT NULL,                  -- comment: Хэш пароля; to_type: VARCHAR(255); to_name: PasswordHash;
    full_name        VARCHAR(255),                                          -- Полное имя пользователя
    status           user_status                 NOT NULL DEFAULT 'active', -- Статус аккаунта

    -- JSON данные для гибкости
    preferences      JSONB                                DEFAULT '{}',
    metadata         JSONB,
    billing_address  address, -- comment: Адрес; to_type: address;
    contact          contact_info, -- comment: Информация о контакте; to_name: ContactInfo;

    -- Массивы
    phone_numbers    phone_number[],
    tags             TEXT[],
    roles            VARCHAR(50)[],

    -- Временные метки
    created_at       TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at       TIMESTAMP(6) WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at    TIMESTAMP(6) WITH TIME ZONE,
    deleted_at       TIMESTAMP(6) WITH TIME ZONE,

    -- Числовые типы
    balance          positive_numeric                     DEFAULT 0.00,
    loyalty_points   INTEGER                              DEFAULT 0,
    discount_percent percentage                           DEFAULT 0,

    -- Булевы флаги
    is_verified      BOOLEAN                     NOT NULL DEFAULT FALSE,
    is_premium       BOOLEAN                     NOT NULL DEFAULT FALSE,
    is_deleted       BOOLEAN                     NOT NULL DEFAULT FALSE,

    -- Бинарные данные
    avatar           BYTEA,

    -- UUID для внешней интеграции
    external_id      UUID                        NOT NULL DEFAULT gen_random_uuid() UNIQUE,

    -- Дата рождения
    date_of_birth    DATE,

    -- Интервал подписки
    subscription_duration INTERVAL,

    -- Диапазон дат активности
    active_period    TSTZRANGE,

    -- Ограничения
    CONSTRAINT check_balance CHECK (balance >= 0),
    CONSTRAINT check_loyalty_points CHECK (loyalty_points >= 0),
    CONSTRAINT check_age CHECK (
        date_of_birth IS NULL OR
        date_of_birth < CURRENT_DATE - INTERVAL '13 years'
) ,
    CONSTRAINT check_deleted_at CHECK (
        (is_deleted = FALSE AND deleted_at IS NULL) OR
        (is_deleted = TRUE AND deleted_at IS NOT NULL)
    )
);