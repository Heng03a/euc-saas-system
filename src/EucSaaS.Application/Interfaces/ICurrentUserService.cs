namespace EucSaaS.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid TenantId { get; }
    string Username { get; }
    string Role { get; }
}
