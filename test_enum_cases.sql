-- Test ENUM extraction with various comment formats

-- This is a header comment that should be ignored

-- ENUM with preceding comment
CREATE TYPE status AS ENUM ('active', 'inactive')

-- ENUM with COMMENT ON TYPE
CREATE TYPE priority AS ENUM ('low', 'medium', 'high');
COMMENT ON TYPE priority IS 'Priority level';

-- ENUM with schema
CREATE TYPE public.color AS ENUM ('red', 'green', 'blue');
COMMENT ON TYPE public.color IS 'Color enum with schema';

-- Multi-value enum
CREATE TYPE day_of_week AS ENUM (
    'monday',
    'tuesday',
    'wednesday',
    'thursday',
    'friday',
    'saturday',
    'sunday'
);
COMMENT ON TYPE day_of_week IS 'Days of the week';
