using VoiceConnectBE.Models;

namespace VoiceConnectBE.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(string id);
        Task<User> CreateUserAsync(string phoneNumber, string displayName, string? email = null);
        Task<bool> UpdateUserAsync(User user);
    }

    public class UserService : IUserService
    {
        // This is a mock implementation for demonstration
        private readonly List<User> _users = new();

        public async Task<User?> GetUserByIdAsync(string id)
        {
            await Task.Delay(1); // Simulate async operation
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public async Task<User> CreateUserAsync(string phoneNumber, string displayName, string? email = null)
        {
            await Task.Delay(1); // Simulate async operation
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                PhoneNumber = phoneNumber,
                DisplayName = displayName,
                Email = email
            };
            _users.Add(user);
            return user;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            await Task.Delay(1); // Simulate async operation
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.DisplayName = user.DisplayName;
                existingUser.Email = user.Email;
                existingUser.ProfilePictureUrl = user.ProfilePictureUrl;
                existingUser.LastActiveAt = DateTime.UtcNow;
                return true;
            }
            return false;
        }
    }
}