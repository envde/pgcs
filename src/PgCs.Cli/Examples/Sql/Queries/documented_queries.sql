-- Example queries demonstrating documentation metadata support

-- name: GetUserById :one
-- summary: Retrieves a user by their unique identifier
-- param: id The unique user identifier (UUID)
-- returns: User object if found, null otherwise
SELECT id, username, email, active
FROM users
WHERE id = $1;

-- name: GetActiveUsers :many
-- summary: Retrieves all active users from the database
-- returns: List of active users with basic information
SELECT id, username, email
FROM users
WHERE active = true
ORDER BY username;

-- name: CreateUser :exec
-- summary: Creates a new user account in the system
-- param: username The desired username (must be unique)
-- param: email The user's email address
-- param: active Whether the account should be immediately active
-- returns: Number of rows affected (should be 1 on success)
INSERT INTO users (username, email, active)
VALUES ($1, $2, $3);

-- name: UpdateUserEmail :exec
-- summary: Updates the email address for an existing user
-- param: id The user's unique identifier
-- param: email The new email address
-- returns: Number of rows affected (1 if updated, 0 if user not found)
UPDATE users
SET email = $2
WHERE id = $1;

-- name: SearchUsers :many
-- summary: Searches for users by username pattern
-- param: search_pattern SQL LIKE pattern to match usernames (e.g., 'john%')
-- returns: List of matching users
SELECT id, username, email, active
FROM users
WHERE username LIKE $1
ORDER BY username
LIMIT 100;

-- This is an informational comment - it should be ignored by the parser
-- No special keywords here, so no processing needed

-- name: GetUserWithPosts :many
-- summary: Retrieves a user along with all their posts
-- param: user_id The unique identifier of the user
-- returns: Combined user and post information
SELECT 
    u.id as user_id,
    u.username,
    u.email,
    p.id as post_id,
    p.title as post_title,
    p.created_at as post_created_at
FROM users u
INNER JOIN posts p ON u.id = p.user_id
WHERE u.id = $1
ORDER BY p.created_at DESC;
