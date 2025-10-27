-- Примеры SQL запросов для тестирования анализатора PgCs
-- Используется синтаксис sqlc с аннотациями для генерации кода
-- Совместимо с PostgreSQL 18

-- ============================================================
-- USERS - Запросы для работы с пользователями
-- ============================================================

-- name: GetUserById :one
-- Получить пользователя по ID
SELECT
    id, -- text: Идентификатор пользователя; override: UserId; type: uuid
    username,
    email,
    full_name,
    status,
    preferences,
    billing_address,
    contact,
    phone_numbers,
    tags,
    roles,
    created_at,
    updated_at,
    last_login_at,
    balance,
    loyalty_points,
    discount_percent,
    is_verified,
    is_premium,
    is_deleted,
    external_id,
    date_of_birth
FROM users
WHERE id = $1 AND is_deleted = FALSE
    LIMIT 1;

-- name: GetUserByEmail :one
-- Получить пользователя по email адресу
SELECT *
FROM users
WHERE email = $1
  AND is_deleted = FALSE
    LIMIT 1;

-- name: GetUserByUsername :one
-- Получить пользователя по username
SELECT
    u.id,
    u.username,
    u.email,
    u.full_name,
    u.status::TEXT,
    u.is_premium,
    u.created_at
FROM users u
WHERE u.username = @username
  AND u.is_deleted = FALSE
    LIMIT 1;

-- name: ListActiveUsers :many
-- Получить список всех активных пользователей
SELECT
    id,
    username,
    email,
    full_name,
    status,
    is_premium,
    balance,
    loyalty_points,
    created_at,
    last_login_at
FROM users
WHERE status = 'active'::user_status
  AND is_deleted = FALSE
ORDER BY created_at DESC;

-- name: ListUsersByStatus :many
-- Получить пользователей по статусу с пагинацией
SELECT
    id,
    username,
    email,
    full_name,
    status,
    created_at
FROM users
WHERE status = $1::user_status
  AND is_deleted = FALSE
ORDER BY created_at DESC
    LIMIT $2::INTEGER
OFFSET $3::INTEGER;

-- name: ListPremiumUsers :many
-- Получить всех премиум пользователей
SELECT
    u.id,
    u.username,
    u.email,
    u.full_name,
    u.balance,
    u.loyalty_points,
    u.subscription_duration,
    u.created_at
FROM users u
WHERE u.is_premium = TRUE
  AND u.is_deleted = FALSE
  AND u.status = 'active'::user_status
ORDER BY u.loyalty_points DESC, u.created_at DESC;

-- name: SearchUsersByName :many
-- Полнотекстовый поиск пользователей по имени
SELECT
    id,
    username,
    email,
    full_name,
    status,
    created_at
FROM users
WHERE (full_name ILIKE '%' || @search_query || '%' 
       OR username ILIKE '%' || @search_query || '%')
  AND is_deleted = FALSE
ORDER BY
    CASE
        WHEN username ILIKE @search_query || '%' THEN 1
        WHEN full_name ILIKE @search_query || '%' THEN 2
        ELSE 3
        END,
    created_at DESC
    LIMIT 50;

-- name: GetUsersByTags :many
-- Получить пользователей по тегам
SELECT
    id,
    username,
    email,
    full_name,
    tags,
    status,
    created_at
FROM users
WHERE tags && $1::TEXT[]
  AND is_deleted = FALSE
ORDER BY created_at DESC;

-- name: GetUsersWithHighBalance :many
-- Получить пользователей с балансом выше указанного
SELECT
    id,
    username,
    email,
    full_name,
    balance,
    loyalty_points,
    is_premium
FROM users
WHERE balance >= $1::NUMERIC
  AND is_deleted = FALSE
ORDER BY balance DESC
    LIMIT $2::INTEGER;

-- name: CreateUser :one
-- Создать нового пользователя
INSERT INTO users (
    username,
    email,
    password_hash,
    full_name,
    status,
    preferences,
    phone_numbers,
    tags,
    roles,
    date_of_birth,
    is_verified
) VALUES (
             $1,
             $2::email,
             $3,
             $4,
             COALESCE($5::user_status, 'active'::user_status),
             COALESCE($6::JSONB, '{}'::JSONB),
             $7::phone_number[],
             COALESCE($8::TEXT[], ARRAY[]::TEXT[]),
             COALESCE($9::TEXT[], ARRAY['customer']::TEXT[]),
             $10::DATE,
             COALESCE($11::BOOLEAN, FALSE)
         )
    RETURNING 
    id,
    username,
    email,
    full_name,
    status,
    external_id,
    created_at;

