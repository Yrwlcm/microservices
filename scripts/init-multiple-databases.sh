#!/bin/bash

set -e
set -u

function create_database() {
    local database=$1
    echo "Creating database '$database'"
    psql -v ON_ERROR_STOP=1 --username "postgres" <<-EOSQL
        CREATE DATABASE $database;
        GRANT ALL PRIVILEGES ON DATABASE $database TO postgres;
EOSQL
}

# Создаем все необходимые базы данных
echo "Creating multiple databases..."
create_database "orders"
create_database "inventory" 
create_database "payments"
create_database "notifications"
echo "All databases created successfully"