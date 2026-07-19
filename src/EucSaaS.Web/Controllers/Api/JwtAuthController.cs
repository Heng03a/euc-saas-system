using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Infrastructure.Data.Seed;
using EucSaaS.Web.ViewModels.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EucSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class JwtAuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public JwtAuthController(
        AppDbContext db,
        IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(
        typeof(JwtLoginResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] JwtLoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var normalizedUsername =
            request.Username.Trim();

        var user = await _db.AppUsers
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Department)
            .Include(x => x.Tenant)
            .FirstOrDefaultAsync(x =>
                x.Username == normalizedUsername);

        if (user == null ||
            !DatabaseSeeder.VerifyPassword(
                request.Password,
                user.PasswordHash))
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        if (user.Role == null)
        {
            return Unauthorized(new
            {
                message =
                    "The user does not have an assigned role."
            });
        }

        if (user.Tenant == null)
        {
            return Unauthorized(new
            {
                message =
                    "The user does not have an assigned tenant."
            });
        }

        var roleCode =
            user.Role.Code?.Trim() ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(roleCode))
        {
            return Unauthorized(new
            {
                message =
                    "The assigned role does not have a valid role code."
            });
        }

        var departmentCode =
            user.Department?.Code?.Trim();

        var jwtIssuer =
            GetRequiredJwtSetting("Jwt:Issuer");

        var jwtAudience =
            GetRequiredJwtSetting("Jwt:Audience");

        var jwtKey =
            GetRequiredJwtSetting("Jwt:Key");

        var expiryMinutes =
            _configuration.GetValue<int?>(
                "Jwt:ExpiryMinutes") ?? 60;

        if (expiryMinutes <= 0)
        {
            expiryMinutes = 60;
        }

        var issuedAtUtc = DateTime.UtcNow;

        var expiresAtUtc =
            issuedAtUtc.AddMinutes(expiryMinutes);

        var claims = CreateClaims(
            user.Id,
            user.Username,
            user.FullName,
            user.TenantId,
            user.RoleId,
            roleCode,
            user.DepartmentId,
            departmentCode);

        var signingKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

        var signingCredentials =
            new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var tokenValue =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        var response = new JwtLoginResponse
        {
            AccessToken = tokenValue,
            TokenType = "Bearer",
            ExpiresIn = expiryMinutes * 60,
            ExpiresAtUtc = expiresAtUtc,

            User = new JwtUserResponse
            {
                UserId = user.Id,
                Username = user.Username,
                FullName =
                    user.FullName ?? user.Username,

                TenantId = user.TenantId,
                AppRoleId = user.RoleId,
                Role = roleCode,

                DepartmentId =
                    user.DepartmentId == Guid.Empty
                        ? null
                        : user.DepartmentId,

                Department =
                    string.IsNullOrWhiteSpace(departmentCode)
                        ? null
                        : departmentCode.ToUpperInvariant()
            }
        };

        return Ok(response);
    }

    private static List<Claim> CreateClaims(
        Guid userId,
        string username,
        string? fullName,
        Guid tenantId,
        Guid appRoleId,
        string roleCode,
        Guid departmentId,
        string? departmentCode)
    {
        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new(
                JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow
                    .ToUnixTimeSeconds()
                    .ToString(),
                ClaimValueTypes.Integer64),

            new(
                ClaimTypes.NameIdentifier,
                userId.ToString()),

            new(
                ClaimTypes.Name,
                username),

            new(
                "FullName",
                fullName ?? username),

            new(
                "TenantId",
                tenantId.ToString()),

            new(
                "AppRoleId",
                appRoleId.ToString()),

            new(
                ClaimTypes.Role,
                roleCode)
        };

        if (departmentId != Guid.Empty)
        {
            claims.Add(
                new Claim(
                    "DepartmentId",
                    departmentId.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(departmentCode))
        {
            claims.Add(
                new Claim(
                    "Department",
                    departmentCode
                        .Trim()
                        .ToUpperInvariant()));
        }

        return claims;
    }

    private string GetRequiredJwtSetting(
        string configurationKey)
    {
        var value =
            _configuration[configurationKey];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Configuration '{configurationKey}' is missing.");
        }

        return value;
    }
}
