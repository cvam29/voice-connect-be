# GraphQL Schema

## Query Operations

### getTopics
Get all topics with optional filtering for boosted topics only.

```graphql
query GetTopics($boostedOnly: Boolean) {
  topics(boostedOnly: $boostedOnly) {
    id
    text
    boosted
    createdAt
    author {
      id
      username
      bio
      profilePictureUrl
    }
  }
}
```

### getUsers  
Get all users (simplified implementation).

```graphql
query GetUsers {
  users {
    id
    username
    bio
    profilePictureUrl
    createdAt
  }
}
```

### getReports
Get reports with optional status filter (for moderators).

```graphql
query GetReports($status: ReportStatus) {
  reports(status: $status) {
    id
    type
    targetId
    reason
    description
    status
    createdAt
    reviewedAt
    resolutionNotes
    reporter {
      id
      username
    }
    reviewedByUser {
      id
      username
    }
  }
}
```

### getModerators
Get all users with moderator role.

```graphql
query GetModerators {
  moderators {
    id
    username
    bio
    role
    createdAt
  }
}
```

## Mutation Operations

### register
Register a new user with username and phone number.

```graphql
mutation Register($username: String!, $phone: String!) {
  register(username: $username, phone: $phone) {
    id
    username
    phone
    createdAt
  }
}
```

### login
Login with phone number and OTP code.

```graphql
mutation Login($phone: String!, $otp: String!) {
  login(phone: $phone, otp: $otp) {
    token
    user {
      id
      username
      phone
      bio
      profilePictureUrl
    }
  }
}
```

### createTopic
Create a new topic.

```graphql
mutation CreateTopic($text: String!, $authorId: String!) {
  createTopic(text: $text, authorId: $authorId) {
    id
    text
    boosted
    createdAt
    author {
      id
      username
    }
  }
}
```

### boostTopic
Boost a topic to make it more visible.

```graphql
mutation BoostTopic($topicId: String!, $userId: String!) {
  boostTopic(topicId: $topicId, userId: $userId) {
    id
    text
    boosted
    boostedUntil
  }
}
```

### favoriteUser
Add a user to favorites.

```graphql
mutation FavoriteUser($userId: String!, $favoriteUserId: String!) {
  favoriteUser(userId: $userId, favoriteUserId: $favoriteUserId)
}
```

### sendMessage
Send a message to another user.

```graphql
mutation SendMessage($fromUserId: String!, $toUserId: String!, $content: String!) {
  sendMessage(fromUserId: $fromUserId, toUserId: $toUserId, content: $content) {
    id
    content
    sentAt
    sender {
      id
      username
    }
    recipient {
      id
      username
    }
  }
}
```

### requestCall
Request a voice call based on a topic.

```graphql
mutation RequestCall($topicId: String!, $fromUserId: String!) {
  requestCall(topicId: $topicId, fromUserId: $fromUserId) {
    id
    status
    createdAt
    topic {
      id
      text
      author {
        id
        username
      }
    }
    from {
      id
      username
    }
  }
}
```

### acceptCall
Accept a call request.

```graphql
mutation AcceptCall($requestId: String!, $userId: String!) {
  acceptCall(requestId: $requestId, userId: $userId)
}
```

### rejectCall
Reject a call request.

```graphql
mutation RejectCall($requestId: String!, $userId: String!) {
  rejectCall(requestId: $requestId, userId: $userId)
}
```

### createReport
Create a report for content moderation.

```graphql
mutation CreateReport($reporterId: String!, $input: CreateReportInput!) {
  createReport(reporterId: $reporterId, input: $input) {
    id
    type
    targetId
    reason
    description
    status
    createdAt
    reporter {
      id
      username
    }
  }
}
```

### assignModerator
Assign moderator role to a user (admin only).

```graphql
mutation AssignModerator($assignedBy: String!, $input: AssignModeratorInput!) {
  assignModerator(assignedBy: $assignedBy, input: $input)
}
```

### banUser
Ban a user (moderator/admin only).

```graphql
mutation BanUser($moderatorId: String!, $input: BanUserInput!) {
  banUser(moderatorId: $moderatorId, input: $input)
}
```

### unbanUser
Unban a user (moderator/admin only).

```graphql
mutation UnbanUser($moderatorId: String!, $userId: String!) {
  unbanUser(moderatorId: $moderatorId, userId: $userId)
}
```

### resolveReport
Resolve a report (moderator/admin only).

```graphql
mutation ResolveReport($moderatorId: String!, $input: ResolveReportInput!) {
  resolveReport(moderatorId: $moderatorId, input: $input) {
    id
    status
    reviewedAt
    resolutionNotes
    reviewedByUser {
      id
      username
    }
  }
}
```

## Subscription Operations

### placeholder
Placeholder for future subscription implementation.

```graphql
subscription {
  placeholder
}
```

## GraphQL Types

### User
```graphql
type User {
  id: ID!
  username: String!
  bio: String
  profilePictureUrl: String
  phone: String!
  createdAt: DateTime!
  lastLoginAt: DateTime
  isActive: Boolean!
  role: UserRole!
  isBanned: Boolean!
  bannedAt: DateTime
  bannedUntil: DateTime
  bannedBy: String
  banReason: String
  favoriteUserIds: [String!]!
}
```

### Topic
```graphql
type Topic {
  id: ID!
  text: String!
  authorId: String!
  boosted: Boolean!
  boostedUntil: DateTime
  createdAt: DateTime!
  author: User
}
```

### Message
```graphql
type Message {
  id: ID!
  senderId: String!
  recipientId: String!
  content: String!
  sentAt: DateTime!
  isRead: Boolean!
  sender: User
  recipient: User
}
```

### CallRequest
```graphql
type CallRequest {
  id: ID!
  topicId: String!
  fromUserId: String!
  toUserId: String
  status: CallRequestStatus!
  createdAt: DateTime!
  respondedAt: DateTime
  topic: Topic
  from: User
  to: User
}
```

### Report
```graphql
type Report {
  id: ID!
  reporterId: String!
  type: ReportType!
  targetId: String!
  reason: String!
  description: String
  status: ReportStatus!
  createdAt: DateTime!
  reviewedAt: DateTime
  reviewedBy: String
  resolutionNotes: String
  reporter: User
  reviewedByUser: User
}
```

### AuthPayload
```graphql
type AuthPayload {
  token: String!
  user: User!
}
```

### Enums

```graphql
enum CallRequestStatus {
  PENDING
  ACCEPTED
  REJECTED
  CANCELLED
  EXPIRED
}

enum UserRole {
  USER
  MODERATOR
  ADMIN
}

enum ReportType {
  USER
  TOPIC
  MESSAGE
}

enum ReportStatus {
  PENDING
  UNDER_REVIEW
  RESOLVED
  DISMISSED
}
```

### Input Types

```graphql
input CreateReportInput {
  type: ReportType!
  targetId: String!
  reason: String!
  description: String
}

input BanUserInput {
  userId: String!
  reason: String!
  bannedUntil: DateTime
}

input AssignModeratorInput {
  userId: String!
}

input ResolveReportInput {
  reportId: String!
  resolutionNotes: String
  status: ReportStatus!
}
```