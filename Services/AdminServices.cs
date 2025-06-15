using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.Azure.Cosmos;
using Models;
using Helpers;
using System.Text.Json.Serialization;
using System.Text.Json;
using FormagenAPI.Services;
using System.Net;
using FormagenAPI.Exceptions;

namespace Services;

public class AdminService : IAdminService
{
    private readonly Container _adminSessionContainer;
    private readonly FormStoreDatabaseSettings _formStoreDatabaseSettings;

    private readonly EmailServiceSettings _emailServiceSettings;

    private readonly HttpClient _emailClient;

    public record EmailServicePayload(string email, string otp);

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

        _emailClient = new HttpClient();
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

            var payload = new StringContent(JsonSerializer.Serialize(new EmailServicePayload(email, otp)));

            // trigger send otp service
            await _emailClient.PostAsync(_emailServiceSettings.FlowURI, payload);
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

    public async Task<AdminSession> GetSessionByIdAsync(string sessionId)
    {
        try
        {
            var session = await _adminSessionContainer.ReadItemAsync<AdminSession>(sessionId, new PartitionKey(sessionId));
            return session;
        }
        catch (CosmosException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AdminSessionNotFoundException("Sessions is not found", ex);
            }
            else
            {
                throw new UnexpectedCosmosException("Cosmos Exception", ex);
            }
        }
    }
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