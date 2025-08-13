using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace VoiceConnectBE.Functions
{
    public class GraphQLFunction
    {
        private readonly ILogger<GraphQLFunction> _logger;

        public GraphQLFunction(ILogger<GraphQLFunction> logger)
        {
            _logger = logger;
        }

        [Function("GraphQL")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "graphql")] HttpRequestData req)
        {
            _logger.LogInformation("GraphQL endpoint triggered.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            await response.WriteStringAsync("{\"data\": {\"hello\": \"Welcome to Voice Connect GraphQL API!\"}}");
            return response;
        }
    }
}