-- name: UpdateUserProfile :exec
-- Обновить профиль пользователя
UPDATE users
SET
    full_name = COALESCE(@full_name, full_name),
    phone_numbers = COALESCE(@phone_numbers::phone_number[], phone_numbers),
    date_of_birth = COALESCE(@date_of_birth::DATE, date_of_birth),
    billing_address = COALESCE(@billing_address::address, billing_address),
    updated_at = CURRENT_TIMESTAMP
WHERE id = @user_id
  AND is_deleted = FALSE;

-- name: UpdateUserPreferences :exec
-- Обновить настройки пользователя
UPDATE users
SET
    preferences = preferences || $2::JSONB,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1;

-- name: UpdateUserStatus :exec
-- Изменить статус пользователя
UPDATE users
SET
    status = $2::user_status,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1
  AND is_deleted = FALSE;

-- name: AddUserTags :exec
-- Добавить теги пользователю
UPDATE users
SET
    tags = array_cat(tags, $2::TEXT[]),
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1
  AND is_deleted = FALSE;

-- name: UpdateUserBalance :exec
-- Обновить баланс пользователя
UPDATE users
SET
    balance = balance + $2::NUMERIC,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1
  AND is_deleted = FALSE;

-- name: UpdateUserLoyaltyPoints :exec
-- Обновить баллы лояльности
UPDATE users
SET
    loyalty_points = loyalty_points + @points::INTEGER,
    updated_at = CURRENT_TIMESTAMP
WHERE id = @user_id
  AND is_deleted = FALSE;

-- name: UpdateUserLastLogin :exec
-- Обновить время последнего входа
UPDATE users
SET
    last_login_at = CURRENT_TIMESTAMP,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1;

-- name: VerifyUserEmail :exec
-- Подтвердить email пользователя
UPDATE users
SET
    is_verified = TRUE,
    updated_at = CURRENT_TIMESTAMP
WHERE id = @user_id
  AND is_deleted = FALSE;

-- name: UpgradeUserToPremium :exec
-- Повысить пользователя до премиум
UPDATE users
SET
    is_premium = TRUE,
    subscription_duration = $2::INTERVAL,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1
  AND is_deleted = FALSE;

-- name: SoftDeleteUser :exec
-- Мягкое удаление пользователя
UPDATE users
SET
    is_deleted = TRUE,
    deleted_at = CURRENT_TIMESTAMP,
    status = 'deleted'::user_status,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1;

-- name: DeleteUser :exec
-- Полное удаление пользователя (физическое)
DELETE FROM users
WHERE id = $1;

-- name: GetUserStatistics :one
-- Получить статистику пользователя
SELECT
    u.id,
    u.username,
    u.email,
    u.balance,
    u.loyalty_points,
    COUNT(DISTINCT o.id) AS total_orders,
    COALESCE(SUM(o.total), 0)::NUMERIC AS total_spent,
    COALESCE(AVG(o.total), 0)::NUMERIC AS average_order_value,
    MAX(o.created_at) AS last_order_date
FROM users u
         LEFT JOIN orders o ON u.id = o.user_id
    AND o.cancelled_at IS NULL
WHERE u.id = $1
  AND u.is_deleted = FALSE
GROUP BY u.id, u.username, u.email, u.balance, u.loyalty_points;

-- name: CountUsersByStatus :one
-- Подсчитать количество пользователей по статусу
SELECT COUNT(*) AS user_count
FROM users
WHERE status = $1::user_status
  AND is_deleted = FALSE;

-- ============================================================
-- CATEGORIES - Запросы для работы с категориями
-- ============================================================

-- name: GetCategoryById :one
-- Получить категорию по ID
SELECT
    id,
    name,
    slug,
    description,
    parent_id,
    level,
    path,
    metadata,
    sort_order,
    is_active,
    created_at,
    updated_at
FROM categories
WHERE id = $1
  AND is_active = TRUE
    LIMIT 1;

-- name: GetCategoryBySlug :one
-- Получить категорию по slug
SELECT *
FROM categories
WHERE slug = @slug
  AND is_active = TRUE
    LIMIT 1;

-- name: ListRootCategories :many
-- Получить корневые категории (без родителя)
SELECT
    id,
    name,
    slug,
    description,
    sort_order,
    created_at
FROM categories
WHERE parent_id IS NULL
  AND is_active = TRUE
ORDER BY sort_order ASC, name ASC;

-- name: ListChildCategories :many
-- Получить дочерние категории
SELECT
    id,
    name,
    slug,
    description,
    level,
    sort_order,
    created_at
FROM categories
WHERE parent_id = $1
  AND is_active = TRUE
ORDER BY sort_order ASC, name ASC;

-- name: ListAllCategories :many
-- Получить все активные категории
SELECT
    id,
    name,
    slug,
    parent_id,
    level,
    path,
    sort_order,
    is_active
FROM categories
WHERE is_active = TRUE
ORDER BY level ASC, sort_order ASC, name ASC;

