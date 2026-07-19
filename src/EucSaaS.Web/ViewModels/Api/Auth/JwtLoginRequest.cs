using System.ComponentModel.DataAnnotations;

namespace EucSaaS.Web.ViewModels.Api.Auth;

public class JwtLoginRequest
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Password { get; set; } = string.Empty;
}
