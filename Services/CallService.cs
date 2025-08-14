using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoiceConnect.Backend.Data;
using VoiceConnect.Backend.Models;

namespace VoiceConnect.Backend.Services;

public interface ICallService
{
    Task<CallRequest?> CreateCallRequestAsync(string topicId, string fromUserId);
    Task<CallRequest?> AcceptCallRequestAsync(string requestId, string userId);
    Task<CallRequest?> RejectCallRequestAsync(string requestId, string userId);
    Task<List<CallRequest>> GetPendingCallRequestsAsync(string userId);
    Task CleanupExpiredCallRequestsAsync();
}

public class CallService : ICallService
{
    private readonly VoiceConnectDbContext _context;
    private readonly ILogger<CallService> _logger;
    private const int CALL_REQUEST_TIMEOUT_MINUTES = 2; // Call requests expire after 2 minutes

    public CallService(VoiceConnectDbContext context, ILogger<CallService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CallRequest?> CreateCallRequestAsync(string topicId, string fromUserId)
    {
        try
        {
            var topic = await _context.Topics.FindAsync(topicId);
            if (topic == null)
            {
                _logger.LogWarning($"Topic not found: {topicId}");
                return null;
            }

            var fromUser = await _context.Users.FindAsync(fromUserId);
            if (fromUser == null)
            {
                _logger.LogWarning($"User not found: {fromUserId}");
                return null;
            }

            // Check if user is trying to call their own topic
            if (topic.AuthorId == fromUserId)
            {
                _logger.LogWarning($"User {fromUserId} cannot call their own topic {topicId}");
                return null;
            }

            // Check for existing pending call requests from this user to this topic
            var existingRequest = await _context.CallRequests
                .FirstOrDefaultAsync(cr => cr.TopicId == topicId && 
                                         cr.FromUserId == fromUserId && 
                                         cr.Status == CallRequestStatus.Pending &&
                                         cr.CreatedAt > DateTime.UtcNow.AddMinutes(-CALL_REQUEST_TIMEOUT_MINUTES));

            if (existingRequest != null)
            {
                _logger.LogWarning($"User {fromUserId} already has a pending call request for topic {topicId}");
                return existingRequest;
            }

            var callRequest = new CallRequest
            {
                TopicId = topicId,
                FromUserId = fromUserId,
                ToUserId = topic.AuthorId, // Initially set to topic author, can be updated when someone accepts
                Status = CallRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.CallRequests.Add(callRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Call request created: {callRequest.Id} from {fromUserId} for topic {topicId}");
            return callRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create call request from {fromUserId} for topic {topicId}");
            return null;
        }
    }

    public async Task<CallRequest?> AcceptCallRequestAsync(string requestId, string userId)
    {
        try
        {
            var callRequest = await _context.CallRequests.FindAsync(requestId);
            if (callRequest == null)
            {
                _logger.LogWarning($"Call request not found: {requestId}");
                return null;
            }

            if (callRequest.Status != CallRequestStatus.Pending)
            {
                _logger.LogWarning($"Call request {requestId} is not in pending status: {callRequest.Status}");
                return null;
            }

            // Check if request has expired
            if (callRequest.CreatedAt < DateTime.UtcNow.AddMinutes(-CALL_REQUEST_TIMEOUT_MINUTES))
            {
                callRequest.Status = CallRequestStatus.Expired;
                await _context.SaveChangesAsync();
                _logger.LogWarning($"Call request {requestId} has expired");
                return null;
            }

            // User cannot accept their own call request
            if (callRequest.FromUserId == userId)
            {
                _logger.LogWarning($"User {userId} cannot accept their own call request {requestId}");
                return null;
            }

            // Get the topic to verify user can accept this call
            var topic = await _context.Topics.FindAsync(callRequest.TopicId);
            if (topic == null)
            {
                _logger.LogWarning($"Topic not found: {callRequest.TopicId}");
                return null;
            }

            callRequest.Status = CallRequestStatus.Accepted;
            callRequest.ToUserId = userId;
            callRequest.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Call request {requestId} accepted by user {userId}");
            return callRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to accept call request {requestId} by user {userId}");
            return null;
        }
    }

    public async Task<CallRequest?> RejectCallRequestAsync(string requestId, string userId)
    {
        try
        {
            var callRequest = await _context.CallRequests.FindAsync(requestId);
            if (callRequest == null)
            {
                _logger.LogWarning($"Call request not found: {requestId}");
                return null;
            }

            if (callRequest.Status != CallRequestStatus.Pending)
            {
                _logger.LogWarning($"Call request {requestId} is not in pending status: {callRequest.Status}");
                return null;
            }

            // User cannot reject their own call request
            if (callRequest.FromUserId == userId)
            {
                _logger.LogWarning($"User {userId} cannot reject their own call request {requestId}");
                return null;
            }

            callRequest.Status = CallRequestStatus.Rejected;
            callRequest.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Call request {requestId} rejected by user {userId}");
            return callRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to reject call request {requestId} by user {userId}");
            return null;
        }
    }

    public async Task<List<CallRequest>> GetPendingCallRequestsAsync(string userId)
    {
        try
        {
            // Clean up expired requests first
            await CleanupExpiredCallRequestsAsync();

            // Get pending requests where user can respond (either topic author or anyone for public topics)
            var pendingRequests = await _context.CallRequests
                .Where(cr => cr.Status == CallRequestStatus.Pending &&
                           cr.FromUserId != userId && // Don't include own requests
                           cr.CreatedAt > DateTime.UtcNow.AddMinutes(-CALL_REQUEST_TIMEOUT_MINUTES))
                .OrderByDescending(cr => cr.CreatedAt)
                .ToListAsync();

            return pendingRequests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get pending call requests for user {userId}");
            return new List<CallRequest>();
        }
    }

    public async Task CleanupExpiredCallRequestsAsync()
    {
        try
        {
            var expiredRequests = await _context.CallRequests
                .Where(cr => cr.Status == CallRequestStatus.Pending &&
                           cr.CreatedAt < DateTime.UtcNow.AddMinutes(-CALL_REQUEST_TIMEOUT_MINUTES))
                .ToListAsync();

            foreach (var request in expiredRequests)
            {
                request.Status = CallRequestStatus.Expired;
            }

            if (expiredRequests.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Marked {expiredRequests.Count} call requests as expired");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired call requests");
        }
    }
}