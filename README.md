# Wakie-like Social Voice + Chat App Backend

This is the backend for a social voice + chat app inspired by Wakie, built with .NET 8, Azure Functions, GraphQL (HotChocolate), and Azure services.

## Tech Stack
- **Runtime:** .NET 8 (Isolated Process) on Azure Functions
- **API Layer:** GraphQL (HotChocolate)
- **Database:** Azure Cosmos DB (PostgreSQL API) / In-Memory for development
- **Auth:** Email OTP (logged to console) + JWT tokens
- **Real-time:** Azure SignalR Service for chat & WebRTC signaling
- **Voice Calls:** WebRTC (SDP & ICE exchanged via SignalR; STUN/TURN via Azure Communication Services)
- **Storage:** Azure Blob Storage for profile pictures
- **Notifications:** Azure Notification Hubs for push messages

## Features
- ✅ User registration and login (OTP-based)
- ✅ Topic creation, boosting, and listing
- ✅ Favorite users management
- ✅ Chat messaging system
- ✅ WebRTC call request system
- ✅ GraphQL API with HotChocolate
- ✅ Azure Functions HTTP triggers
- 🚧 Real-time SignalR integration (basic setup)
- 🚧 JWT authentication (implemented but not enforced in GraphQL)
- ⏳ Push notifications
- ⏳ File upload for profile pictures

## GraphQL Schema
See `/docs/schema.md` for the full schema documentation with example queries.

## Project Structure
- `Functions/` - Azure Functions (GraphQL endpoint, Auth, SignalR)
- `Services/` - Business logic (UserService, TopicService, etc.)
- `Models/` - Data models and GraphQL types
- `Data/` - Database context and repositories
- `SignalR/` - SignalR hubs and real-time messaging
- `GraphQL/` - GraphQL schema and resolvers

## Getting Started

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools v4
- (Optional) Azure Cosmos DB Emulator for local development
- (Optional) Email service for OTP delivery (currently OTPs are logged to console)

### Installation
1. **Clone the repository**
   ```bash
   git clone https://github.com/cvam29/voice-connect-be.git
   cd voice-connect-be
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure environment variables**
   - Copy `.env.example` to `local.settings.json`
   - Update the values with your actual service credentials
   
   For development, the app will use in-memory database if Cosmos DB is not configured.

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run locally**
   ```bash
   func start
   ```

6. **Access GraphQL Playground**
   Open your browser to `http://localhost:7071/api/graphql` to explore the GraphQL schema.

### Example GraphQL Queries

**Register a user:**
```graphql
mutation {
  register(username: "john_doe", email: "john@example.com") {
    id
    username
    email
    createdAt
  }
}
```

**Create a topic:**
```graphql
mutation {
  createTopic(text: "Let's talk about travel!", authorId: "user-id-here") {
    id
    text
    boosted
    createdAt
    author {
      username
    }
  }
}
```

**Get all topics:**
```graphql
query {
  topics {
    id
    text
    boosted
    createdAt
    author {
      username
    }
  }
}
```

### API Endpoints
- `GET/POST /api/graphql` - Main GraphQL endpoint
- `POST /api/auth/send-otp` - Send OTP for registration/login
- `POST /api/signalr/negotiate` - SignalR negotiation endpoint

## Development Notes

### Database
- Uses in-memory database for development by default
- Switch to Azure Cosmos DB by configuring the connection string
- Entity Framework migrations are not used with Cosmos DB

### Authentication
- Email-based OTP registration and login
- JWT tokens for session management
- OTPs are currently logged to console (no email service configured)
- Authentication is implemented but not enforced in all GraphQL operations (simplified for MVP)

### Real-time Features
- SignalR hub setup for WebRTC signaling
- Message broadcasting capabilities
- Call request notifications (basic implementation)

### Error Handling
- Comprehensive logging throughout the application
- Graceful error responses in GraphQL resolvers
- Input validation in service methods

## Deployment

### Azure Resources Required
1. **Azure Functions App** (Consumption or Premium plan)
2. **Azure Cosmos DB** (with PostgreSQL API)
3. **Azure SignalR Service**
4. **Azure Blob Storage** (for file uploads)
5. **Azure Notification Hubs** (for push notifications)

### Deployment Steps
1. Create Azure resources using Azure Portal or ARM templates
2. Configure application settings in Azure Functions
3. Deploy using Azure Functions Core Tools:
   ```bash
   func azure functionapp publish <your-function-app-name>
   ```

## Architecture Decisions

### Why Azure Functions?
- Serverless scaling for variable load
- Cost-effective for startup MVP
- Easy integration with other Azure services
- Built-in authentication and authorization options

### Why GraphQL?
- Single endpoint for all operations
- Strong typing and schema introspection
- Efficient data fetching (no over/under-fetching)
- Built-in playground for API exploration

### Why In-Memory Database for Development?
- Faster local development setup
- No external dependencies for basic development
- Easy to switch to Cosmos DB for production

## Contributing
1. Fork the repository
2. Create a feature branch
3. Make changes and test thoroughly
4. Submit a pull request

## License
MIT License - see LICENSE file for details

## Support
For issues and questions, please create an issue in the GitHub repository.
