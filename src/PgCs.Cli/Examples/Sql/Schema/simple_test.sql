-- Simple test schema

CREATE TABLE users
(
    id       BIGSERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    email    VARCHAR(255) NOT NULL,
    active   BOOLEAN DEFAULT true
);

CREATE TABLE posts
(
    id         BIGSERIAL PRIMARY KEY,
    user_id    BIGINT NOT NULL REFERENCES users(id),
    title      VARCHAR(200) NOT NULL,
    content    TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
