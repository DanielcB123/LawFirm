<?php

namespace App\Providers;

use App\Models\Firm;
use App\Policies\FirmPolicy;
use App\Tenancy\Contracts\RunsTenantMigrations;
use App\Tenancy\LaravelTenantMigrator;
use App\Tenancy\OnboardingFastTrackGuard;
use App\Tenancy\TenantConnectionManager;
use App\Tenancy\TenantContext;
use Illuminate\Support\Facades\Gate;
use Illuminate\Support\Facades\Queue;
use Illuminate\Support\ServiceProvider;

class AppServiceProvider extends ServiceProvider
{
    /**
     * Register any application services.
     */
    public function register(): void
    {
        $this->app->singleton(TenantContext::class);
        $this->app->singleton(RunsTenantMigrations::class, LaravelTenantMigrator::class);
    }

    /**
     * Bootstrap any application services.
     */
    public function boot(): void
    {
        $this->app->make(OnboardingFastTrackGuard::class)->assertConfigurationIsSafe();

        Gate::policy(Firm::class, FirmPolicy::class);

        $clearTenant = function (): void {
            $this->app->make(TenantConnectionManager::class)->clear();
        };

        Queue::before($clearTenant);
        Queue::after($clearTenant);
        Queue::failing($clearTenant);
        Queue::looping($clearTenant);
    }
}
