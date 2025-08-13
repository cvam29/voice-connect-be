using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using HotChocolate;
using HotChocolate.Execution;
using System.Text;
using System.Text.Json;
using VoiceConnect.Backend.Services;

namespace VoiceConnect.Backend.Functions;

public class GraphQLFunction
{
    private readonly ILogger<GraphQLFunction> _logger;
    private readonly IRequestExecutor _executor;

    public GraphQLFunction(ILogger<GraphQLFunction> logger, IRequestExecutor executor)
    {
        _logger = logger;
        _executor = executor;
    }

    [Function("GraphQL")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "graphql")] HttpRequestData req)
    {
        _logger.LogInformation("GraphQL request received");

        try
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            if (req.Method == "GET")
            {
                // Handle GraphQL Playground request
                var playground = GetGraphQLPlaygroundHtml();
                await response.WriteStringAsync(playground, Encoding.UTF8);
                response.Headers.Remove("Content-Type");
                response.Headers.Add("Content-Type", "text/html; charset=utf-8");
                return response;
            }

            // Handle GraphQL POST request
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            var graphQLRequest = JsonSerializer.Deserialize<GraphQLHttpRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (graphQLRequest == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("{\"errors\":[{\"message\":\"Invalid request format\"}]}");
                return response;
            }

            // Create GraphQL request context
            var requestBuilder = new QueryRequestBuilder()
                .SetQuery(graphQLRequest.Query);

            if (graphQLRequest.Variables != null)
            {
                requestBuilder.SetVariableValues(graphQLRequest.Variables);
            }

            if (!string.IsNullOrEmpty(graphQLRequest.OperationName))
            {
                requestBuilder.SetOperation(graphQLRequest.OperationName);
            }

            // Add HTTP context properties
            requestBuilder.AddGlobalState("HttpContext", req);

            var graphQLQuery = requestBuilder.Create();
            var result = await _executor.ExecuteAsync(graphQLQuery);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteStringAsync(json);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GraphQL request");
            
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync("{\"errors\":[{\"message\":\"Internal server error\"}]}");
            return response;
        }
    }

    private string GetGraphQLPlaygroundHtml()
    {
        return """
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>GraphQL Playground</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link
        rel="stylesheet"
        href="//cdn.jsdelivr.net/npm/graphql-playground-react/build/static/css/index.css"
    />
    <link
        rel="shortcut icon"
        href="//cdn.jsdelivr.net/npm/graphql-playground-react/build/favicon.png"
    />
    <script
        src="//cdn.jsdelivr.net/npm/graphql-playground-react/build/static/js/middleware.js"
    ></script>
</head>
<body>
    <div id="root">
        <style>
            body {
                background-color: rgb(23, 42, 58);
                font-family: Open Sans, sans-serif;
                height: 90vh;
            }
            #root {
                height: 100%;
                width: 100%;
                display: flex;
                align-items: center;
                justify-content: center;
            }
            .loading {
                font-size: 32px;
                font-weight: 200;
                color: rgba(255, 255, 255, .6);
                margin-left: 20px;
            }
            img {
                width: 78px;
                height: 78px;
            }
            .title {
                font-weight: 400;
            }
        </style>
        <img
            src="//cdn.jsdelivr.net/npm/graphql-playground-react/build/logo.png"
            alt=""
        />
        <div class="loading"> Loading
            <span class="title">GraphQL Playground</span>
        </div>
    </div>
    <script>
        window.addEventListener('load', function (event) {
            GraphQLPlayground.init(document.getElementById('root'), {
                endpoint: '/api/graphql'
            })
        })
    </script>
</body>
</html>
""";
    }
}

public class GraphQLHttpRequest
{
    public string Query { get; set; } = string.Empty;
    public Dictionary<string, object>? Variables { get; set; }
    public string? OperationName { get; set; }
}