using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.Azure.Cosmos;
using Models;
using Helpers;
using System.Text.Json.Serialization;

namespace Services;

public class AdminService
{
    private readonly Container _adminSessionContainer;
    private readonly FormStoreDatabaseSettings _formStoreDatabaseSettings;

    private readonly EmailServiceSettings _emailServiceSettings;

    private readonly HttpClient _emailClient;

    public AdminService(
        IOptions<FormStoreDatabaseSettings> formStoreDatabaseSettings,
        IOptions<EmailServiceSettings> emailServiceSettings
        )
    {

        CosmosClient cosmosClient = new(
            formStoreDatabaseSettings.Value.ConnectionString,
            new CosmosClientOptions
            {

                UseSystemTextJsonSerializerWithOptions = new System.Text.Json.JsonSerializerOptions()
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                }

            }

        );

        using HttpClient _emailClient = new();
        _emailClient.BaseAddress = new Uri(emailServiceSettings.Value.EmailFlowURI);
        _emailServiceSettings = emailServiceSettings.Value;


        Database database = cosmosClient.GetDatabase(formStoreDatabaseSettings.Value.DatabaseName);

        _adminSessionContainer = database.GetContainer(
           formStoreDatabaseSettings.Value.AdminSessionCollectionName);

        _formStoreDatabaseSettings = formStoreDatabaseSettings.Value;


    }

    public async Task<bool> SendOTPAsync(string email)
    {
        var (userExists, session) = await this.VerifyEmailExistsAsync(email);
        bool otpSent = false;

        if (userExists)
        {
            // prepare otp
            var otp = OtpGenerator.GenerateOtp();
            // replace current otp
            session!.OTP = otp;
            await CreateSessionAsync(session);

            // trigger send otp service
            // await _emailClient.PostAsync("","");
            otpSent = true;
        }
        else
        {
            // tell the user that account does not exist
        }

        return otpSent;
    }

    public async Task<(bool, AdminSession?)> VerifyOTPAsync(string email, string otp)
    {
        var (userExists, session) = await this.VerifyEmailExistsAsync(email);

        if (userExists)
        {
            if (session!.OTP.ToLower() == otp.ToLower())
            {
                return (true, session);
            }
        }

        return (false, null);
    }

    public async Task CreateSessionAsync(AdminSession newAdminSession) => await _adminSessionContainer.UpsertItemAsync<AdminSession>(newAdminSession);

    private async Task<(bool, AdminSession?)> VerifyEmailExistsAsync(string email)
    {
        // check if the admin session store that the user exists
        string userByEmailQuery = $"SELECT * FROM {_formStoreDatabaseSettings.AdminSessionCollectionName} f WHERE f.userEmail = @userEmail";

        var query = new QueryDefinition(userByEmailQuery)
            .WithParameter("@userEmail", email.ToLower());

        using FeedIterator<AdminSession> feed = _adminSessionContainer.GetItemQueryIterator<AdminSession>(
               queryDefinition: query
            );

        FeedResponse<AdminSession> response = await feed.ReadNextAsync();

        return (response.ToList().Count > 0, response.FirstOrDefault());
    }



}