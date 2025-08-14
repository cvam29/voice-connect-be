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

## Mutation Operations

### register
Register a new user with username and email address.

```graphql
mutation Register($username: String!, $email: String!) {
  register(username: $username, email: $email) {
    id
    username
    email
    createdAt
  }
}
```

### login
Login with email address and OTP code.

```graphql
mutation Login($email: String!, $otp: String!) {
  login(email: $email, otp: $otp) {
    token
    user {
      id
      username
      email
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
  email: String!
  createdAt: DateTime!
  lastLoginAt: DateTime
  isActive: Boolean!
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
```