-- name: SearchCategories :many
-- Полнотекстовый поиск по категориям
SELECT
    c.id,
    c.name,
    c.slug,
    c.description,
    c.level,
    ts_rank(c.search_vector, websearch_to_tsquery('english', @query)) AS rank
FROM categories c
WHERE c.search_vector @@ websearch_to_tsquery('english', @query)
  AND c.is_active = TRUE
ORDER BY rank DESC, c.name ASC
    LIMIT 20;

-- name: GetCategoryPath :one
-- Получить полный путь категории
SELECT get_category_path($1::INTEGER) AS full_path;

-- name: GetCategoryWithAncestors :many
-- Получить категорию со всеми предками
SELECT
    c.id,
    c.name,
    c.slug,
    c.level,
    c.parent_id
FROM categories c
WHERE c.id = ANY(
    (SELECT path FROM categories WHERE id = $1)
)
  AND c.is_active = TRUE
ORDER BY c.level ASC;

-- name: GetCategoryWithDescendants :many
-- Получить категорию со всеми потомками
SELECT *
FROM get_child_categories($1::INTEGER);

-- name: CreateCategory :one
-- Создать новую категорию
INSERT INTO categories (
    name,
    slug,
    description,
    parent_id,
    sort_order,
    metadata
) VALUES (
             $1,
             $2,
             $3,
             $4::INTEGER,
             COALESCE($5::INTEGER, 0),
             COALESCE($6::JSONB, '{}'::JSONB)
         )
    RETURNING 
    id,
    name,
    slug,
    level,
    path,
    created_at;

-- name: UpdateCategory :exec
-- Обновить категорию
UPDATE categories
SET
    name = COALESCE(@name, name),
    description = COALESCE(@description, description),
    sort_order = COALESCE(@sort_order::INTEGER, sort_order),
    metadata = COALESCE(@metadata::JSONB, metadata),
    updated_at = CURRENT_TIMESTAMP
WHERE id = @category_id
  AND is_active = TRUE;

-- name: MoveCategoryToParent :exec
-- Переместить категорию к другому родителю
UPDATE categories
SET
    parent_id = $2::INTEGER,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1
  AND is_active = TRUE;

-- name: UpdateCategoryStatus :exec
-- Изменить статус категории
UPDATE categories
SET
    is_active = $2::BOOLEAN,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1;

-- name: DeleteCategory :exec
-- Удалить категорию (каскадное удаление дочерних)
DELETE FROM categories
WHERE id = $1;

-- name: GetCategoriesWithItemCount :many
-- Получить категории с количеством товаров
SELECT
    c.id,
    c.name,
    c.slug,
    c.level,
    COUNT(oi.id) AS items_count,
    COALESCE(SUM(oi.total_price), 0)::NUMERIC AS total_revenue
FROM categories c
         LEFT JOIN order_items oi ON c.id = oi.category_id
WHERE c.is_active = TRUE
GROUP BY c.id, c.name, c.slug, c.level
ORDER BY total_revenue DESC
    LIMIT $1::INTEGER;

-- ============================================================
-- ORDERS - Запросы для работы с заказами
-- ============================================================

-- name: GetOrderById :one
-- Получить заказ по ID
SELECT
    o.id,
    o.user_id,
    o.order_number,
    o.status,
    o.priority,
    o.subtotal,
    o.tax,
    o.shipping_cost,
    o.discount,
    o.total,
    o.shipping_address,
    o.billing_address,
    o.payment_method,
    o.payment_status,
    o.status_history,
    o.created_at,
    o.updated_at,
    o.confirmed_at,
    o.shipped_at,
    o.delivered_at,
    o.delivery_window,
    o.notes,
    o.external_id
FROM orders o
WHERE o.id = $1
    LIMIT 1;

-- name: GetOrderByNumber :one
-- Получить заказ по номеру
SELECT *
FROM orders
WHERE order_number = @order_number
    LIMIT 1;

-- name: GetOrderByExternalId :one
-- Получить заказ по внешнему ID
SELECT
    id,
    order_number,
    status,
    total,
    created_at
FROM orders
WHERE external_id = $1::UUID
LIMIT 1;

-- name: ListUserOrders :many
-- Получить все заказы пользователя
SELECT
    o.id,
    o.order_number,
    o.status,
    o.priority,
    o.total,
    o.payment_status,
    o.created_at,
    o.confirmed_at,
    o.delivered_at
FROM orders o
WHERE o.user_id = $1
  AND o.cancelled_at IS NULL
ORDER BY o.created_at DESC
    LIMIT $2::INTEGER
OFFSET $3::INTEGER;

-- name: ListOrdersByStatus :many
-- Получить заказы по статусу
SELECT
    o.id,
    o.order_number,
    o.user_id,
    o.status,
    o.total,
    o.created_at,
    u.username,
    u.email
