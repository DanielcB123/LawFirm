<?php

namespace App\Tenancy;

use App\Enums\FirmProvisioningStatus;
use App\Models\Firm;
use App\Models\User;

class FirmAuditRecorder
{
    public function record(Firm $firm, ?User $actor, string $eventType, string $message): void
    {
        $status = $firm->provisioning_status instanceof FirmProvisioningStatus
            ? $firm->provisioning_status->value
            : (string) $firm->provisioning_status;

        $firm->provisioningEvents()->create([
            'actor_user_id' => $actor?->id,
            'event_type' => $eventType,
            'provisioning_status' => $status,
            'message' => mb_substr($message, 0, 255),
        ]);
    }
}
