FROM composer:2 AS vendor

WORKDIR /app

COPY laravel-backend/composer.json laravel-backend/composer.lock ./

RUN composer install --no-interaction --prefer-dist --no-scripts

FROM php:8.4-cli

RUN apt-get update && apt-get install -y --no-install-recommends \
    git \
    unzip \
    libzip-dev \
    libicu-dev \
    libonig-dev \
    libxml2-dev \
    default-mysql-client \
    && docker-php-ext-install pdo_mysql mbstring intl bcmath \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
COPY --from=vendor /app/vendor /var/www/html/vendor

WORKDIR /var/www/html

COPY laravel-backend /var/www/html
COPY docker/laravel/entrypoint.sh /usr/local/bin/laravel-entrypoint

RUN sed -i 's/\r$//' /usr/local/bin/laravel-entrypoint \
    && chmod +x /usr/local/bin/laravel-entrypoint

ENTRYPOINT ["laravel-entrypoint"]
CMD ["web"]
