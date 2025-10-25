-- ==================================================================
-- Примеры SQL запросов для демонстрации возможностей PgCs
-- ==================================================================

-- ==================================================================
-- SELECT запросы - возврат одного объекта (:one)
-- ==================================================================

-- name: GetUserById :one
-- Получить пользователя по ID
SELECT id,
       username,
       email,
       full_name,
       status,
       preferences,
       phone_numbers,
       tags,
       balance,
       loyalty_points,
       is_verified,
       is_premium,
       external_id,
       date_of_birth,
       created_at,
       updated_at,
       last_login_at
FROM users
WHERE id = $1;

-- name: GetUserByEmail :one
-- Получить пользователя по email (использует DOMAIN тип)
SELECT id,
       username,
       email,
       full_name,
       status,
       balance
FROM users
WHERE email = $1;

-- name: GetUserByExternalId :one
-- Поиск по UUID
SELECT *
FROM users
WHERE external_id = $1;

-- name: GetOrderByNumber :one
-- Получить заказ по номеру (работа с ENUM и JSONB)
SELECT id,
       user_id,
       order_number,
       status,
       subtotal,
       tax,
       shipping_cost,
       discount,
       total,
       shipping_address,
       billing_address,
       payment_method,
       payment_details,
       status_history,
       created_at,
       shipped_at,
       delivered_at
FROM orders
WHERE order_number = $1;

-- name: GetCategoryBySlug :one
SELECT id,
       name,
       slug,
       description,
       parent_id,
       created_at
FROM categories
WHERE slug = $1;

-- ==================================================================
-- SELECT запросы - возврат множества (:many)
-- ==================================================================

-- name: ListUsers :many
-- Список пользователей с фильтрацией по статусу
SELECT id,
       username,
       email,
       full_name,
       status,
       balance,
       is_premium,
       created_at
FROM users
WHERE status = $1
ORDER BY created_at DESC;

-- name: ListActiveUsers :many
-- Список активных пользователей (без параметров)
SELECT id,
