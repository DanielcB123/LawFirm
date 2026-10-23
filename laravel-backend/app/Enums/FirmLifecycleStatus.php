<?php

namespace App\Enums;

enum FirmLifecycleStatus: string
{
    case PendingReview = 'pending_review';
    case Approved = 'approved';
    case Rejected = 'rejected';
    case Active = 'active';
    case Suspended = 'suspended';
}
