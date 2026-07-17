using System.Security;

namespace EucSaaS.Web.Services.Security;

public class DataAccessScopeResolver
    : IDataAccessScopeResolver
{
    private readonly ICurrentUserContext _currentUserContext;

    public DataAccessScopeResolver(
        ICurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;
    }

    public DataAccessScope Resolve()
    {
        if (!_currentUserContext.IsAuthenticated)
        {
            throw new SecurityException(
                "The current user is not authenticated.");
        }

        if (!_currentUserContext.TenantId.HasValue)
        {
            throw new SecurityException(
                "The authenticated user does not have a TenantId claim.");
        }

        var roleName = NormalizeRoleName(
            _currentUserContext.RoleName);

        var canAccessAllDepartments =
            IsAdministratorRole(roleName);

        var department = NormalizeDepartment(
            _currentUserContext.Department);

        /*
         * Admin users are permitted to have no Department
         * claim because they can access all departments
         * within their own tenant.
         *
         * Non-admin users must have a Department claim.
         */
        if (!canAccessAllDepartments &&
            string.IsNullOrWhiteSpace(department))
        {
            throw new SecurityException(
                $"The authenticated user with role '{roleName ?? "Unknown"}' does not have a department claim.");
        }

        return new DataAccessScope
        {
            TenantId = _currentUserContext.TenantId.Value,
            UserId = _currentUserContext.UserId,
            AppRoleId = _currentUserContext.AppRoleId,
            RoleName = roleName,
            Department = department,
            CanAccessAllDepartments =
                canAccessAllDepartments
        };
    }

    private static bool IsAdministratorRole(
        string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        return roleName.Equals(
                   "Admin",
                   StringComparison.OrdinalIgnoreCase)
               ||
               roleName.Equals(
                   "Administrator",
                   StringComparison.OrdinalIgnoreCase)
               ||
               roleName.Equals(
                   "System Admin",
                   StringComparison.OrdinalIgnoreCase)
               ||
               roleName.Equals(
                   "System Administrator",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeRoleName(
        string? roleName)
    {
        return string.IsNullOrWhiteSpace(roleName)
            ? null
            : roleName.Trim();
    }

    private static string? NormalizeDepartment(
        string? department)
    {
        return string.IsNullOrWhiteSpace(department)
            ? null
            : department.Trim().ToUpperInvariant();
    }
}
