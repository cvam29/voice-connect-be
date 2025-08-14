using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoiceConnect.Backend.Data;
using VoiceConnect.Backend.Models;

namespace VoiceConnect.Backend.Services;

public interface IUserService
{
    Task<User?> RegisterAsync(string username, string phone);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByPhoneAsync(string phone);
    Task<List<User>> GetFavoritesAsync(string userId);
    Task<bool> AddFavoriteAsync(string userId, string favoriteUserId);
    Task<bool> RemoveFavoriteAsync(string userId, string favoriteUserId);
}

public class UserService : IUserService
{
    private readonly VoiceConnectDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IAuthService _authService;

    public UserService(VoiceConnectDbContext context, ILogger<UserService> logger, IAuthService authService)
    {
        _context = context;
        _logger = logger;
        _authService = authService;
    }

    public async Task<User?> RegisterAsync(string username, string phone)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
            if (existingUser != null)
            {
                _logger.LogWarning($"User already exists with phone {phone}");
                return null;
            }

            // Check if username is taken
            var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existingUsername != null)
            {
                _logger.LogWarning($"Username {username} is already taken");
                return null;
            }

            // Create new user
            var user = new User
            {
                Username = username,
                Phone = phone,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send OTP for verification
            await _authService.SendOtpAsync(phone);

            _logger.LogInformation($"User registered successfully: {username} ({phone})");
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to register user {username} with phone {phone}");
            return null;
        }
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        try
        {
            return await _context.Users.FindAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get user by ID {userId}");
            return null;
        }
    }

    public async Task<User?> GetUserByPhoneAsync(string phone)
    {
        try
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get user by phone {phone}");
            return null;
        }
    }

    public async Task<List<User>> GetFavoritesAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.FavoriteUserIds.Any())
            {
                return new List<User>();
            }

            var favorites = await _context.Users
                .Where(u => user.FavoriteUserIds.Contains(u.Id))
                .ToListAsync();

            return favorites;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get favorites for user {userId}");
            return new List<User>();
        }
    }

    public async Task<bool> AddFavoriteAsync(string userId, string favoriteUserId)
    {
        try
        {
            if (userId == favoriteUserId)
            {
                _logger.LogWarning($"User {userId} cannot favorite themselves");
                return false;
            }

            var user = await _context.Users.FindAsync(userId);
            var favoriteUser = await _context.Users.FindAsync(favoriteUserId);

            if (user == null || favoriteUser == null)
            {
                _logger.LogWarning($"User not found: {userId} or {favoriteUserId}");
                return false;
            }

            if (!user.FavoriteUserIds.Contains(favoriteUserId))
            {
                user.FavoriteUserIds.Add(favoriteUserId);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"User {userId} added {favoriteUserId} to favorites");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to add favorite {favoriteUserId} for user {userId}");
            return false;
        }
    }

    public async Task<bool> RemoveFavoriteAsync(string userId, string favoriteUserId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                return false;
            }

            if (user.FavoriteUserIds.Contains(favoriteUserId))
            {
                user.FavoriteUserIds.Remove(favoriteUserId);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"User {userId} removed {favoriteUserId} from favorites");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to remove favorite {favoriteUserId} for user {userId}");
            return false;
        }
    }
}