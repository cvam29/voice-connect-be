using VoiceConnect.Backend.Models;
using VoiceConnect.Backend.Services;

namespace VoiceConnect.Backend.GraphQL;

public class Query
{
    // Get topics with optional boosted filter
    public async Task<List<Topic>> GetTopicsAsync(
        [Service] ITopicService topicService,
        [Service] IUserService userService,
        bool? boostedOnly = null)
    {
        var topics = await topicService.GetTopicsAsync(boostedOnly ?? false);
        
        // Populate authors
        foreach (var topic in topics)
        {
            topic.Author = await userService.GetUserByIdAsync(topic.AuthorId);
        }

        return topics;
    }

    // Get all users (for favorites list)
    public async Task<List<User>> GetUsersAsync([Service] IUserService userService)
    {
        return new List<User>(); // Simplified for now - would implement proper user listing
    }
}

public class Mutation
{
    // Register a new user
    public async Task<User?> RegisterAsync(
        [Service] IUserService userService,
        string username,
        string email)
    {
        return await userService.RegisterAsync(username, email);
    }

    // Login with email and OTP
    public async Task<AuthPayload?> LoginAsync(
        [Service] IAuthService authService,
        string email,
        string otp)
    {
        return await authService.LoginAsync(email, otp);
    }

    // Create a new topic (simplified without auth for now)
    public async Task<Topic?> CreateTopicAsync(
        [Service] ITopicService topicService,
        [Service] IUserService userService,
        string text,
        string authorId)
    {
        var topic = await topicService.CreateTopicAsync(text, authorId);
        if (topic != null)
        {
            topic.Author = await userService.GetUserByIdAsync(authorId);
        }

        return topic;
    }

    // Boost a topic (simplified without auth for now)
    public async Task<Topic?> BoostTopicAsync(
        [Service] ITopicService topicService,
        [Service] IUserService userService,
        string topicId,
        string userId)
    {
        var topic = await topicService.BoostTopicAsync(topicId, userId);
        if (topic != null)
        {
            topic.Author = await userService.GetUserByIdAsync(topic.AuthorId);
        }

        return topic;
    }

    // Add user to favorites (simplified without auth for now)
    public async Task<bool> FavoriteUserAsync(
        [Service] IUserService userService,
        string userId,
        string favoriteUserId)
    {
        return await userService.AddFavoriteAsync(userId, favoriteUserId);
    }

    // Send a message (simplified without auth for now)
    public async Task<Message?> SendMessageAsync(
        [Service] IChatService chatService,
        [Service] IUserService userService,
        string fromUserId,
        string toUserId,
        string content)
    {
        var message = await chatService.SendMessageAsync(fromUserId, toUserId, content);
        if (message != null)
        {
            // Populate navigation properties
            message.Sender = await userService.GetUserByIdAsync(fromUserId);
            message.Recipient = await userService.GetUserByIdAsync(toUserId);
        }

        return message;
    }

    // Request a call (simplified without auth for now)
    public async Task<CallRequest?> RequestCallAsync(
        [Service] ICallService callService,
        [Service] IUserService userService,
        [Service] ITopicService topicService,
        string topicId,
        string fromUserId)
    {
        var callRequest = await callService.CreateCallRequestAsync(topicId, fromUserId);
        if (callRequest != null)
        {
            // Populate navigation properties
            callRequest.From = await userService.GetUserByIdAsync(fromUserId);
            callRequest.Topic = await topicService.GetTopicByIdAsync(topicId);
            if (callRequest.Topic != null)
            {
                callRequest.Topic.Author = await userService.GetUserByIdAsync(callRequest.Topic.AuthorId);
            }
        }

        return callRequest;
    }

    // Accept a call request (simplified without auth for now)
    public async Task<bool> AcceptCallAsync(
        [Service] ICallService callService,
        string requestId,
        string userId)
    {
        var callRequest = await callService.AcceptCallRequestAsync(requestId, userId);
        return callRequest != null;
    }

    // Reject a call request (simplified without auth for now)
    public async Task<bool> RejectCallAsync(
        [Service] ICallService callService,
        string requestId,
        string userId)
    {
        var callRequest = await callService.RejectCallRequestAsync(requestId, userId);
        return callRequest != null;
    }
}

public class Subscription
{
    // Placeholder for subscriptions - would be implemented with proper SignalR integration
    public async Task<string> PlaceholderAsync() => "Subscriptions coming soon";
}