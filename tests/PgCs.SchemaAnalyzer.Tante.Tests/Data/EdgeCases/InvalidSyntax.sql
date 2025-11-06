-- Файл с синтаксическими ошибками

-- Неполный CREATE TABLE
CREATE TABLE broken_table

-- Неполный CREATE TYPE
CREATE TYPE broken_enum AS ENUM

-- Невалидный синтаксис
THIS IS NOT VALID SQL;

-- Незавершенная функция
CREATE FUNCTION incomplete_function()
RETURNS VOID AS $$
BEGIN
-- Нет END;

