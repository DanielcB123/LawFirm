<?php

return [
    'dev_fast_track' => (bool) env('FIRM_ONBOARDING_DEV_FAST_TRACK', false),
    'dev_simplified_flow' => (bool) env('FIRM_ONBOARDING_DEV_SIMPLIFIED_FLOW', false),

    'required_verifications' => [
        'business_existence',
        'applicant_authority',
        'applicant_email',
        'domain_ownership',
    ],

    'name_suggestion_suffixes' => [
        'law',
        'legal',
        'group',
        'trial',
        'partners',
        'llp',
    ],

    'local_mode_notice' => 'Local development fast-track is enabled. Verification and two-person approval gates are bypassed for onboarding only.',

    'firm_demo_users' => [
        'enabled' => (bool) env('FIRM_ONBOARDING_DEMO_USERS_ENABLED', env('APP_ENV') !== 'production'),
        'email_domain' => env('FIRM_ONBOARDING_DEMO_USERS_EMAIL_DOMAIN', 'firms@localhost'),
        'password' => env('FIRM_ONBOARDING_DEMO_USERS_PASSWORD', 'Passw0rd!'),
        'accounts' => [
            ['prefix' => 'partner', 'name' => 'Firm Partner', 'role' => 'Partner'],
            ['prefix' => 'attorney', 'name' => 'Firm Attorney', 'role' => 'Attorney'],
            ['prefix' => 'paralegal', 'name' => 'Firm Paralegal', 'role' => 'Paralegal'],
            ['prefix' => 'assistant', 'name' => 'Firm Legal Assistant', 'role' => 'LegalAssistant'],
            ['prefix' => 'adminit', 'name' => 'Firm IT Administrator', 'role' => 'AdminIT'],
        ],
    ],

    'verification_providers' => [
        'business_existence' => env('FIRM_ONBOARDING_BUSINESS_PROVIDER'),
        'applicant_authority' => env('FIRM_ONBOARDING_AUTHORITY_PROVIDER'),
        'applicant_email' => env('FIRM_ONBOARDING_EMAIL_PROVIDER'),
        'domain_ownership' => env('FIRM_ONBOARDING_DOMAIN_PROVIDER'),
    ],

    'verification_workflows' => [
        'business_existence' => [
            'label' => 'Business existence',
            'description' => 'Confirm the applicant firm is an active legal entity.',
            'steps' => [
                'Collect legal business name, TIN/EIN, and jurisdiction of formation from the applicant.',
                'Validate records against IRS/public registry sources and keep a lookup reference.',
                'Record determination with evidence before approving this check.',
            ],
            'required_fields' => [
                ['key' => 'legal_business_name', 'label' => 'Legal business name', 'help_text' => 'Must match registration records.'],
                ['key' => 'tin_ein_last4', 'label' => 'TIN/EIN (last 4)', 'help_text' => 'Store minimum necessary tax identifier details.'],
                ['key' => 'irs_lookup_reference', 'label' => 'IRS/public lookup reference', 'help_text' => 'Reference id, URL, or case note proving the lookup.'],
            ],
        ],
        'applicant_authority' => [
            'label' => 'Applicant authority',
            'description' => 'Confirm the applicant can bind or administer the firm onboarding request.',
            'steps' => [
                'Verify applicant identity and their relationship to the firm.',
                'Validate authority through a trusted source (firm website, bar profile, or signed authorization).',
                'Record authority evidence and reviewer decision.',
            ],
            'required_fields' => [
                ['key' => 'applicant_role_title', 'label' => 'Applicant role/title', 'help_text' => 'Partner, administrator, founder, etc.'],
                ['key' => 'authority_source', 'label' => 'Authority source', 'help_text' => 'How authority was confirmed.'],
                ['key' => 'authority_reference', 'label' => 'Authority reference', 'help_text' => 'URL, document id, or recorded call reference.'],
            ],
        ],
        'applicant_email' => [
            'label' => 'Applicant email',
            'description' => 'Confirm the onboarding contact channel belongs to the applicant.',
            'steps' => [
                'Send or confirm a verification message for the applicant contact email.',
                'Validate the returned proof token or equivalent evidence.',
                'Record the verified email and proof reference.',
            ],
            'required_fields' => [
                ['key' => 'verified_email', 'label' => 'Verified email', 'help_text' => 'Email address that completed verification.'],
                ['key' => 'verification_method', 'label' => 'Verification method', 'help_text' => 'Email link, challenge code, trusted identity provider, etc.'],
                ['key' => 'proof_reference', 'label' => 'Proof reference', 'help_text' => 'Token id, audit record id, or provider reference.'],
            ],
        ],
        'domain_ownership' => [
            'label' => 'Domain ownership',
            'description' => 'Confirm control of the firm public hostname.',
            'steps' => [
                'Issue DNS TXT challenge or verify platform-managed fallback host status.',
                'Validate challenge completion and ensure domain is attached to the correct firm.',
                'Record domain evidence and finalize verification.',
            ],
            'required_fields' => [
                ['key' => 'hostname', 'label' => 'Hostname', 'help_text' => 'Tenant or custom hostname that was verified.'],
                ['key' => 'verification_source', 'label' => 'Verification source', 'help_text' => 'DNS TXT, platform-managed fallback, registrar proof, etc.'],
                ['key' => 'proof_reference', 'label' => 'Proof reference', 'help_text' => 'Challenge id, resolver snapshot, or verification event id.'],
            ],
        ],
    ],
];