FROM orders o
         INNER JOIN users u ON o.user_id = u.id
WHERE o.status = $1::order_status
  AND o.cancelled_at IS NULL
ORDER BY o.created_at DESC;

-- name: ListOrdersByPriority :many
-- Получить заказы по приоритету
SELECT
    o.id,
    o.order_number,
    o.user_id,
    o.status,
    o.priority,
    o.total,
    o.created_at
FROM orders o
WHERE o.priority = @priority::priority_level
  AND o.status NOT IN ('delivered'::order_status, 'cancelled'::order_status)
ORDER BY o.created_at ASC;

-- name: ListPendingOrders :many
-- Получить все ожидающие обработки заказы
SELECT
    o.id,
    o.order_number,
    o.user_id,
    o.priority,
    o.total,
    o.created_at,
    EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - o.created_at))::INTEGER AS age_seconds
FROM orders o
WHERE o.status = 'pending'::order_status
  AND o.payment_status = 'paid'
  AND o.cancelled_at IS NULL
ORDER BY o.priority DESC, o.created_at ASC;

-- name: ListOrdersInDeliveryWindow :many
-- Получить заказы с окном доставки в указанный период
SELECT
    o.id,
    o.order_number,
    o.user_id,
    o.status,
    o.delivery_window,
    o.shipping_address,
    o.total
FROM orders o
WHERE o.delivery_window && tstzrange($1::TIMESTAMPTZ, $2::TIMESTAMPTZ)
  AND o.status IN ('processing'::order_status, 'shipped'::order_status)
ORDER BY lower(o.delivery_window) ASC;

-- name: SearchOrdersByAddress :many
-- Поиск заказов по адресу доставки
SELECT
    o.id,
    o.order_number,
    o.user_id,
    o.status,
    o.shipping_address,
    o.total,
    o.created_at
FROM orders o
WHERE o.shipping_address @> $1::JSONB
  AND o.cancelled_at IS NULL
ORDER BY o.created_at DESC;

-- name: GetOrdersWithPaymentMethod :many
-- Получить заказы по способу оплаты
SELECT
    o.id,
    o.order_number,
    o.user_id,
    o.status,
    o.payment_method,
    o.payment_status,
    o.total,
    o.created_at
FROM orders o
WHERE o.payment_method = $1::payment_method
  AND o.created_at >= $2::TIMESTAMPTZ
  AND o.cancelled_at IS NULL
ORDER BY o.created_at DESC;

-- name: CreateOrder :one
-- Создать новый заказ
INSERT INTO orders (
    user_id,
    order_number,
    status,
    priority,
    subtotal,
    tax,
    shipping_cost,
    discount,
    total,
    shipping_address,
    billing_address,
    payment_method,
    payment_status,
    notes,
    customer_notes
) VALUES (
             $1,
             $2,
             COALESCE($3::order_status, 'pending'::order_status),
             COALESCE($4::priority_level, 'medium'::priority_level),
             $5::NUMERIC,
             COALESCE($6::NUMERIC, 0),
             COALESCE($7::NUMERIC, 0),
             COALESCE($8::NUMERIC, 0),
             $9::NUMERIC,
             $10::JSONB,
             $11::JSONB,
             $12::payment_method,
             COALESCE($13, 'pending'),
             $14,
             $15
         )
    RETURNING 
    id,
    order_number,
    status,
    external_id,
    created_at;

-- name: UpdateOrderStatus :exec
-- Обновить статус заказа
UPDATE orders
SET
    status = $2::order_status,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1;

-- name: UpdateOrderPaymentStatus :exec
-- Обновить статус оплаты
UPDATE orders
SET
    payment_status = @payment_status,
    payment_details = COALESCE(@payment_details::JSONB, payment_details),
    updated_at = CURRENT_TIMESTAMP
WHERE id = @order_id;

-- name: ConfirmOrder :exec
-- Подтвердить заказ
UPDATE orders
SET
    status = 'processing'::order_status,
    confirmed_at = CURRENT_TIMESTAMP,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1
  AND status = 'pending'::order_status;

-- name: ShipOrder :exec
-- Отметить заказ как отправленный
UPDATE orders
SET
    status = 'shipped'::order_status,
    shipped_at = CURRENT_TIMESTAMP,
    delivery_window = tstzrange(
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP + @estimated_delivery::INTERVAL
    ),
    updated_at = CURRENT_TIMESTAMP
WHERE id = @order_id
  AND status = 'processing'::order_status;

-- name: DeliverOrder :exec
-- Отметить заказ как доставленный
UPDATE orders
SET
    status = 'delivered'::order_status,
    delivered_at = CURRENT_TIMESTAMP,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1
  AND status = 'shipped'::order_status;

