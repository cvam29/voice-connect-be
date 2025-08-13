using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VoiceConnect.Backend.Models;
using VoiceConnect.Backend.Services;

namespace VoiceConnect.Backend.SignalR;

public class VoiceConnectHub : Hub
{
    private readonly ILogger<VoiceConnectHub> _logger;
    private readonly IAuthService _authService;

    public VoiceConnectHub(ILogger<VoiceConnectHub> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    // Join a user-specific group for receiving messages and call requests
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        _logger.LogInformation($"Connection {Context.ConnectionId} joined user group {userId}");
    }

    // Leave a user-specific group
    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        _logger.LogInformation($"Connection {Context.ConnectionId} left user group {userId}");
    }

    // Join a call-specific group for WebRTC signaling
    public async Task JoinCallGroup(string callId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"call_{callId}");
        _logger.LogInformation($"Connection {Context.ConnectionId} joined call group {callId}");
    }

    // Leave a call-specific group
    public async Task LeaveCallGroup(string callId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"call_{callId}");
        _logger.LogInformation($"Connection {Context.ConnectionId} left call group {callId}");
    }

    // Send WebRTC signaling data (SDP offers, answers, ICE candidates)
    public async Task SendWebRtcSignal(WebRtcSignal signal)
    {
        try
        {
            _logger.LogInformation($"WebRTC signal from {signal.FromUserId} to {signal.ToUserId} in call {signal.CallId}: {signal.Type}");
            
            // Send to specific user
            await Clients.Group($"user_{signal.ToUserId}")
                .SendAsync("WebRtcSignal", signal);
            
            // Also send to call group for redundancy
            await Clients.Group($"call_{signal.CallId}")
                .SendAsync("WebRtcSignal", signal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send WebRTC signal: {signal.CallId}");
        }
    }

    // Notify when a call ends
    public async Task EndCall(string callId, string userId)
    {
        try
        {
            _logger.LogInformation($"Call {callId} ended by user {userId}");
            
            await Clients.Group($"call_{callId}")
                .SendAsync("CallEnded", new { callId, endedBy = userId, endedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to end call: {callId}");
        }
    }
}

// Extension methods for easier SignalR integration with Azure Functions
public static class SignalRExtensions
{
    public static async Task SendMessageToUserAsync(
        this IHubContext<VoiceConnectHub> hubContext, 
        string userId, 
        Message message)
    {
        await hubContext.Clients.Group($"user_{userId}")
            .SendAsync("MessageReceived", message);
    }

    public static async Task SendCallRequestToUsersAsync(
        this IHubContext<VoiceConnectHub> hubContext, 
        CallRequest callRequest)
    {
        // Send to all online users who can respond to this call request
        await hubContext.Clients.All
            .SendAsync("CallRequestReceived", callRequest);
    }

    public static async Task SendCallResponseAsync(
        this IHubContext<VoiceConnectHub> hubContext,
        string userId,
        CallRequest callRequest)
    {
        await hubContext.Clients.Group($"user_{userId}")
            .SendAsync("CallRequestResponse", callRequest);
    }
}