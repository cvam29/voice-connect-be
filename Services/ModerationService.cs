using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoiceConnect.Backend.Data;
using VoiceConnect.Backend.Models;

namespace VoiceConnect.Backend.Services;

public interface IModerationService
{
    Task<Report?> CreateReportAsync(string reporterId, ReportType type, string targetId, string reason, string? description = null);
    Task<List<Report>> GetReportsAsync(ReportStatus? status = null);
    Task<List<Report>> GetPendingReportsAsync();
    Task<Report?> GetReportByIdAsync(string reportId);
    Task<Report?> ResolveReportAsync(string reportId, string moderatorId, ReportStatus status, string? resolutionNotes = null);
    Task<bool> AssignModeratorRoleAsync(string userId, string assignedBy);
    Task<bool> RemoveModeratorRoleAsync(string userId, string removedBy);
    Task<bool> BanUserAsync(string userId, string moderatorId, string reason, DateTime? bannedUntil = null);
    Task<bool> UnbanUserAsync(string userId, string moderatorId);
    Task<List<User>> GetModeratedUsersAsync();
    Task<bool> IsModerator(string userId);
}

public class ModerationService : IModerationService
{
    private readonly VoiceConnectDbContext _context;
    private readonly ILogger<ModerationService> _logger;
    private readonly IUserService _userService;

    public ModerationService(VoiceConnectDbContext context, ILogger<ModerationService> logger, IUserService userService)
    {
        _context = context;
        _logger = logger;
        _userService = userService;
    }

    public async Task<Report?> CreateReportAsync(string reporterId, ReportType type, string targetId, string reason, string? description = null)
    {
        try
        {
            // Check if reporter exists
            var reporter = await _userService.GetUserByIdAsync(reporterId);
            if (reporter == null)
            {
                _logger.LogWarning($"Reporter not found: {reporterId}");
                return null;
            }

            // Validate target exists based on type
            if (!await ValidateTargetExistsAsync(type, targetId))
            {
                _logger.LogWarning($"Target {targetId} of type {type} not found");
                return null;
            }

            var report = new Report
            {
                ReporterId = reporterId,
                Type = type,
                TargetId = targetId,
                Reason = reason,
                Description = description,
                Status = ReportStatus.Pending
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Report created by {reporterId} for {type} {targetId}");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create report by {reporterId} for {type} {targetId}");
            return null;
        }
    }

    public async Task<List<Report>> GetReportsAsync(ReportStatus? status = null)
    {
        try
        {
            var query = _context.Reports.AsQueryable();
            
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get reports");
            return new List<Report>();
        }
    }

    public async Task<List<Report>> GetPendingReportsAsync()
    {
        return await GetReportsAsync(ReportStatus.Pending);
    }

    public async Task<Report?> GetReportByIdAsync(string reportId)
    {
        try
        {
            return await _context.Reports.FindAsync(reportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get report {reportId}");
            return null;
        }
    }

    public async Task<Report?> ResolveReportAsync(string reportId, string moderatorId, ReportStatus status, string? resolutionNotes = null)
    {
        try
        {
            // Verify moderator
            if (!await IsModerator(moderatorId))
            {
                _logger.LogWarning($"User {moderatorId} is not a moderator");
                return null;
            }

            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                _logger.LogWarning($"Report not found: {reportId}");
                return null;
            }

            report.Status = status;
            report.ReviewedAt = DateTime.UtcNow;
            report.ReviewedBy = moderatorId;
            report.ResolutionNotes = resolutionNotes;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Report {reportId} resolved by moderator {moderatorId}");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to resolve report {reportId}");
            return null;
        }
    }

    public async Task<bool> AssignModeratorRoleAsync(string userId, string assignedBy)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                return false;
            }

            user.Role = UserRole.Moderator;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} assigned moderator role by {assignedBy}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to assign moderator role to user {userId}");
            return false;
        }
    }

    public async Task<bool> RemoveModeratorRoleAsync(string userId, string removedBy)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                return false;
            }

            user.Role = UserRole.User;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} removed from moderator role by {removedBy}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to remove moderator role from user {userId}");
            return false;
        }
    }

    public async Task<bool> BanUserAsync(string userId, string moderatorId, string reason, DateTime? bannedUntil = null)
    {
        try
        {
            // Verify moderator
            if (!await IsModerator(moderatorId))
            {
                _logger.LogWarning($"User {moderatorId} is not a moderator");
                return false;
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                return false;
            }

            user.IsBanned = true;
            user.BannedAt = DateTime.UtcNow;
            user.BannedUntil = bannedUntil;
            user.BannedBy = moderatorId;
            user.BanReason = reason;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} banned by moderator {moderatorId} until {bannedUntil?.ToString() ?? "indefinitely"}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to ban user {userId}");
            return false;
        }
    }

    public async Task<bool> UnbanUserAsync(string userId, string moderatorId)
    {
        try
        {
            // Verify moderator
            if (!await IsModerator(moderatorId))
            {
                _logger.LogWarning($"User {moderatorId} is not a moderator");
                return false;
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                return false;
            }

            user.IsBanned = false;
            user.BannedAt = null;
            user.BannedUntil = null;
            user.BannedBy = null;
            user.BanReason = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} unbanned by moderator {moderatorId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to unban user {userId}");
            return false;
        }
    }

    public async Task<List<User>> GetModeratedUsersAsync()
    {
        try
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Moderator || u.Role == UserRole.Admin)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get moderated users");
            return new List<User>();
        }
    }

    public async Task<bool> IsModerator(string userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Role == UserRole.Moderator || user?.Role == UserRole.Admin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to check moderator status for user {userId}");
            return false;
        }
    }

    private async Task<bool> ValidateTargetExistsAsync(ReportType type, string targetId)
    {
        try
        {
            return type switch
            {
                ReportType.User => await _context.Users.AnyAsync(u => u.Id == targetId),
                ReportType.Topic => await _context.Topics.AnyAsync(t => t.Id == targetId),
                ReportType.Message => await _context.Messages.AnyAsync(m => m.Id == targetId),
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to validate target {targetId} of type {type}");
            return false;
        }
    }
}