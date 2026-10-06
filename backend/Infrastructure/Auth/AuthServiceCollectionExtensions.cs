using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddLawFirmAuthorization(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(AuthConstants.JwtSectionName)
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.Issuer) &&
                    !string.IsNullOrWhiteSpace(options.Audience) &&
                    !string.IsNullOrWhiteSpace(options.SigningKey) &&
                    options.SigningKey.Length >= 32,
                "Jwt configuration is missing or invalid.")
            .ValidateOnStart();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.ContactsRead, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.ContactsRead)));
            options.AddPolicy(AuthPolicies.ContactsManage, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.ContactsManage)));
            options.AddPolicy(AuthPolicies.ConflictsSearch, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.ConflictsSearch)));
            options.AddPolicy(AuthPolicies.ConflictsReview, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.ConflictsReview)));
            options.AddPolicy(AuthPolicies.CalendarRead, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.CalendarRead)));
            options.AddPolicy(AuthPolicies.CalendarManage, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.CalendarManage)));
            options.AddPolicy(AuthPolicies.IntakeRead, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.IntakeRead)));
            options.AddPolicy(AuthPolicies.IntakeManage, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.IntakeManage)));
            options.AddPolicy(AuthPolicies.ClientPortal, policy =>
                policy.RequireAuthenticatedUser().RequireRole(AuthRoles.Client));

            options.AddPolicy(AuthPolicies.StaffPortal, policy =>
                policy.RequireAuthenticatedUser().RequireRole(AuthRoles.Attorney, AuthRoles.Paralegal, AuthRoles.Admin));

            options.AddPolicy(AuthPolicies.MatterReadOwn, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.MattersReadOwn)));
            options.AddPolicy(AuthPolicies.MatterReadAssigned, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.MattersReadAssigned)));
            options.AddPolicy(AuthPolicies.MatterReadAll, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.MattersReadAll)));
            options.AddPolicy(AuthPolicies.MatterUpdateAssigned, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.MattersUpdateAssigned)));
            options.AddPolicy(AuthPolicies.MatterUpdateAll, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.MattersUpdateAll)));
            options.AddPolicy(AuthPolicies.DocumentUpload, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.DocumentsUpload)));
            options.AddPolicy(AuthPolicies.DocumentReview, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.DocumentsReview)));
            options.AddPolicy(AuthPolicies.BillingReadOwn, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.BillingReadOwn)));
            options.AddPolicy(AuthPolicies.BillingManage, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.BillingManage)));
            options.AddPolicy(AuthPolicies.UsersManage, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(AuthPermissions.UsersManage)));
        });

        return services;
    }

    private sealed class ConfigureJwtBearerOptions(IOptions<JwtOptions> options) : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly JwtOptions _jwtOptions = options.Value;

        public void Configure(string? name, JwtBearerOptions options)
        {
            Configure(options);
        }

        public void Configure(JwtBearerOptions options)
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = "name",
                RoleClaimType = "role"
            };
        }
    }
}
