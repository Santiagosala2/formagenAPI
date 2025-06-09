using System.ComponentModel.DataAnnotations;
using System.Net;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Models;
using Services;

namespace FormagenAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AdminServices _adminService;

    public AdminController(AdminServices formsService)
    {
        _adminService = formsService;
    }

    public record ErrorMessage(string message, HttpStatusCode statusCode);




    [HttpPost("otp")]
    public async Task<IActionResult> SendOTP(SendOTPRequest sendOTPRequest)
    {
        var sentOtp = await _adminService.SendOTPAsync(sendOTPRequest.UserEmail);

        return Ok(sentOtp);
    }

    [HttpPost("verifyOtp")]
    public async Task<IActionResult> VerifyOTP(VerifyOTPRequest verifyOTPRequest)
    {
        var (passOtp, session) = await _adminService.VerifyOTPAsync(verifyOTPRequest.Email, verifyOTPRequest.OTP);

        if (passOtp)
        {
            Response.Cookies.Append("SessionId", session!.Id, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(2)
            });
        }

        return Ok(passOtp);
    }




}
