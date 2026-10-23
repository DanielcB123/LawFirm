<?php

return [
    'roles' => [
        'platform_owner',
        'platform_ops',
    ],

    'permissions' => [
        'platform_owner' => [
            'platform.firms.read',
            'platform.firms.create',
            'platform.firms.memberships.manage',
            'platform.firms.domains.manage',
            'platform.firms.domains.finalize',
            'platform.firms.branding.manage',
            'platform.firms.verifications.record',
            'platform.firms.review.finalize',
            'platform.firms.lifecycle.manage',
            'platform.firms.provision',
            'platform.admins.manage',
        ],
        'platform_ops' => [
            'platform.firms.read',
            'platform.firms.create',
            'platform.firms.memberships.manage',
            'platform.firms.domains.manage',
            'platform.firms.branding.manage',
            'platform.firms.verifications.record',
        ],
    ],
];

