<?php

return [
    'dev_fast_track' => (bool) env('FIRM_ONBOARDING_DEV_FAST_TRACK', false),

    'required_verifications' => [
        'business_existence',
        'applicant_authority',
        'applicant_email',
        'domain_ownership',
    ],

    'local_mode_notice' => 'Local development fast-track is enabled. Verification and two-person approval gates are bypassed for onboarding only.',

    'verification_providers' => [
        'business_existence' => env('FIRM_ONBOARDING_BUSINESS_PROVIDER'),
        'applicant_authority' => env('FIRM_ONBOARDING_AUTHORITY_PROVIDER'),
        'applicant_email' => env('FIRM_ONBOARDING_EMAIL_PROVIDER'),
        'domain_ownership' => env('FIRM_ONBOARDING_DOMAIN_PROVIDER'),
    ],
];
