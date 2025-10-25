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
       username,
       email,
       status,
       balance
FROM users
WHERE status = 'active'
ORDER BY balance DESC;

-- name: ListUsersByTags :many
-- Поиск пользователей по массиву тегов (работа с ARRAY)
SELECT id,
       username,
       email,
       tags
FROM users
WHERE tags && $1::text[]
ORDER BY created_at DESC;

-- name: SearchUsersByPreferences :many
-- Поиск по JSONB (например, пользователи с темной темой)
SELECT id,
       username,
       email,
       preferences
FROM users
WHERE preferences @ > $1::jsonb
ORDER BY created_at DESC;

-- name: ListOrdersByUser :many
-- Все заказы пользователя
SELECT id,
       order_number,
       status,
       total,
       payment_method,
       created_at,
       shipped_at,
       delivered_at
FROM orders
WHERE user_id = $1
ORDER BY created_at DESC;

-- name: ListOrdersByStatus :many
-- Заказы по статусу и диапазону дат
SELECT id,
       user_id,
       order_number,
       status,
       total,
       created_at
FROM orders
WHERE status = $1
  AND created_at BETWEEN $2 AND $3
ORDER BY created_at DESC;

-- name: ListOrdersByMultipleStatuses :many
-- Заказы по нескольким статусам (работа с ANY)
SELECT id,
       order_number,
       status,
       total,
       created_at
FROM orders
WHERE status = ANY ($1::order_status[])
ORDER BY created_at DESC;

-- name: ListOrderItemsByOrder :many
-- Позиции конкретного заказа
SELECT id,
       order_id,
       category_id,
       product_name,
       sku,
       quantity,
       unit_price,
       discount_percent,
       total_price,
       attributes,
       created_at
FROM order_items
WHERE order_id = $1
ORDER BY id;

-- name: ListCategoriesWithParent :many
-- Подкатегории
SELECT id,
       name,
       slug,
       description,
       parent_id
FROM categories
WHERE parent_id = $1
ORDER BY name;

-- name: ListTopLevelCategories :many
SELECT id,
       name,
       slug,
       description
FROM categories
WHERE parent_id IS NULL
ORDER BY name;

-- name: SearchCategories :many
-- Полнотекстовый поиск по категориям
SELECT id,
       name,
       slug,
       description,
       ts_rank(search_vector, to_tsquery('english', $1)) as rank
FROM categories
WHERE search_vector @@ to_tsquery('english', $1)
ORDER BY rank DESC;

-- ==================================================================
-- JOIN запросы
-- ==================================================================

-- name: GetOrderWithUser :one
-- Заказ с информацией о пользователе
SELECT o.id,
       o.order_number,
       o.status,
       o.total,
       o.created_at,
       u.id as user_id,
       u.username,
       u.email,
       u.full_name
FROM orders o
         INNER JOIN users u ON o.user_id = u.id
WHERE o.id = $1;

-- name: ListOrdersWithUserInfo :many
-- Список заказов с информацией о пользователях
SELECT o.id,
       o.order_number,
       o.status,
       o.total,
       o.created_at,
       u.username,
       u.email
FROM orders o
         INNER JOIN users u ON o.user_id = u.id
WHERE o.status = $1
ORDER BY o.created_at DESC LIMIT $2;

-- name: GetOrderWithItems :many
-- Заказ со всеми позициями (множественный JOIN)
SELECT o.id     as order_id,
       o.order_number,
       o.status as order_status,
       o.total  as order_total,
       oi.id    as item_id,
       oi.product_name,
       oi.quantity,
       oi.unit_price,
       oi.total_price,
       c.name   as category_name
FROM orders o
         LEFT JOIN order_items oi ON o.id = oi.order_id
         LEFT JOIN categories c ON oi.category_id = c.id
WHERE o.id = $1
ORDER BY oi.id;

-- name: GetUserOrdersSummary :one
-- Агрегированная информация о заказах пользователя
SELECT u.id,
       u.username,
       u.email,
       COUNT(o.id)               as total_orders,
       COALESCE(SUM(o.total), 0) as total_spent,
       MAX(o.created_at)         as last_order_date
FROM users u
         LEFT JOIN orders o ON u.id = o.user_id
WHERE u.id = $1
GROUP BY u.id, u.username, u.email;

-- ==================================================================
-- INSERT запросы (:exec - ничего не возвращает, :one - возврат ID)
-- ==================================================================

-- name: CreateUser :one
-- Создать пользователя с возвратом ID
INSERT INTO users (username,
                   email,
                   password_hash,
                   full_name,
                   status,
                   preferences,
                   phone_numbers,
                   tags)
