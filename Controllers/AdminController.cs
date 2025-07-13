using System.Net;
using DTOs.Admin;
using FormagenAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Models.Admin;

namespace FormagenAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService formsService)
    {
        _adminService = formsService;
    }

    [HttpPost("otp")]
    public async Task<IActionResult> SendOTP(SendAdminOTPRequest sendOTPRequest)
    {
        var sentOtp = await _adminService.SendOTPAsync(sendOTPRequest.Email);

        return Ok(sentOtp);
    }

    [HttpPost("verifyOtp")]
    public async Task<IActionResult> VerifyOTP(VerifyAdminOTPRequest verifyOTPRequest)
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

    [AdminAuthorizeSession]
    [HttpGet("user")]
    public IActionResult GetAdminUser()
    {
        var session = HttpContext.Items["Session"] as AdminSession;
        AdminSessionResponse adminSessionResponse = new()
        {
            Email = session!.Email
        };
        return Ok(adminSessionResponse);
    }

    [AdminAuthorizeSession]
    [HttpPost("user")]
    public async Task<IActionResult> CreateAdminUser(CreateAdminUser createUserRequest)
    {
        var user = await _adminService.CreateUserAsync(createUserRequest);
        return Ok(user);
    }

    [AdminAuthorizeSession]
    [HttpGet("users")]
    public async Task<IActionResult> GetAdminUsers()
    {
        var users = await _adminService.GetUsersAsync();
        return Ok(users);
    }

    [AdminAuthorizeSession]
    [HttpDelete("user/{id}")]
    public async Task<IActionResult> DeleteAdminUser(string id)
    {
        var formResponse = await _adminService.DeleteUserAsync(id);
        return Ok(formResponse);
    }

    [AdminAuthorizeSession]
    [HttpPost("updateUser")]
    public async Task<IActionResult> UpdateAdminUser(UpdateAdminUser updateUserRequest)
    {

        var formResponse = await _adminService.UpdateUserAsync(updateUserRequest);
        return Ok(formResponse);
    }





}
