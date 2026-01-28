-- Creates the application databases if they don't already exist.
-- Runs automatically on first container init.
--
-- Note: CREATE DATABASE cannot run inside a transaction/DO block.
-- psql's \gexec lets us conditionally execute the generated statement.

SELECT format('CREATE DATABASE %I', 'vibeapi')
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'vibeapi')\gexec

SELECT format('CREATE DATABASE %I', 'vibeapi_dev')
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'vibeapi_dev')\gexec
