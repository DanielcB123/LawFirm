<?php

namespace App\Tenancy;

class DnsTxtLookup
{
    /**
     * @return list<string>
     */
    public function records(string $recordName): array
    {
        $rows = dns_get_record($recordName, DNS_TXT);
        if (! is_array($rows)) {
            return [];
        }

        $values = [];
        foreach ($rows as $row) {
            if (isset($row['txt']) && is_string($row['txt'])) {
                $values[] = trim($row['txt']);
            }
        }

        return $values;
    }
}
