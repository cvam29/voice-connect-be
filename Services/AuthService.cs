using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VoiceConnect.Backend.Data;
using VoiceConnect.Backend.Models;

namespace VoiceConnect.Backend.Services;

public interface IAuthService
{
    Task<bool> SendOtpAsync(string email);
    Task<AuthPayload?> LoginAsync(string email, string otp);
    Task<string> GenerateJwtTokenAsync(User user);
    Task<User?> ValidateJwtTokenAsync(string token);
}

public class AuthService : IAuthService
{
    private readonly VoiceConnectDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public AuthService(
        VoiceConnectDbContext context,
        ILogger<AuthService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _jwtSecret = configuration["JwtSecret"] ?? throw new ArgumentNullException("JwtSecret");
        _jwtIssuer = configuration["JwtIssuer"] ?? throw new ArgumentNullException("JwtIssuer");
        _jwtAudience = configuration["JwtAudience"] ?? throw new ArgumentNullException("JwtAudience");
    }

    public async Task<bool> SendOtpAsync(string email)
    {
        try
        {
            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();
            
            // Store OTP in database
            var otpCode = new OtpCode
            {
                Email = email,
                Code = otp,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            _context.OtpCodes.Add(otpCode);
            await _context.SaveChangesAsync();

            // Log OTP instead of sending via email (no email service configured)
            _logger.LogInformation($"OTP for {email}: {otp} (valid until {otpCode.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC)");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to generate OTP for {email}");
            return false;
        }
    }

    public async Task<AuthPayload?> LoginAsync(string email, string otp)
    {
        try
        {
            // Validate OTP
            var otpCode = await _context.OtpCodes
                .Where(o => o.Email == email && o.Code == otp && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpCode == null)
            {
                _logger.LogWarning($"Invalid OTP for email {email}");
                return null;
            }

            // Mark OTP as used
            otpCode.IsUsed = true;
            await _context.SaveChangesAsync();

            // Find user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning($"User not found for email {email} during login");
                return null;
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = await GenerateJwtTokenAsync(user);

            return new AuthPayload
            {
                Token = token,
                User = user
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Login failed for email {email}");
            return null;
        }
    }

    public Task<string> GenerateJwtTokenAsync(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: credentials
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public async Task<User?> ValidateJwtTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

            var user = await _context.Users.FindAsync(userId);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWT token validation failed");
            return null;
        }
    }
}