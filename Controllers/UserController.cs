using System.Net;
using DTOs.User;
using FormagenAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace FormagenAPI.Controllers;

[ApiController]
[Route("api")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("user/otp")]
    public async Task<IActionResult> SendOTP(SendUserOTPRequest sendOTPRequest)
    {
        var sentOtp = await _userService.SendOTPAsync(sendOTPRequest.Email);

        return Ok(sentOtp);
    }

    [HttpPost("user/verifyOtp")]
    public async Task<IActionResult> VerifyOTP(VerifyOTPRequest verifyOTPRequest)
    {
        var (passOtp, session) = await _userService.VerifyOTPAsync(verifyOTPRequest.Email, verifyOTPRequest.OTP);

        if (passOtp)
        {
            Response.Cookies.Append("UserSessionId", session!.Id, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });
        }

        return Ok(passOtp);
    }

    [UserAuthorizeSession]
    [HttpGet("user")]
    public IActionResult GetUser()
    {
        var session = HttpContext.Items["Session"] as Session;
        UserSessionResponse userSessionResponse = new()
        {
            UserId = session!.UserId,
            Email = session!.Email
        };
        return Ok(userSessionResponse);
    }

    [AdminAuthorizeSession]
    [HttpPost("user")]
    public async Task<IActionResult> CreateUser(CreateUser createUserRequest)
    {
        var user = await _userService.CreateUserAsync(createUserRequest);
        return Ok(user);
    }

    [AdminAuthorizeSession]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetUsersAsync();
        return Ok(users);
    }

    [AdminAuthorizeSession]
    [HttpDelete("user/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var formResponse = await _userService.DeleteUserAsync(id);
        return Ok(formResponse);
    }

    [AdminAuthorizeSession]
    [HttpPost("updateUser")]
    public async Task<IActionResult> UpdateUser(UpdateUser updateUserRequest)
    {

        var formResponse = await _userService.UpdateUserAsync(updateUserRequest);
        return Ok(formResponse);
    }





}
