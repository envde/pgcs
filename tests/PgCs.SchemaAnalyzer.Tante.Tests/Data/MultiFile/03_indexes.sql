-- Файл 3: Индексы

-- Индексы для users
CREATE INDEX idx_users_email ON users (email);
CREATE INDEX idx_users_username ON users (username);
CREATE INDEX idx_users_status ON users (status) WHERE NOT is_deleted;
CREATE INDEX idx_users_created_at ON users (created_at DESC);
CREATE INDEX idx_users_external_id ON users (external_id);

-- Индексы для categories
CREATE INDEX idx_categories_parent ON categories (parent_id);
CREATE INDEX idx_categories_slug ON categories (slug);
CREATE INDEX idx_categories_path ON categories USING GIST (path);
CREATE INDEX idx_categories_search ON categories USING GIN (search_vector);

-- Индексы для orders
CREATE INDEX idx_orders_user ON orders (user_id);
CREATE INDEX idx_orders_status ON orders (status);
CREATE INDEX idx_orders_created_at ON orders (created_at DESC);
CREATE INDEX idx_orders_order_number ON orders (order_number);

-- Индексы для order_items
CREATE INDEX idx_order_items_order ON order_items (order_id);
CREATE INDEX idx_order_items_category ON order_items (category_id);

-- Комментарии на индексы
COMMENT ON INDEX idx_users_email IS 'Индекс для быстрого поиска по email';
COMMENT ON INDEX idx_users_status IS 'Индекс для фильтрации по статусу';
COMMENT ON INDEX idx_categories_search IS 'Полнотекстовый поиск по категориям';
COMMENT ON INDEX idx_orders_user IS 'Индекс для выборки заказов пользователя';

