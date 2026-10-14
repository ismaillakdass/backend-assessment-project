using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Assessment.Application.DTOs.Auth;
using Assessment.Application.Interfaces;
using Assessment.Domain.Entities.Identity;
using Assessment.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Assessment.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ITokenRevocationService _tokenRevocationService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration,
        ITokenRevocationService tokenRevocationService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _tokenRevocationService = tokenRevocationService;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("This user account has been deactivated.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = new List<string>();

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                permissions.AddRange(roleClaims.Where(c => c.Type == "Permission").Select(c => c.Value));
            }
        }

        permissions = permissions.Distinct().ToList();

        var (token, expiresAt) = GenerateJwtToken(user, roles, permissions);

        return new LoginResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Roles = roles.ToList(),
            Permissions = permissions,
            ExpiresAt = expiresAt
        };
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            throw new BusinessException($"Email '{dto.Email}' is already registered.", "DUPLICATE_EMAIL");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BusinessException($"Failed to register user: {errors}");
        }

        // Assign default Viewer role
        await _userManager.AddToRoleAsync(user, "Viewer");
        return true;
    }

    public async Task LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        // Calls token revocation service
        await _tokenRevocationService.RevokeTokenAsync(token, "User logged out", cancellationToken);
        _logger.LogInformation("Logout processed for token");
    }

    private (string Token, DateTime ExpiresAt) GenerateJwtToken(
        ApplicationUser user,
        IList<string> roles,
        IList<string> permissions)
    {
        var jwtSecret = _configuration["Jwt:Secret"] ?? "SuperSecretKeyForAssessmentTestingPurposes123!#";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(4);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("name", $"{user.FirstName} {user.LastName}".Trim())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _configuration["Jwt:Issuer"] ?? "LosevAssessmentApi",
            Audience = _configuration["Jwt:Audience"] ?? "LosevAssessmentClient",
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }
}
