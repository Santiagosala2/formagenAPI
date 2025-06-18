using DTOs;
using Microsoft.Azure.Cosmos;
using Models;

namespace FormagenAPI.Services
{
    public interface IAdminService
    {
        Task<bool> SendOTPAsync(string email);

        Task<(bool, AdminSession?)> VerifyOTPAsync(string email, string otp);

        Task<AdminSession> GetSessionByIdAsync(string sessionId);

        Task<AdminUser> CreateUserAsync(CreateUser user);
    }
}
