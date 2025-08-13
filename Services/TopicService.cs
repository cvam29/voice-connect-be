using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoiceConnect.Backend.Data;
using VoiceConnect.Backend.Models;

namespace VoiceConnect.Backend.Services;

public interface ITopicService
{
    Task<Topic?> CreateTopicAsync(string text, string authorId);
    Task<Topic?> BoostTopicAsync(string topicId, string userId);
    Task<List<Topic>> GetTopicsAsync(bool boostedOnly = false);
    Task<Topic?> GetTopicByIdAsync(string topicId);
}

public class TopicService : ITopicService
{
    private readonly VoiceConnectDbContext _context;
    private readonly ILogger<TopicService> _logger;

    public TopicService(VoiceConnectDbContext context, ILogger<TopicService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Topic?> CreateTopicAsync(string text, string authorId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Topic text cannot be empty");
                return null;
            }

            var author = await _context.Users.FindAsync(authorId);
            if (author == null)
            {
                _logger.LogWarning($"Author not found: {authorId}");
                return null;
            }

            var topic = new Topic
            {
                Text = text.Trim(),
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Topic created by user {authorId}: {topic.Id}");
            return topic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create topic for user {authorId}");
            return null;
        }
    }

    public async Task<Topic?> BoostTopicAsync(string topicId, string userId)
    {
        try
        {
            var topic = await _context.Topics.FindAsync(topicId);
            if (topic == null)
            {
                _logger.LogWarning($"Topic not found: {topicId}");
                return null;
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                return null;
            }

            // For MVP, boosting is free and lasts for 24 hours
            topic.Boosted = true;
            topic.BoostedUntil = DateTime.UtcNow.AddHours(24);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Topic {topicId} boosted by user {userId} until {topic.BoostedUntil}");
            return topic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to boost topic {topicId} by user {userId}");
            return null;
        }
    }

    public async Task<List<Topic>> GetTopicsAsync(bool boostedOnly = false)
    {
        try
        {
            var query = _context.Topics.AsQueryable();

            if (boostedOnly)
            {
                query = query.Where(t => t.Boosted && t.BoostedUntil > DateTime.UtcNow);
            }
            else
            {
                // Clean up expired boosts
                await CleanupExpiredBoostsAsync();
            }

            // Order by boosted status first, then by creation time
            var topics = await query
                .OrderByDescending(t => t.Boosted && t.BoostedUntil > DateTime.UtcNow)
                .ThenByDescending(t => t.CreatedAt)
                .Take(50) // Limit results
                .ToListAsync();

            return topics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get topics");
            return new List<Topic>();
        }
    }

    public async Task<Topic?> GetTopicByIdAsync(string topicId)
    {
        try
        {
            return await _context.Topics.FindAsync(topicId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get topic {topicId}");
            return null;
        }
    }

    private async Task CleanupExpiredBoostsAsync()
    {
        try
        {
            var expiredBoostedTopics = await _context.Topics
                .Where(t => t.Boosted && t.BoostedUntil <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var topic in expiredBoostedTopics)
            {
                topic.Boosted = false;
                topic.BoostedUntil = null;
            }

            if (expiredBoostedTopics.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Cleaned up {expiredBoostedTopics.Count} expired boosts");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired boosts");
        }
    }
}