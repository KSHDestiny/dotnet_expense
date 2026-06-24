using System.ComponentModel.DataAnnotations;

namespace DotnetApp.Features.Auth;

/// <summary>
/// Strongly-typed JWT configuration, bound from the "Jwt" config section.
/// Validated at startup (see <c>AddJwtAuth</c>) so the app fails fast on bad config.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32, ErrorMessage = "Jwt:Secret must be at least 32 characters.")]
    public string Secret { get; set; } = null!;

    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int ExpiryMinutes { get; set; }
}
