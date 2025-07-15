using DTOs.Admin;
using Microsoft.Azure.Cosmos;
using Models.Admin;

namespace FormagenAPI.Services
{
    public interface IAdminService
    {
        Task<bool> SendOTPAsync(string email);

        Task<(bool, AdminSession?)> VerifyOTPAsync(string email, string otp);

        Task<AdminSession> GetSessionByIdAsync(string sessionId);

        Task<AdminUser> CreateUserAsync(CreateAdminUserRequest user);

        Task<List<AdminUser>> GetUsersAsync();

        Task<bool> DeleteUserAsync(string userId);

        Task<ItemResponse<AdminUser>> UpdateUserAsync(UpdateAdminUser updateRequest);
    }
}