VALUES ($1, $2, $3, $4, $5, $6, $7, $8) RETURNING id, external_id, created_at;

-- name: CreateUserSimple :exec
-- Создать пользователя без возврата
INSERT INTO users (username,
                   email,
                   password_hash,
                   full_name)
VALUES ($1, $2, $3, $4);

-- name: CreateCategory :one
INSERT INTO categories (name,
                        slug,
                        description,
                        parent_id)
VALUES ($1, $2, $3, $4) RETURNING id, created_at;

-- name: CreateOrder :one
-- Создать заказ (работа с JSONB и ENUM)
INSERT INTO orders (user_id,
                    order_number,
                    status,
                    subtotal,
                    tax,
                    shipping_cost,
                    total,
                    shipping_address,
                    billing_address,
                    payment_method)
VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10) RETURNING id, order_number, created_at;

-- name: CreateOrderItem :one
INSERT INTO order_items (order_id,
                         category_id,
                         product_name,
                         sku,
                         quantity,
                         unit_price,
                         total_price,
                         attributes)
VALUES ($1, $2, $3, $4, $5, $6, $7, $8) RETURNING id;

-- name: CreateOrderBatch :exec
-- Пакетная вставка (можно будет сгенерировать метод с массивом параметров)
INSERT INTO order_items (order_id,
                         category_id,
                         product_name,
                         quantity,
                         unit_price,
                         total_price)
SELECT $1,
       unnest($2::integer[]),
       unnest($3::text[]),
       unnest($4::integer[]),
       unnest($5::numeric[]),
       unnest($6::numeric[]);

-- ==================================================================
-- UPDATE запросы (:exec - количество обновленных, :execrows - то же)
-- ==================================================================

-- name: UpdateUserStatus :execrows
-- Обновить статус пользователя
UPDATE users
SET status     = $2,
    updated_at = NOW()
WHERE id = $1;

-- name: UpdateUserBalance :execrows
-- Обновить баланс пользователя
UPDATE users
SET balance    = balance + $2,
    updated_at = NOW()
WHERE id = $1;

-- name: UpdateUserPreferences :exec
-- Обновить JSONB настройки
UPDATE users
SET preferences = preferences || $2::jsonb,
    updated_at = NOW()
WHERE id = $1;

-- name: AddUserTag :exec
-- Добавить тег в массив
UPDATE users
SET tags       = array_append(tags, $2),
    updated_at = NOW()
WHERE id = $1;

-- name: UpdateOrderStatus :execrows
-- Обновить статус заказа с добавлением в историю
UPDATE orders
SET status         = $2,
    status_history = status_history || jsonb_build_object(
            'status', $2::text,
            'timestamp', NOW(),
            'notes', $3
                                       ),
    updated_at     = NOW()
WHERE id = $1;

-- name: MarkOrderShipped :execrows
UPDATE orders
SET status     = 'shipped',
    shipped_at = NOW(),
    updated_at = NOW()
WHERE id = $1;

-- name: UpdateCategoryParent :exec
UPDATE categories
SET parent_id = $2
WHERE id = $1;

-- ==================================================================
-- DELETE запросы
-- ==================================================================

-- name: DeleteUser :execrows
-- Мягкое удаление (изменение статуса)
UPDATE users
SET status     = 'deleted',
    updated_at = NOW()
WHERE id = $1;

-- name: HardDeleteUser :execrows
-- Полное удаление
DELETE
FROM users
WHERE id = $1;

-- name: DeleteOrder :execrows
DELETE
FROM orders
WHERE id = $1;

-- name: DeleteOrdersByUser :execrows
DELETE
FROM orders
WHERE user_id = $1
  AND status = $2;

-- name: DeleteCategory :execrows
DELETE
FROM categories
WHERE id = $1;

-- ==================================================================
-- Сложные запросы с подзапросами и CTE
-- ==================================================================

-- name: GetUsersWithOrderStats :many
-- Пользователи со статистикой заказов (CTE)
WITH order_stats AS (SELECT user_id,
                            COUNT(*)        as order_count,
                            SUM(total)      as total_spent,
                            MAX(created_at) as last_order_date
                     FROM orders
                     WHERE status != 'cancelled'
GROUP BY user_id
    )
SELECT u.id,
       u.username,
       u.email,
       u.status,
       u.balance,
       COALESCE(os.order_count, 0) as order_count,
       COALESCE(os.total_spent, 0) as total_spent,
       os.last_order_date
FROM users u
         LEFT JOIN order_stats os ON u.id = os.user_id
WHERE u.status = $1
ORDER BY os.total_spent DESC NULLS LAST LIMIT $2;

