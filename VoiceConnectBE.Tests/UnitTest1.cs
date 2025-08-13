using VoiceConnectBE.Models;

namespace VoiceConnectBE.Tests;

public class ModelsTests
{
    [Fact]
    public void User_Creation_Should_Set_Default_Values()
    {
        var user = new User
        {
            Id = "test-id",
            PhoneNumber = "+1234567890",
            DisplayName = "Test User"
        };

        Assert.Equal("test-id", user.Id);
        Assert.Equal("+1234567890", user.PhoneNumber);
        Assert.Equal("Test User", user.DisplayName);
        Assert.True(user.IsActive);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Topic_Creation_Should_Set_Default_Values()
    {
        var topic = new Topic
        {
            Id = "topic-id",
            Title = "Test Topic",
            Description = "Test Description",
            CreatedBy = "user-id"
        };

        Assert.Equal("topic-id", topic.Id);
        Assert.Equal("Test Topic", topic.Title);
        Assert.Equal("Test Description", topic.Description);
        Assert.Equal("user-id", topic.CreatedBy);
        Assert.True(topic.IsActive);
        Assert.Equal(0, topic.BoostCount);
        Assert.True(topic.CreatedAt <= DateTime.UtcNow);
    }
}