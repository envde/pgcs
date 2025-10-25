-- name: GetUserById :one
-- Получить пользователя по ID
SELECT id,
       username,
       email
FROM users
WHERE id = $1;

-- name: GetUserByEmail :one
-- Получить пользователя по email (использует DOMAIN тип)
SELECT id,
       username,
       email
FROM users
WHERE email = $1;
