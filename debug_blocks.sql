CREATE TABLE users (
                       id BIGSERIAL PRIMARY KEY, -- comment: Уникальный идентификатор; type: BIGSERIAL; rename: UserId

                       username VARCHAR(50) NOT NULL UNIQUE -- comment: Логин пользователя; type: VARCHAR(50); rename: UserName
);
