namespace VoiceConnect.Backend.Models;

public class AuthPayload
{
    public string Token { get; set; } = string.Empty;
    public User User { get; set; } = default!;
}

public class RegisterInput
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class LoginInput
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class CreateTopicInput
{
    public string Text { get; set; } = string.Empty;
}

public class SendMessageInput
{
    public string ToUserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class RequestCallInput
{
    public string TopicId { get; set; } = string.Empty;
}

public class WebRtcSignal
{
    public string CallId { get; set; } = string.Empty;
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "offer", "answer", "ice-candidate"
    public object? Data { get; set; }
}