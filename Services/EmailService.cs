
using System.Text.Json;
using FormagenAPI.Services;
using Microsoft.Extensions.Options;
using Models;

namespace Services;

public class EmailService : IEmailService
{

    private readonly EmailServiceSettings _emailServiceSettings;

    private readonly HttpClient _emailClient;


    public EmailService(IOptions<EmailServiceSettings> emailServiceSettings)
    {
        _emailClient = new HttpClient();
        _emailServiceSettings = emailServiceSettings.Value;
    }

    public async Task SendOTPEmail(string email, string otp)
    {

        var payload = new StringContent(JsonSerializer.Serialize(new EmailServiceRequest
        {
            Email = email,
            Type = "otp",
            Payload = new Payload
            {
                Otp = otp,
            }
        }));
        await SendRequest(payload);
    }

    public async Task SendShareEmail(string formId, string email, string userName)
    {

        var payload = new StringContent(JsonSerializer.Serialize(new EmailServiceRequest
        {
            Email = email,
            Type = "share",
            Payload = new Payload
            {
                Link = $"https://localhost:3000/submit/{formId}",
                UserName = userName
            }
        }));

        await SendRequest(payload);
    }


    private async Task SendRequest(StringContent body)
    {
        await _emailClient.PostAsync(_emailServiceSettings.FlowURI, body);
    }



}


public class Payload
{
    public string? Otp { get; set; } = "";
    public string? Link { get; set; } = "";
    public string? UserName { get; set; } = "";
}

public class EmailServiceRequest
{
    public required string Email { get; set; }
    public required string Type { get; set; }
    public required Payload Payload { get; set; }
}