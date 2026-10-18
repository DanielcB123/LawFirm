#!/bin/sh
set -eu

APP_ROOT="/var/www/html"
LOCK_HASH_FILE="${APP_ROOT}/vendor/.composer.lock.hash"

cd "${APP_ROOT}"

if [ -f ".env.docker" ]; then
    cp .env.docker .env
fi

sync_composer_dependencies_if_needed() {
    if [ ! -f "composer.lock" ]; then
        return
    fi

    current_hash="$(sha256sum composer.lock | awk '{print $1}')"
    previous_hash=""

    if [ -f "${LOCK_HASH_FILE}" ]; then
        previous_hash="$(cat "${LOCK_HASH_FILE}")"
    fi

    if [ ! -f "vendor/autoload.php" ] || [ "${current_hash}" != "${previous_hash}" ]; then
        composer install --no-interaction --prefer-dist
        echo "${current_hash}" > "${LOCK_HASH_FILE}"
    fi
}

run_migrations() {
    php artisan migrate --force
}

start_server() {
    php artisan serve --host=0.0.0.0 --port=8000
}

mode="${1:-web}"

case "${mode}" in
    web)
        start_server
        ;;
    dev-web)
        sync_composer_dependencies_if_needed
        start_server
        ;;
    migrate)
        sync_composer_dependencies_if_needed
        run_migrations
        ;;
    *)
        exec "$@"
        ;;
esac
