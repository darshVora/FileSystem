Make sure you have .Net 8 SDK, RabbitMQ, Postgres installed
Make sure you have database filedb with filedata table

CREATE DATABASE filedb
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'English_India.1252'
    LC_CTYPE = 'English_India.1252'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

CREATE TABLE FileData (
    Id SERIAL PRIMARY KEY,
    Content TEXT NOT NULL
);

Change password in appsetting.json for postgres and rabbitmq
Modify throttle count in appsetting.json of FileDataProcessorService default is 30
Run both FileDataProcessorService and FileManager