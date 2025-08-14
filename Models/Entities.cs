using System.ComponentModel.DataAnnotations;

namespace VoiceConnect.Backend.Models;

public enum UserRole
{
    User,
    Moderator,
    Admin
}

public enum ReportType
{
    User,
    Topic,
    Message
}

public enum ReportStatus
{
    Pending,
    UnderReview,
    Resolved,
    Dismissed
}

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    public string? Bio { get; set; }
    
    public string? ProfilePictureUrl { get; set; }
    
    [Required]
    public string Phone { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Role and moderation properties
    public UserRole Role { get; set; } = UserRole.User;
    
    public bool IsBanned { get; set; } = false;
    
    public DateTime? BannedAt { get; set; }
    
    public DateTime? BannedUntil { get; set; }
    
    public string? BannedBy { get; set; }
    
    public string? BanReason { get; set; }
    
    // Navigation properties
    public List<string> FavoriteUserIds { get; set; } = new List<string>();
}

public class Topic
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string Text { get; set; } = string.Empty;
    
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    
    public bool Boosted { get; set; } = false;
    
    public DateTime? BoostedUntil { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public User? Author { get; set; }
}

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string SenderId { get; set; } = string.Empty;
    
    [Required]
    public string RecipientId { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    public bool IsRead { get; set; } = false;
    
    // Navigation properties
    public User? Sender { get; set; }
    public User? Recipient { get; set; }
}

public class CallRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string TopicId { get; set; } = string.Empty;
    
    [Required]
    public string FromUserId { get; set; } = string.Empty;
    
    public string? ToUserId { get; set; }
    
    [Required]
    public CallRequestStatus Status { get; set; } = CallRequestStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? RespondedAt { get; set; }
    
    // Navigation properties
    public Topic? Topic { get; set; }
    public User? From { get; set; }
    public User? To { get; set; }
}

public enum CallRequestStatus
{
    Pending,
    Accepted,
    Rejected,
    Cancelled,
    Expired
}

public class OtpCode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    public string Code { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    
    public bool IsUsed { get; set; } = false;
}

public class Report
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string ReporterId { get; set; } = string.Empty;
    
    [Required]
    public ReportType Type { get; set; }
    
    [Required]
    public string TargetId { get; set; } = string.Empty;
    
    [Required]
    public string Reason { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ReviewedAt { get; set; }
    
    public string? ReviewedBy { get; set; }
    
    public string? ResolutionNotes { get; set; }
    
    // Navigation properties
    public User? Reporter { get; set; }
    public User? ReviewedByUser { get; set; }
}