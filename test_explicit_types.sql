-- name: TestWithExplicitTypes :one
SELECT 
    id::bigint as id,
    username::text as username, 
    email::text as email,
    status::text as status,
    balance::numeric as balance
FROM users
WHERE id = $1::bigint;
