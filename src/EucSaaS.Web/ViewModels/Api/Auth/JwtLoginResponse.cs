namespace EucSaaS.Web.ViewModels.Api.Auth;

public class JwtLoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public int ExpiresIn { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public JwtUserResponse User { get; set; } = new();
}

public class JwtUserResponse
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public Guid TenantId { get; set; }

    public Guid AppRoleId { get; set; }

    public string Role { get; set; } = string.Empty;

    public Guid? DepartmentId { get; set; }

    public string? Department { get; set; }
}
