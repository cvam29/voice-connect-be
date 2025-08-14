using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Worker;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Data;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.SignalR;
using VoiceConnect.Backend.Data;
using VoiceConnect.Backend.Services;
using VoiceConnect.Backend.GraphQL;
using VoiceConnect.Backend.SignalR;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // Add Application Insights telemetry - commented out for now, add packages if needed
        // services.AddApplicationInsightsTelemetryWorkerService();
        // services.ConfigureFunctionsApplicationInsights();

        // Configure Entity Framework with In-Memory Database for development
        services.AddDbContext<VoiceConnectDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("CosmosDbConnectionString") 
                ?? configuration["CosmosDbConnectionString"];
            
            if (!string.IsNullOrEmpty(connectionString) && !connectionString.Contains("localhost:8081"))
            {
                options.UseCosmos(connectionString, "VoiceConnectDb");
            }
            else
            {
                // Use in-memory database for development
                options.UseInMemoryDatabase("VoiceConnectDb");
            }
        });

        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITopicService, TopicService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<ICallService, CallService>();

        // Configure GraphQL
        services
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddSubscriptionType<Subscription>()
            .AddInMemorySubscriptions()
            .ModifyRequestOptions(opt =>
            {
                opt.IncludeExceptionDetails = true;
            });

        // Add configuration
        services.AddSingleton<IConfiguration>(configuration);

        // Add logging
        services.AddLogging();
    })
    .Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VoiceConnectDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not ensure database creation - this may be normal in development");
    }
}

host.Run();