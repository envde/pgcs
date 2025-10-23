CREATE TABLE a1 (
    id INT NOT NULL
)
CREATE TABLE a2 (
    id INT NOT NULL
)
CREATE TYPE status AS ENUM ('active', 'inactive')
CREATE INDEX idx_a1 ON a1(id)