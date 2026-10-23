<?php

namespace App\Tenancy;

use Illuminate\Validation\ValidationException;

class HostnameNormalizer
{
    public function normalize(string $input): string
    {
        $value = trim($input);
        if ($value === '') {
            $this->throwInvalid();
        }

        $host = $this->extractHost($value);
        $host = rtrim(strtolower($host), '.');
        if ($host === '') {
            $this->throwInvalid();
        }

        $ascii = $this->toAscii($host);
        $ascii = strtolower($ascii);

        if (strlen($ascii) > 253) {
            $this->throwInvalid();
        }

        if (preg_match('/^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?)+$/', $ascii) !== 1) {
            $this->throwInvalid();
        }

        $reserved = config('tenancy.reserved_domains');
        if (is_array($reserved) && in_array($ascii, $reserved, true)) {
            throw ValidationException::withMessages([
                'hostname' => 'Hostname is reserved.',
            ]);
        }

        return $ascii;
    }

    private function extractHost(string $value): string
    {
        if (str_contains($value, '://')) {
            $parts = parse_url($value);
            if (! is_array($parts)) {
                $this->throwInvalid();
            }

            if (! empty($parts['user']) || ! empty($parts['pass']) || isset($parts['port']) || ! empty($parts['path']) || ! empty($parts['query']) || ! empty($parts['fragment'])) {
                $this->throwInvalid();
            }

            $host = $parts['host'] ?? null;
            if (! is_string($host) || $host === '') {
                $this->throwInvalid();
            }

            return $host;
        }

        if (preg_match('/[\/@?#]/', $value) === 1 || preg_match('/:\d+$/', $value) === 1) {
            $this->throwInvalid();
        }

        return $value;
    }

    private function toAscii(string $host): string
    {
        if (! function_exists('idn_to_ascii')) {
            return $host;
        }

        $flags = defined('IDNA_NONTRANSITIONAL_TO_ASCII') ? IDNA_NONTRANSITIONAL_TO_ASCII : 0;
        $variant = defined('INTL_IDNA_VARIANT_UTS46') ? INTL_IDNA_VARIANT_UTS46 : 0;
        $ascii = idn_to_ascii($host, $flags, $variant);

        if (! is_string($ascii) || $ascii === '') {
            $this->throwInvalid();
        }

        return $ascii;
    }

    private function throwInvalid(): never
    {
        throw ValidationException::withMessages([
            'hostname' => 'Hostname is not allowed.',
        ]);
    }
}
