-- Test queries with schema integration

-- name: GetUserById :one
SELECT id, username, email, full_name, status
FROM users
WHERE id = $1;

-- name: ListUsers :many
SELECT id, username, email, status, balance
FROM users
WHERE status = $1
ORDER BY created_at DESC;

-- name: CreateUser :one
INSERT INTO users (username, email, password_hash, full_name, status)
VALUES ($1, $2, $3, $4, $5)
RETURNING id, external_id, created_at;

-- name: GetOrderWithUser :one
SELECT 
    o.id,
    o.order_number,
    o.status,
    o.total,
    u.username,
    u.email
FROM orders o
INNER JOIN users u ON o.user_id = u.id
WHERE o.id = $1;
