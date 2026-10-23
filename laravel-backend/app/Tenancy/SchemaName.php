<?php

namespace App\Tenancy;

use InvalidArgumentException;

class SchemaName
{
    public static function generate(): string
    {
        return 'mf_f_'.bin2hex(random_bytes(16));
    }

    public static function assertValid(string $schemaName): void
    {
        if (preg_match('/^mf_f_[a-f0-9]{32}$/', $schemaName) !== 1) {
            throw new InvalidArgumentException('Schema name is not a server-generated identifier.');
        }
    }

    public static function quote(string $schemaName): string
    {
        self::assertValid($schemaName);

        return '`'.$schemaName.'`';
    }
}
