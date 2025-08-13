using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using VoiceConnect.Backend.Services;

namespace VoiceConnect.Backend.Functions;

public class AuthFunction
{
    private readonly ILogger<AuthFunction> _logger;
    private readonly IAuthService _authService;

    public AuthFunction(ILogger<AuthFunction> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    [Function("SendOtp")]
    public async Task<HttpResponseData> SendOtpAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/send-otp")] HttpRequestData req)
    {
        _logger.LogInformation("Send OTP request received");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var sendOtpRequest = System.Text.Json.JsonSerializer.Deserialize<SendOtpRequest>(requestBody);

            if (sendOtpRequest == null || string.IsNullOrEmpty(sendOtpRequest.Phone))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("{\"message\":\"Phone number is required\"}");
                return badResponse;
            }

            var success = await _authService.SendOtpAsync(sendOtpRequest.Phone);
            
            if (success)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("{\"message\":\"OTP sent successfully\"}");
                return response;
            }
            else
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("{\"message\":\"Failed to send OTP\"}");
                return errorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("{\"message\":\"Internal server error\"}");
            return response;
        }
    }
}

public class SendOtpRequest
{
    public string Phone { get; set; } = string.Empty;
}