-- name: CancelOrder :exec
-- Отменить заказ
UPDATE orders
SET
    status = 'cancelled'::order_status,
    cancelled_at = CURRENT_TIMESTAMP,
    internal_notes = COALESCE(internal_notes || E'\n', '') || 'Cancelled: ' || $2,
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1
  AND status NOT IN ('delivered'::order_status, 'cancelled'::order_status);

-- name: UpdateOrderDeliveryWindow :exec
-- Обновить окно доставки
UPDATE orders
SET
    delivery_window = tstzrange($2::TIMESTAMPTZ, $3::TIMESTAMPTZ),
    updated_at = CURRENT_TIMESTAMP
WHERE id = $1;

-- name: AddOrderNote :exec
-- Добавить заметку к заказу
UPDATE orders
SET
    internal_notes = COALESCE(internal_notes || E'\n', '') ||
                     '[' || CURRENT_TIMESTAMP::TEXT || '] ' || @note,
    updated_at = CURRENT_TIMESTAMP
WHERE id = @order_id;

-- name: DeleteOrder :exec
-- Удалить заказ (физически)
DELETE FROM orders
WHERE id = $1;

-- name: GetOrderWithItems :many
-- Получить заказ со всеми позициями
SELECT
    o.id AS order_id,
    o.order_number,
    o.status,
    o.total AS order_total,
    oi.id AS item_id,
    oi.product_name,
    oi.quantity,
    oi.unit_price,
    oi.total_price,
    oi.attributes,
    c.name AS category_name
FROM orders o
         INNER JOIN order_items oi ON o.id = oi.order_id
         INNER JOIN categories c ON oi.category_id = c.id
WHERE o.id = $1
ORDER BY oi.id;

-- name: GetOrderTotalsByStatus :many
-- Получить суммы заказов по статусам
SELECT
    status,
    COUNT(*) AS order_count,
    SUM(total)::NUMERIC AS total_amount,
    AVG(total)::NUMERIC AS average_amount,
    MIN(created_at) AS oldest_order,
    MAX(created_at) AS newest_order
FROM orders
WHERE created_at >= $1::TIMESTAMPTZ
  AND cancelled_at IS NULL
GROUP BY status
ORDER BY order_count DESC;

-- name: GetDailyOrderStatistics :many
-- Получить статистику заказов по дням
SELECT
    DATE(created_at) AS order_date,
    COUNT(*) AS order_count,
    SUM(total)::NUMERIC AS total_revenue,
    AVG(total)::NUMERIC AS average_order_value,
    COUNT(DISTINCT user_id) AS unique_customers,
    COUNT(*) FILTER (WHERE status = 'delivered'::order_status) AS delivered_count,
    COUNT(*) FILTER (WHERE status = 'cancelled'::order_status) AS cancelled_count
FROM orders
WHERE created_at >= $1::DATE
  AND created_at < $2::DATE
GROUP BY DATE(created_at)
ORDER BY order_date DESC;

-- ============================================================
-- ORDER ITEMS - Запросы для работы с позициями заказов
-- ============================================================

-- name: GetOrderItemById :one
-- Получить позицию заказа по ID
SELECT
    oi.id,
    oi.order_id,
    oi.category_id,
    oi.product_name,
    oi.product_description,
    oi.sku,
    oi.quantity,
    oi.unit_price,
    oi.discount_amount,
    oi.total_price,
    oi.attributes,
    c.name AS category_name
FROM order_items oi
         INNER JOIN categories c ON oi.category_id = c.id
WHERE oi.id = $1
    LIMIT 1;

-- name: ListOrderItems :many
-- Получить все позиции заказа
SELECT
    id,
    product_name,
    sku,
    quantity,
    unit_price,
    discount_amount,
    total_price,
    attributes
FROM order_items
WHERE order_id = $1
ORDER BY id;

-- name: GetOrderItemsByCategory :many
-- Получить позиции по категории
SELECT
    oi.id,
    oi.order_id,
    oi.product_name,
    oi.quantity,
    oi.total_price,
    o.order_number,
    o.status,
    o.created_at
FROM order_items oi
         INNER JOIN orders o ON oi.order_id = o.id
WHERE oi.category_id = $1
  AND o.cancelled_at IS NULL
ORDER BY o.created_at DESC
    LIMIT $2::INTEGER;

-- name: GetOrderItemsBySku :many
-- Получить позиции по артикулу
SELECT
    oi.id,
    oi.order_id,
    oi.product_name,
    oi.quantity,
    oi.unit_price,
    oi.total_price,
    o.order_number,
    o.created_at
FROM order_items oi
         INNER JOIN orders o ON oi.order_id = o.id
WHERE oi.sku = @sku
  AND o.cancelled_at IS NULL
ORDER BY o.created_at DESC;

