-- Пример схемы БЕЗ точек с запятой
-- Statements разделяются пустой строкой

CREATE TYPE user_role AS ENUM ('admin', 'user', 'guest')

CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL,
    role user_role NOT NULL DEFAULT 'user',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
)

CREATE TABLE posts (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id),
    title VARCHAR(255) NOT NULL,
    content TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
)

CREATE INDEX idx_posts_user_id ON posts(user_id)

CREATE INDEX idx_posts_created_at ON posts(created_at DESC)

CREATE VIEW active_users AS
SELECT id, username, email
FROM users
WHERE role != 'guest'

CREATE FUNCTION count_user_posts(user_id_param BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    post_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO post_count
    FROM posts
    WHERE user_id = user_id_param;
    
    RETURN post_count;
END;
$$
