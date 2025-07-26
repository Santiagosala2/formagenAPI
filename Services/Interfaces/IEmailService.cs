using DTOs.Form;
using Microsoft.Azure.Cosmos;
using Models;
using Models.Form;

namespace FormagenAPI.Services
{
    public interface IEmailService
    {
        Task SendShareEmail(string formId, string email, string userName);

        Task SendOTPEmail(string email, string otp);


    }
}