-- name: SearchOrderItemsByAttributes :many
-- Поиск позиций по атрибутам
SELECT
    oi.id,
    oi.order_id,
    oi.product_name,
    oi.quantity,
    oi.attributes,
    oi.total_price
FROM order_items oi
WHERE oi.attributes @> $1::JSONB
ORDER BY oi.id DESC
    LIMIT 100;

-- name: CreateOrderItem :one
-- Создать позицию заказа
INSERT INTO order_items (
    order_id,
    category_id,
    product_name,
    product_description,
    sku,
    barcode,
    quantity,
    unit_price,
    discount_percent,
    discount_amount,
    tax_percent,
    tax_amount,
    total_price,
    attributes,
    weight_grams,
    dimensions
) VALUES (
             $1,
             $2,
             $3,
             $4,
             $5,
             $6,
             $7::INTEGER,
             $8::NUMERIC,
             COALESCE($9::NUMERIC, 0),
             COALESCE($10::NUMERIC, 0),
             COALESCE($11::NUMERIC, 0),
             COALESCE($12::NUMERIC, 0),
             $13::NUMERIC,
             COALESCE($14::JSONB, '{}'::JSONB),
             $15::INTEGER,
             $16::JSONB
         )
    RETURNING 
    id,
    order_id,
    product_name,
    quantity,
    total_price;

-- name: UpdateOrderItemQuantity :exec
-- Обновить количество в позиции
UPDATE order_items
SET
    quantity = @quantity::INTEGER,
    total_price = unit_price * @quantity::INTEGER - discount_amount + tax_amount
WHERE id = @item_id;

-- name: UpdateOrderItemPrice :exec
-- Обновить цену позиции
UPDATE order_items
SET
    unit_price = $2::NUMERIC,
    total_price = $2::NUMERIC * quantity - discount_amount + tax_amount
WHERE id = $1;

-- name: DeleteOrderItem :exec
-- Удалить позицию из заказа
DELETE FROM order_items
WHERE id = $1;

-- name: GetTopSellingProducts :many
-- Получить самые продаваемые товары
SELECT
    oi.product_name,
    oi.sku,
    COUNT(*) AS times_ordered,
    SUM(oi.quantity) AS total_quantity_sold,
    SUM(oi.total_price)::NUMERIC AS total_revenue,
    AVG(oi.unit_price)::NUMERIC AS average_price
FROM order_items oi
         INNER JOIN orders o ON oi.order_id = o.id
WHERE o.status = 'delivered'::order_status
  AND o.created_at >= $1::TIMESTAMPTZ
GROUP BY oi.product_name, oi.sku
ORDER BY total_revenue DESC
    LIMIT $2::INTEGER;

-- name: GetCategoryRevenue :many
-- Получить выручку по категориям
SELECT
    c.id,
    c.name,
    c.slug,
    COUNT(DISTINCT oi.id) AS items_sold,
    SUM(oi.quantity) AS total_quantity,
    SUM(oi.total_price)::NUMERIC AS total_revenue,
    AVG(oi.unit_price)::NUMERIC AS average_unit_price
FROM categories c
         INNER JOIN order_items oi ON c.id = oi.category_id
         INNER JOIN orders o ON oi.order_id = o.id
WHERE o.status IN ('delivered'::order_status, 'shipped'::order_status)
  AND o.created_at >= $1::TIMESTAMPTZ
GROUP BY c.id, c.name, c.slug
ORDER BY total_revenue DESC;

-- ============================================================
-- AUDIT LOGS - Запросы для работы с логами аудита
-- ============================================================

-- name: CreateAuditLog :exec
-- Создать запись в логе аудита
INSERT INTO audit_logs (
    user_id,
    action,
    entity_type,
    entity_id,
    old_values,
    new_values,
    ip_address,
    user_agent,
    created_at
) VALUES (
             $1,
             $2,
             $3,
             $4,
             $5::JSONB,
             $6::JSONB,
             $7::INET,
             $8,
             COALESCE($9::TIMESTAMPTZ, CURRENT_TIMESTAMP)
         );

-- name: GetAuditLogsByUser :many
-- Получить логи пользователя
SELECT
    id,
    action,
    entity_type,
    entity_id,
    old_values,
    new_values,
    ip_address,
    created_at
FROM audit_logs
WHERE user_id = $1
  AND created_at >= $2::TIMESTAMPTZ
ORDER BY created_at DESC
    LIMIT $3::INTEGER;

-- name: GetAuditLogsByEntity :many
-- Получить историю изменений сущности
SELECT
    al.id,
    al.user_id,
    al.action,
    al.old_values,
    al.new_values,
    al.ip_address,
    al.created_at,
    u.username
FROM audit_logs al
         LEFT JOIN users u ON al.user_id = u.id
WHERE al.entity_type = @entity_type
  AND al.entity_id = @entity_id
ORDER BY al.created_at DESC;

