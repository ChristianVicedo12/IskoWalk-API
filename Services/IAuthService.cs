using IskoWalkAPI.Models;

namespace IskoWalkAPI.Services
{
    public interface IAuthService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<string> GenerateJwtTokenAsync(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
