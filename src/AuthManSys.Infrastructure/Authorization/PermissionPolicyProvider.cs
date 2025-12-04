using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AuthManSys.Infrastructure.Authorization;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private const string PermissionPolicyPrefix = "Permission.";

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if this is a permission-based policy
        if (policyName.StartsWith(PermissionPolicyPrefix))
        {
            var permission = policyName.Substring(PermissionPolicyPrefix.Length);
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Check if this is a plain permission name (without prefix)
        if (IsPermissionPolicy(policyName))
        {
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to default provider for other policies
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    private static bool IsPermissionPolicy(string policyName)
    {
        // You can add logic here to determine if a policy name represents a permission
        // For now, we'll assume any policy that contains certain patterns is a permission
        return policyName.Contains("Manage") ||
               policyName.Contains("View") ||
               policyName.Contains("Create") ||
               policyName.Contains("Edit") ||
               policyName.Contains("Delete") ||
               policyName.Contains("Access") ||
               policyName.Contains("Grant") ||
               policyName.Contains("Revoke") ||
               policyName.Contains("Bulk");
    }
}