-- name: GetRecentAuditLogs :many
-- Получить последние записи аудита
SELECT
    al.id,
    al.user_id,
    al.action,
    al.entity_type,
    al.entity_id,
    al.created_at,
    u.username,
    u.email
FROM audit_logs al
         LEFT JOIN users u ON al.user_id = u.id
WHERE al.created_at >= $1::TIMESTAMPTZ
ORDER BY al.created_at DESC
    LIMIT 100;

-- ============================================================
-- COMPLEX QUERIES - Сложные аналитические запросы
-- ============================================================

-- name: GetUserOrderSummary :one
-- Получить сводку по заказам пользователя
SELECT
    u.id,
    u.username,
    u.email,
    u.status,
    u.is_premium,
    u.balance,
    u.loyalty_points,
    COUNT(DISTINCT o.id) FILTER (WHERE o.cancelled_at IS NULL) AS total_orders,
    COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'delivered'::order_status) AS completed_orders,
    COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'pending'::order_status) AS pending_orders,
    COALESCE(SUM(o.total) FILTER (WHERE o.cancelled_at IS NULL), 0)::NUMERIC AS lifetime_value,
    COALESCE(AVG(o.total) FILTER (WHERE o.cancelled_at IS NULL), 0)::NUMERIC AS average_order_value,
    MAX(o.created_at) AS last_order_date,
    MIN(o.created_at) AS first_order_date,
    ARRAY_AGG(DISTINCT o.payment_method) FILTER (WHERE o.payment_method IS NOT NULL) AS used_payment_methods
FROM users u
         LEFT JOIN orders o ON u.id = o.user_id
WHERE u.id = $1
GROUP BY u.id, u.username, u.email, u.status, u.is_premium, u.balance, u.loyalty_points;

-- name: GetOrderFulfillmentReport :many
-- Отчет по выполнению заказов
SELECT
    o.status,
    o.priority,
    COUNT(*) AS order_count,
    AVG(EXTRACT(EPOCH FROM (o.delivered_at - o.created_at)))::INTEGER AS avg_fulfillment_seconds,
    AVG(EXTRACT(EPOCH FROM (o.shipped_at - o.confirmed_at)))::INTEGER AS avg_processing_seconds,
    AVG(o.total)::NUMERIC AS avg_order_value,
    SUM(o.total)::NUMERIC AS total_revenue
FROM orders o
WHERE o.created_at >= $1::TIMESTAMPTZ
  AND o.created_at < $2::TIMESTAMPTZ
  AND o.cancelled_at IS NULL
GROUP BY o.status, o.priority
ORDER BY o.status, o.priority;

-- name: GetCustomerSegmentation :many
-- Сегментация клиентов
WITH customer_metrics AS (
    SELECT
        u.id,
        u.username,
        u.email,
        u.is_premium,
        COUNT(o.id) AS order_count,
        COALESCE(SUM(o.total), 0) AS total_spent,
        MAX(o.created_at) AS last_order_date,
        EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - MAX(o.created_at)))/86400 AS days_since_last_order
    FROM users u
             LEFT JOIN orders o ON u.id = o.user_id
        AND o.cancelled_at IS NULL
    WHERE u.is_deleted = FALSE
    GROUP BY u.id, u.username, u.email, u.is_premium
)
SELECT
    id,
    username,
    email,
    is_premium,
    order_count,
    total_spent::NUMERIC,
    last_order_date,
    days_since_last_order::INTEGER,
    CASE
        WHEN order_count = 0 THEN 'new'
        WHEN days_since_last_order > 90 THEN 'dormant'
        WHEN order_count >= 10 AND total_spent >= 1000 THEN 'vip'
        WHEN order_count >= 5 THEN 'regular'
        ELSE 'occasional'
        END AS segment
FROM customer_metrics
ORDER BY total_spent DESC;

-- name: GetProductPerformanceByCategory :many
-- Производительность товаров по категориям
SELECT
    c.id AS category_id,
    c.name AS category_name,
    c.level,
    COUNT(DISTINCT oi.id) AS unique_products_sold,
    SUM(oi.quantity) AS total_items_sold,
    SUM(oi.total_price)::NUMERIC AS total_revenue,
    AVG(oi.unit_price)::NUMERIC AS avg_price,
    STDDEV(oi.unit_price)::NUMERIC AS price_stddev,
    COUNT(DISTINCT o.user_id) AS unique_customers,
    (SUM(oi.total_price) / NULLIF(SUM(oi.quantity), 0))::NUMERIC AS revenue_per_item
FROM categories c
         INNER JOIN order_items oi ON c.id = oi.category_id
         INNER JOIN orders o ON oi.order_id = o.id
WHERE o.status IN ('delivered'::order_status, 'shipped'::order_status)
  AND o.created_at >= $1::TIMESTAMPTZ
