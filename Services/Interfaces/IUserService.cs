using DTOs.User;
using Microsoft.Azure.Cosmos;
using Models;
using User = Models.User.User;


namespace FormagenAPI.Services
{
    public interface IUserService
    {
        Task<bool> SendOTPAsync(string email);

        Task<(bool, Session?)> VerifyOTPAsync(string email, string otp);

        Task<Session> GetSessionByIdAsync(string sessionId);

        Task<User> GetUserByIdAsync(string id);

        Task<User> CreateUserAsync(CreateUser user);

        Task<List<User>> GetUsersAsync();

        Task<bool> DeleteUserAsync(string userId);

        Task<ItemResponse<User>> UpdateUserAsync(UpdateUser updateRequest);
    }
}
