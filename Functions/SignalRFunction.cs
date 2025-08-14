using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace VoiceConnect.Backend.Functions;

public class SignalRFunction
{
    private readonly ILogger<SignalRFunction> _logger;

    public SignalRFunction(ILogger<SignalRFunction> logger)
    {
        _logger = logger;
    }

    [Function("SignalRNegotiate")]
    public async Task<HttpResponseData> NegotiateAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "signalr/negotiate")] HttpRequestData req)
    {
        _logger.LogInformation("SignalR negotiate request received");

        try
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            // For development, return a simple response
            // In production, this would integrate with Azure SignalR Service
            var negotiateResponse = new
            {
                url = "ws://localhost:7071/api/signalr",
                accessToken = "development-token"
            };
            
            await response.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(negotiateResponse));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SignalR negotiation");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("{\"message\":\"Negotiation failed\"}");
            return response;
        }
    }
}

public class BroadcastRequest
{
    public string Message { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
}