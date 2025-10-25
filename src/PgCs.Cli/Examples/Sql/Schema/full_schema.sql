-- Full database schema for Queries.sql examples

-- User status enum
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'deleted', 'banned');

-- Order status enum
CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');

-- Users table
CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    username VARCHAR(255) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(500),
    status user_status NOT NULL DEFAULT 'active',
    preferences JSONB DEFAULT '{}',
    phone_numbers TEXT[],
    tags TEXT[],
    balance NUMERIC(10,2) DEFAULT 0.00,
    loyalty_points INTEGER DEFAULT 0,
    is_verified BOOLEAN DEFAULT FALSE,
    is_premium BOOLEAN DEFAULT FALSE,
    external_id UUID DEFAULT gen_random_uuid(),
    date_of_birth DATE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_login_at TIMESTAMP WITH TIME ZONE
);

-- Categories table
CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL UNIQUE,
    description TEXT,
    parent_id INTEGER REFERENCES categories(id),
    search_vector TSVECTOR,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Orders table
CREATE TABLE orders (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id),
    order_number VARCHAR(50) NOT NULL UNIQUE,
    status order_status NOT NULL DEFAULT 'pending',
    subtotal NUMERIC(10,2) NOT NULL,
    tax NUMERIC(10,2) DEFAULT 0.00,
    shipping_cost NUMERIC(10,2) DEFAULT 0.00,
    discount NUMERIC(10,2) DEFAULT 0.00,
    total NUMERIC(10,2) NOT NULL,
    shipping_address JSONB,
    billing_address JSONB,
    payment_method VARCHAR(50),
    payment_details JSONB,
    status_history JSONB DEFAULT '[]',
    delivery_window TSTZRANGE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    shipped_at TIMESTAMP WITH TIME ZONE,
    delivered_at TIMESTAMP WITH TIME ZONE
);

-- Order items table
CREATE TABLE order_items (
    id BIGSERIAL PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES orders(id),
    category_id INTEGER REFERENCES categories(id),
    product_name VARCHAR(255) NOT NULL,
    sku VARCHAR(100),
    quantity INTEGER NOT NULL,
    unit_price NUMERIC(10,2) NOT NULL,
    discount_percent NUMERIC(5,2) DEFAULT 0.00,
    total_price NUMERIC(10,2) NOT NULL,
    attributes JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- View for active users with order statistics
CREATE VIEW active_users_with_orders AS
SELECT 
    u.id,
    u.username,
    u.email,
    u.is_premium,
    COUNT(o.id) as total_orders,
    COALESCE(SUM(o.total), 0) as total_spent
FROM users u
LEFT JOIN orders o ON u.id = o.user_id
WHERE u.status = 'active'
GROUP BY u.id, u.username, u.email, u.is_premium;

-- Materialized view for category statistics
CREATE MATERIALIZED VIEW category_statistics AS
SELECT 
    c.id,
    c.name,
    SUM(oi.quantity) as total_items_sold,
    SUM(oi.total_price) as total_revenue,
    AVG(oi.unit_price) as avg_price
FROM categories c
LEFT JOIN order_items oi ON c.id = oi.category_id
GROUP BY c.id, c.name;

-- Function to get category path
CREATE OR REPLACE FUNCTION get_category_path(category_id INTEGER)
RETURNS TEXT AS $$
DECLARE
    path TEXT := '';
    current_id INTEGER := category_id;
    current_name TEXT;
    parent INTEGER;
BEGIN
    WHILE current_id IS NOT NULL LOOP
        SELECT name, parent_id INTO current_name, parent
        FROM categories
        WHERE id = current_id;
        
        IF path = '' THEN
            path := current_name;
        ELSE
            path := current_name || ' > ' || path;
        END IF;
        
        current_id := parent;
    END LOOP;
    
    RETURN path;
END;
$$ LANGUAGE plpgsql;