-- name: GetTopSellingCategories :many
-- Топ категорий по продажам
WITH category_sales AS (SELECT oi.category_id,
                               SUM(oi.total_price)         as revenue,
                               SUM(oi.quantity)            as items_sold,
                               COUNT(DISTINCT oi.order_id) as order_count
                        FROM order_items oi
                                 INNER JOIN orders o ON oi.order_id = o.id
                        WHERE o.created_at >= $1
                          AND o.status != 'cancelled'
GROUP BY oi.category_id
    )
SELECT c.id,
       c.name,
       c.slug,
       cs.revenue,
       cs.items_sold,
       cs.order_count
FROM categories c
         INNER JOIN category_sales cs ON c.id = cs.category_id
ORDER BY cs.revenue DESC LIMIT $2;

-- name: GetOrdersInDateRange :many
-- Заказы в диапазоне дат с TSTZRANGE
SELECT id,
       order_number,
       status,
       total,
       created_at
FROM orders
WHERE delivery_window && tstzrange($1::timestamptz, $2::timestamptz)
ORDER BY created_at DESC;

-- ==================================================================
-- Запросы с использованием WINDOW функций
-- ==================================================================

-- name: GetUsersWithRanking :many
-- Пользователи с рейтингом по балансу
SELECT id,
       username,
       email,
       balance,
       RANK() OVER (ORDER BY balance DESC) as balance_rank, PERCENT_RANK() OVER (ORDER BY balance DESC) as balance_percentile
FROM users
WHERE status = 'active'
ORDER BY balance DESC LIMIT $1;

-- name: GetOrdersWithRunningTotal :many
-- Заказы с накопительным итогом
SELECT id,
       order_number,
       total,
       created_at,
       SUM(total) OVER (
        PARTITION BY user_id 
        ORDER BY created_at 
        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
    ) as running_total
FROM orders
WHERE user_id = $1
ORDER BY created_at;

-- ==================================================================
-- Запросы с COALESCE, NULLIF и условиями
-- ==================================================================

-- name: GetUsersWithDefaultPreferences :many
SELECT id,
       username,
       email,
       COALESCE(preferences, '{}'::jsonb) as preferences,
       NULLIF(full_name, '')              as full_name
FROM users
WHERE status = ANY ($1::user_status[])
ORDER BY id LIMIT $2
OFFSET $3;

-- ==================================================================
-- Запросы для работы с массивами
-- ==================================================================

-- name: GetUsersWithPhoneNumber :many
-- Поиск по номеру телефона в массиве
SELECT id,
       username,
       email,
       phone_numbers
FROM users
WHERE $1 = ANY (phone_numbers);

-- name: GetUsersWithAllTags :many
-- Пользователи, имеющие все указанные теги
SELECT id,
       username,
       email,
       tags
FROM users
WHERE tags @ > $1::text[]
ORDER BY created_at DESC;

-- ==================================================================
-- Запросы с JSONB операторами
-- ==================================================================

-- name: GetUsersWithJsonbPath :many
-- Извлечение данных из JSONB по пути
SELECT id,
       username,
       preferences,
       preferences - > 'theme' as theme,
       preferences ->>'language' as language
FROM users
WHERE preferences ? $1
ORDER BY created_at DESC;

-- name: UpdateJsonbField :exec
-- Обновление конкретного поля в JSONB
UPDATE users
SET preferences = jsonb_set(
        preferences,
        $2::text[],
        to_jsonb($3::text)
                  ),
    updated_at  = NOW()
WHERE id = $1;

-- ==================================================================
-- Запросы с функциями
-- ==================================================================

-- name: GetCategoryWithPath :one
-- Использование пользовательской функции
SELECT id,
       name,
       slug,
       get_category_path(id) as full_path
FROM categories
WHERE id = $1;

-- ==================================================================
-- Запросы для Views
-- ==================================================================

-- name: GetActiveUsersWithOrdersView :many
-- Запрос к представлению
SELECT id,
       username,
       email,
       total_orders,
       total_spent
FROM active_users_with_orders
WHERE is_premium = $1
ORDER BY total_spent DESC LIMIT $2;

-- name: GetCategoryStatistics :one
-- Запрос к материализованному представлению
SELECT id,
       name,
       total_items_sold,
       total_revenue,
       avg_price
FROM category_statistics
WHERE id = $1;

-- ==================================================================
-- Транзакционные операции (для примера batch операций)
-- ==================================================================

-- name: GetUserForUpdate :one
-- SELECT FOR UPDATE (для транзакций)
SELECT id,
       username,
       balance
FROM users
WHERE id = $1
    FOR UPDATE;