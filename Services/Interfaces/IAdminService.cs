using DTOs.Admin;
using Microsoft.Azure.Cosmos;
using Models.Admin;
using Models;

namespace FormagenAPI.Services
{
    public interface IAdminService
    {
        Task<bool> SendOTPAsync(string email);

        Task<(bool, Session?)> VerifyOTPAsync(string email, string otp);

        Task<Session> GetSessionByIdAsync(string sessionId);

        Task<AdminUser> CreateUserAsync(CreateAdminUserRequest user);

        Task<AdminUser> GetUserByIdAsync(string id);

        Task<List<AdminUser>> GetUsersAsync();

        Task<bool> DeleteUserAsync(string userId);

        Task<ItemResponse<AdminUser>> UpdateUserAsync(UpdateAdminUser updateRequest);
    }
}
