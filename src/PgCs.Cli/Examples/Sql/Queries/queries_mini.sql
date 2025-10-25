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