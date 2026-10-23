<?php

namespace App\Enums;

enum FirmProvisioningStatus: string
{
    case NotRequested = 'not_requested';
    case Requested = 'requested';
    case Provisioning = 'provisioning';
    case Ready = 'ready';
    case Failed = 'failed';
}
