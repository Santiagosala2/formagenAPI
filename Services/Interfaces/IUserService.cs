using DTOs.User;
using Microsoft.Azure.Cosmos;
using Models;

namespace FormagenAPI.Services
{
    public interface IUserService
    {
        Task<bool> SendOTPAsync(string email);

        Task<(bool, Session?)> VerifyOTPAsync(string email, string otp);

        Task<Session> GetSessionByIdAsync(string sessionId);

        Task<Models.User.User> GetUserByIdAsync(string id);

        Task<Models.User.User> CreateUserAsync(CreateUser user);

        Task<List<Models.User.User>> GetUsersAsync();

        Task<bool> DeleteUserAsync(string userId);

        Task<ItemResponse<Models.User.User>> UpdateUserAsync(UpdateUser updateRequest);
    }
}
