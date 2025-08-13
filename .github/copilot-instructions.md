# Voice Connect Backend - Development Instructions

**ALWAYS FOLLOW THESE INSTRUCTIONS FIRST.** Only search for additional context or use bash commands if the information below is incomplete or incorrect.

Voice Connect Backend is a social voice + chat app backend built with .NET 8 Azure Functions, GraphQL (HotChocolate), and Azure services. This runs as an isolated process Azure Function with GraphQL endpoints for real-time voice and chat features.

## Working Effectively

### Prerequisites and Setup
- Install Azure Functions Core Tools manually since npm installation fails in restricted environments:
  ```bash
  cd /tmp
  wget https://github.com/Azure/azure-functions-core-tools/releases/download/4.0.6280/Azure.Functions.Cli.linux-x64.4.0.6280.zip
  unzip -q Azure.Functions.Cli.linux-x64.4.0.6280.zip
  chmod +x /tmp/func
  export PATH="/tmp:$PATH"
  ```

### Build and Run Operations
- **NEVER CANCEL** builds or long-running commands. Set timeout to 60+ minutes for builds, 30+ minutes for tests.

- **Build the application**:
  ```bash
  dotnet build
  ```
  - Takes approximately 30 seconds on first build after clean
  - Subsequent builds are faster (3-5 seconds)
  - **NEVER CANCEL** - Set timeout to 60+ minutes

- **Clean build artifacts**:
  ```bash
  dotnet clean
  ```
  - Takes less than 1 second

- **Restore dependencies**:
  ```bash
  dotnet restore
  ```
  - Cold restore (first time): ~1.5 seconds  
  - Warm restore: ~1 second

### Running the Application
- **Start the Azure Functions host**:
  ```bash
  export PATH="/tmp:$PATH"
  func start
  ```
  - Takes 10-15 seconds to start
  - Serves GraphQL endpoint at: http://localhost:7071/api/graphql
  - **NEVER CANCEL** startup process - it may appear to hang but will complete

- **Test the GraphQL endpoint**:
  ```bash
  curl -X GET http://localhost:7071/api/graphql
  curl -X POST http://localhost:7071/api/graphql -H "Content-Type: application/json" -d '{"query":"{ hello }"}'
  ```
  - Both should return: `{"data": {"hello": "Welcome to Voice Connect GraphQL API!"}}`

### Running Tests
- **Run unit tests**:
  ```bash
  cd VoiceConnectBE.Tests
  dotnet test --verbosity normal
  ```
  - Takes approximately 18 seconds to complete
  - **NEVER CANCEL** - Set timeout to 30+ minutes
  - Tests validate model creation and default values

- **Run tests from solution root**:
  ```bash
  dotnet test VoiceConnectBE.Tests/ --verbosity normal
  ```

### Validation Steps
- **ALWAYS test these scenarios after making changes**:
  1. Build succeeds: `dotnet build`
  2. Tests pass: `cd VoiceConnectBE.Tests && dotnet test`
  3. Application starts: `export PATH="/tmp:$PATH" && func start`
  4. GraphQL endpoint responds: `curl -X GET http://localhost:7071/api/graphql`
  5. POST requests work: `curl -X POST http://localhost:7071/api/graphql -H "Content-Type: application/json" -d '{"query":"{ hello }"}'`

## Project Structure

```
.
├── README.md                    # Project overview and description
├── VoiceConnectBE.csproj       # Main project file with dependencies
├── VoiceConnectBE.sln          # Solution file containing both projects
├── Program.cs                  # Azure Functions host entry point
├── host.json                   # Azure Functions host configuration
├── local.settings.json         # Local environment configuration
├── .env.example               # Environment configuration template
├── Functions/                  # Azure Functions endpoints
│   └── GraphQLFunction.cs     # GraphQL HTTP trigger function
├── Models/                     # Data models and GraphQL types
│   └── Models.cs              # User and Topic models
├── Services/                   # Business logic services
│   └── UserService.cs         # User management service (mock implementation)
├── VoiceConnectBE.Tests/      # Unit test project
│   ├── VoiceConnectBE.Tests.csproj
│   └── UnitTest1.cs           # Model validation tests
├── docs/                      # Documentation
│   └── schema.graphql         # GraphQL schema definition
├── Data/                      # Cosmos DB/PostgreSQL context (planned)
├── SignalR/                   # SignalR hubs and bindings (planned)
├── Storage/                   # Azure Blob Storage integration (planned)
└── Notifications/            # Notification Hubs integration (planned)
```

## Key Technologies
- **.NET 8** - Runtime framework
- **Azure Functions (Isolated Process)** - Serverless hosting
- **HotChocolate** - GraphQL implementation
- **Azure Cosmos DB** - Database (configured, not implemented)
- **Azure SignalR Service** - Real-time communication (planned)
- **Azure Blob Storage** - File storage (planned)
- **xUnit** - Testing framework

## Common Issues and Solutions

### Azure Functions Core Tools Installation
- **Issue**: `npm install -g azure-functions-core-tools@4` fails due to network restrictions
- **Solution**: Download and extract manually as shown in Prerequisites

### Build Warnings
- **Warning**: "The '--output' option isn't supported when building a solution"
- **Impact**: This is expected when building the solution file, does not affect functionality

### Test Project Compilation
- **Issue**: Test files being compiled with main project
- **Solution**: Main project excludes `VoiceConnectBE.Tests/**` files via `<Compile Remove>` directive

## Configuration Files

### local.settings.json
Contains local development configuration including:
- Azure Storage connection strings
- Cosmos DB connection strings
- SignalR connection strings
- Authentication keys (development values)

### host.json
Configures Azure Functions runtime behavior including:
- Application Insights logging
- Sampling settings
- Live metrics filters

## Development Workflow
1. Make code changes
2. Run `dotnet build` to verify compilation
3. Run tests: `cd VoiceConnectBE.Tests && dotnet test`
4. Test locally: `export PATH="/tmp:$PATH" && func start`
5. Validate endpoints with curl commands
6. Commit changes

## Important Notes
- **All timings are from Ubuntu 24.04 with .NET 8.0.118**
- **Azure Functions Core Tools version 4.0.6280 required**
- **Solution file exists but individual project builds are preferred**
- **Mock implementations are used for services (no actual Azure resources required for local development)**
- **GraphQL schema is available at `/docs/schema.graphql`**

Always verify that the basic GraphQL endpoint is working after any changes by testing both GET and POST requests to `/api/graphql`.