namespace EucSaaS.Web.Security;

public static class AppPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string ManagerOrAdmin = "ManagerOrAdmin";
    public const string AuthenticatedOnly = "AuthenticatedOnly";
    public const string ReadAccess = "ReadAccess";
}
