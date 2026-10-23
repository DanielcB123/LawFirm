<?php

namespace App\Console\Commands;

use App\Models\User;
use App\Tenancy\FirmOnboardingService;
use App\Tenancy\OnboardingFastTrackGuard;
use Illuminate\Console\Command;
use RuntimeException;

class FastTrackFirmOnboardingCommand extends Command
{
    protected $signature = 'onboarding:fast-track
        {--name= : Applicant full name}
        {--email= : Applicant email}
        {--password= : Optional password (generated when omitted)}
        {--firm= : Firm name}
        {--phone= : Optional business phone}';

    protected $description = 'Create or reuse a local/testing applicant and fast-track firm onboarding.';

    public function handle(FirmOnboardingService $onboarding, OnboardingFastTrackGuard $guard): int
    {
        try {
            $guard->assertRuntimeAllowed();
        } catch (RuntimeException $exception) {
            $this->error($exception->getMessage());

            return self::FAILURE;
        }

        $name = (string) ($this->option('name') ?: 'Local Firm Owner');
        $firmName = (string) ($this->option('firm') ?: 'Local Test Firm '.now()->format('His'));
        $email = (string) ($this->option('email') ?: 'owner+'.now()->format('YmdHis').'@matterforge.local');
        $password = (string) ($this->option('password') ?: bin2hex(random_bytes(6)).'!Aa1');
        $phoneOption = $this->option('phone');
        $phone = is_string($phoneOption) && trim($phoneOption) !== '' ? trim($phoneOption) : null;

        $user = User::query()->where('email', $email)->first();
        if ($user instanceof User) {
            $application = $onboarding->latestApplicationFor($user);
            if ($application === null) {
                $application = $onboarding->submitForAuthenticatedUser($user, [
                    'firmName' => $firmName,
                    'applicantName' => $name,
                    'applicantEmail' => $email,
                    'businessPhone' => $phone,
                ]);
            }
        } else {
            $result = $onboarding->registerGuestAndSubmit([
                'name' => $name,
                'email' => $email,
                'password' => $password,
                'firmName' => $firmName,
                'businessPhone' => $phone,
            ]);
            $application = $result['application'];
        }

        $applicant = $application->applicant()->firstOrFail();
        $onboarding->runFastTrack($application, $applicant);
        $status = $onboarding->statusPayload($applicant);
        /** @var array<string, mixed> $details */
        $details = is_array($status['application'] ?? null) ? $status['application'] : [];

        $this->info('Fast-track onboarding complete.');
        $this->line('Applicant email: '.$email);
        $this->line('Generated password (if new user): '.$password);
        $this->line('Firm: '.((string) ($details['firmName'] ?? $firmName)));
        $this->line('Lifecycle: '.((string) ($details['lifecycleStatus'] ?? 'unknown')));
        $this->line('Provisioning: '.((string) ($details['provisioningStatus'] ?? 'unknown')));
        $this->line('Workspace host: '.((string) ($details['preferredHost'] ?? $details['fallbackHost'] ?? 'localhost')));

        return self::SUCCESS;
    }
}
