# Wakie-like Social Voice + Chat App Backend

This is the backend for a social voice + chat app inspired by Wakie, built with .NET 8, Azure Functions, GraphQL (HotChocolate), and Azure services.

## Tech Stack
- **Runtime:** .NET 8 (Isolated Process) on Azure Functions
- **API Layer:** GraphQL (HotChocolate)
- **Database:** Azure Cosmos DB (PostgreSQL API)
- **Auth:** Phone OTP or email login (Azure AD B2C or custom via Twilio/Firebase)
- **Real-time:** Azure SignalR Service for chat & WebRTC signaling
- **Voice Calls:** WebRTC (SDP & ICE exchanged via SignalR; STUN/TURN via Azure Communication Services)
- **Storage:** Azure Blob Storage for profile pictures
- **Notifications:** Azure Notification Hubs for push messages

## Features
- User registration and login (OTP-based)
- Topic creation, boosting, and listing
- Favorite users management
- Real-time chat and WebRTC call signaling
- Push notifications

## GraphQL Schema
See `/docs/schema.graphql` for the full schema.

## Project Structure
- `Functions/` - Azure Functions (GraphQL endpoint, SignalR, HTTP triggers)
- `Services/` - Business logic (UserService, TopicService, etc.)
- `Models/` - Data models and GraphQL types
- `Data/` - Cosmos DB/PostgreSQL context and repositories
- `SignalR/` - SignalR hubs and bindings
- `Storage/` - Azure Blob Storage integration
- `Notifications/` - Notification Hubs integration

## Getting Started
1. **Clone the repo**
2. **Configure environment variables** (see `.env.example`)
3. **Run locally:**
   ```pwsh
   func start
   ```
4. **Deploy to Azure:**
   - Use Azure Functions, Cosmos DB, SignalR, Blob Storage, and Notification Hubs

## Development Notes
- Uses dependency injection for all services
- Timestamps are stored in UTC
- Logging via `ILogger`
- Basic validation and error handling included

## License
MIT