GROUP BY c.id, c.name, c.level
HAVING SUM(oi.quantity) > 0
ORDER BY total_revenue DESC;

-- name: GetCohortAnalysis :many
-- Когортный анализ пользователей
WITH user_cohorts AS (
    SELECT
        u.id,
        DATE_TRUNC('month', u.created_at) AS cohort_month,
        DATE_TRUNC('month', o.created_at) AS order_month,
        o.total
    FROM users u
             LEFT JOIN orders o ON u.id = o.user_id
        AND o.cancelled_at IS NULL
    WHERE u.created_at >= $1::TIMESTAMPTZ
    )
SELECT
    cohort_month,
    order_month,
    COUNT(DISTINCT id) AS active_users,
    COUNT(total) AS orders_placed,
    SUM(total)::NUMERIC AS revenue,
    EXTRACT(MONTH FROM AGE(order_month, cohort_month))::INTEGER AS months_since_registration
FROM user_cohorts
WHERE order_month IS NOT NULL
GROUP BY cohort_month, order_month
ORDER BY cohort_month, order_month;

-- name: GetAbandonedCarts :many
-- Получить брошенные корзины (незавершенные заказы)
SELECT
    o.id,
    o.order_number,
    o.user_id,
    u.username,
    u.email,
    o.total,
    o.created_at,
    EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - o.created_at))/3600 AS hours_abandoned,
    COUNT(oi.id) AS items_count
FROM orders o
         INNER JOIN users u ON o.user_id = u.id
         LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.status = 'pending'::order_status
  AND o.payment_status = 'pending'
  AND o.created_at < CURRENT_TIMESTAMP - INTERVAL '1 hour'
  AND o.created_at >= $1::TIMESTAMPTZ
  AND o.cancelled_at IS NULL
GROUP BY o.id, o.order_number, o.user_id, u.username, u.email, o.total, o.created_at
ORDER BY o.created_at DESC;

-- name: GetRevenueByPaymentMethod :many
-- Анализ выручки по способам оплаты
SELECT
    payment_method,
    payment_status,
    COUNT(*) AS transaction_count,
    SUM(total)::NUMERIC AS total_revenue,
    AVG(total)::NUMERIC AS average_transaction,
    MIN(total)::NUMERIC AS min_transaction,
    MAX(total)::NUMERIC AS max_transaction,
    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY total)::NUMERIC AS median_transaction
FROM orders
WHERE created_at >= $1::TIMESTAMPTZ
  AND cancelled_at IS NULL
GROUP BY payment_method, payment_status
ORDER BY total_revenue DESC;

-- ============================================================
-- MATERIALIZED VIEW QUERIES - Работа с материализованными представлениями
-- ============================================================

-- name: RefreshCategoryStatistics :exec
-- Обновить материализованное представление статистики категорий
REFRESH MATERIALIZED VIEW CONCURRENTLY category_statistics;

-- name: GetCategoryStatistics :many
-- Получить статистику по категориям из materialized view
SELECT
    id,
    name,
    slug,
    level,
    total_items_sold,
    total_quantity,
    total_revenue,
    avg_price,
    unique_customers,
    last_order_date
FROM category_statistics
WHERE total_revenue > $1::NUMERIC
ORDER BY total_revenue DESC
    LIMIT $2::INTEGER;

-- ============================================================
-- UTILITY QUERIES - Вспомогательные запросы
-- ============================================================

-- name: GetDatabaseStatistics :one
-- Получить общую статистику базы данных
SELECT
    (SELECT COUNT(*) FROM users WHERE is_deleted = FALSE) AS total_users,
    (SELECT COUNT(*) FROM users WHERE status = 'active'::user_status AND is_deleted = FALSE) AS active_users,
    (SELECT COUNT(*) FROM users WHERE is_premium = TRUE AND is_deleted = FALSE) AS premium_users,
    (SELECT COUNT(*) FROM categories WHERE is_active = TRUE) AS total_categories,
    (SELECT COUNT(*) FROM orders WHERE cancelled_at IS NULL) AS total_orders,
    (SELECT COUNT(*) FROM orders WHERE status = 'pending'::order_status) AS pending_orders,
    (SELECT COALESCE(SUM(total), 0)::NUMERIC FROM orders WHERE cancelled_at IS NULL) AS total_revenue,
    (SELECT COUNT(*) FROM order_items) AS total_items_sold;

-- name: CheckUserExists :one
-- Проверить существование пользователя
SELECT EXISTS(
    SELECT 1 FROM users
    WHERE email = $1::email
        AND is_deleted = FALSE
) AS exists;

-- name: CheckOrderExists :one
-- Проверить существование заказа
SELECT EXISTS(
    SELECT 1 FROM orders
    WHERE order_number = @order_number
) AS exists;