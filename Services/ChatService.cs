using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoiceConnect.Backend.Data;
using VoiceConnect.Backend.Models;

namespace VoiceConnect.Backend.Services;

public interface IChatService
{
    Task<Message?> SendMessageAsync(string senderId, string recipientId, string content);
    Task<List<Message>> GetMessagesAsync(string userId, string otherUserId, int limit = 50);
    Task<bool> MarkMessageAsReadAsync(string messageId, string userId);
}

public class ChatService : IChatService
{
    private readonly VoiceConnectDbContext _context;
    private readonly ILogger<ChatService> _logger;

    public ChatService(VoiceConnectDbContext context, ILogger<ChatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Message?> SendMessageAsync(string senderId, string recipientId, string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Message content cannot be empty");
                return null;
            }

            if (senderId == recipientId)
            {
                _logger.LogWarning($"User {senderId} cannot send message to themselves");
                return null;
            }

            var sender = await _context.Users.FindAsync(senderId);
            var recipient = await _context.Users.FindAsync(recipientId);

            if (sender == null || recipient == null)
            {
                _logger.LogWarning($"Sender or recipient not found: {senderId}, {recipientId}");
                return null;
            }

            var message = new Message
            {
                SenderId = senderId,
                RecipientId = recipientId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Message sent from {senderId} to {recipientId}: {message.Id}");
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send message from {senderId} to {recipientId}");
            return null;
        }
    }

    public async Task<List<Message>> GetMessagesAsync(string userId, string otherUserId, int limit = 50)
    {
        try
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.RecipientId == otherUserId) ||
                           (m.SenderId == otherUserId && m.RecipientId == userId))
                .OrderByDescending(m => m.SentAt)
                .Take(limit)
                .ToListAsync();

            // Reverse to get chronological order
            messages.Reverse();

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get messages between {userId} and {otherUserId}");
            return new List<Message>();
        }
    }

    public async Task<bool> MarkMessageAsReadAsync(string messageId, string userId)
    {
        try
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                _logger.LogWarning($"Message not found: {messageId}");
                return false;
            }

            if (message.RecipientId != userId)
            {
                _logger.LogWarning($"User {userId} is not the recipient of message {messageId}");
                return false;
            }

            message.IsRead = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Message {messageId} marked as read by user {userId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to mark message {messageId} as read by user {userId}");
            return false;
        }
    }
}