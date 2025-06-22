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
using DTOs;

namespace Services;

public class AdminService : IAdminService
{
    private readonly Container _adminSessionContainer;

    private readonly Container _adminUserContainer;
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

        _adminUserContainer = database.GetContainer(
           formStoreDatabaseSettings.Value.AdminUserCollectionName);

        _formStoreDatabaseSettings = formStoreDatabaseSettings.Value;


    }

    public async Task<bool> SendOTPAsync(string email)
    {
        var user = await this.GetUserByEmailAsync(email);
        bool otpSent = false;

        if (user is not null)
        {
            // prepare otp
            var otp = OtpGenerator.GenerateOtp();
            // replace current otp
            AdminSession session = new()
            {
                Id = Guid.NewGuid().ToString(),
                OTP = otp,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

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
        var (userExists, session) = await this.GetSessionByEmailAsync(email);

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
                throw new AdminSessionNotFoundException("Session is not found", ex);
            }
            else
            {
                throw new UnexpectedCosmosException("Cosmos Exception", ex);
            }
        }
    }

    public async Task<AdminUser> CreateUserAsync(CreateUser userRequest)
    {
        var userExists = await GetUserByEmailAsync(userRequest.Email);

        if (userExists is not null)
        {
            throw new UserEmailNotUniqueException("User email is not unique");
        }

        try
        {
            AdminUser user = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = userRequest.Name,
                Email = userRequest.Email.ToLower(),
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            await _adminUserContainer.CreateItemAsync(user, new PartitionKey(user.Id));

            return user;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }

    public async Task<ItemResponse<AdminUser>> UpdateUserAsync(UpdateUser updateRequest)
    {
        try
        {
            var user = await GetUserByIdAsync(updateRequest.Id);
            var updatedUser = new AdminUser()
            {
                Id = user.Id,
                Name = updateRequest.Name,
                Email = user.Email,
                LastUpdated = DateTime.UtcNow
            };
            var response = await _adminUserContainer.UpsertItemAsync<AdminUser>(updatedUser, new PartitionKey(user.Id));
            return response;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }


    }

    public async Task<AdminUser> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await _adminUserContainer.ReadItemAsync<AdminUser>(
                  id: id,
                  partitionKey: new PartitionKey(id)
            );

            return user;
        }
        catch (CosmosException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AdminUserNotFoundException("User is not found", ex);
            }
            else
            {
                throw new UnexpectedCosmosException("Cosmos Exception", ex);
            }

        }

    }


    public async Task<bool> DeleteUserAsync(string userId)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);

            if (user.IsOwner)
            {
                throw new AdminUserOwnerException("Admin user is owner");
            }

            var deleteUserSessions = await DeleteUserSessionsAsync(user!.Email);
            if (!deleteUserSessions)
            {
                throw new AdminUserSessionsCouldNotDeleteException("Could not delete admin user sessions");
            }
            var deleteUserResponse = await _adminUserContainer.DeleteItemAsync<ItemResponse<AdminUser>>(user.Id, new PartitionKey(user.Id));
            return true;

        }
        catch (CosmosException ex)
        {

            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }


    public async Task<List<AdminUser>> GetUsersAsync()
    {
        try
        {
            string getAllUsersQuery = $"SELECT * FROM {_formStoreDatabaseSettings.AdminUserCollectionName}";

            var query = new QueryDefinition(getAllUsersQuery);

            using FeedIterator<AdminUser> feed = _adminUserContainer.GetItemQueryIterator<AdminUser>(
               queryDefinition: query
            );

            List<AdminUser> users = new();
            while (feed.HasMoreResults)
            {
                FeedResponse<AdminUser> response = await feed.ReadNextAsync();
                foreach (AdminUser user in response)
                {
                    users.Add(user);
                }
            }

            return users;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }

    private FeedIterator<AdminSession> GetUserSessionsFeed(string email)
    {
        try
        {
            // check if the admin session store that the user exists
            string userByEmailQuery = $"SELECT * FROM {_formStoreDatabaseSettings.AdminSessionCollectionName} f WHERE f.email = @email ORDER BY f.created DESC";

            var query = new QueryDefinition(userByEmailQuery)
                .WithParameter("@email", email.ToLower());

            using FeedIterator<AdminSession> feed = _adminSessionContainer.GetItemQueryIterator<AdminSession>(
                   queryDefinition: query
                );

            return feed;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException(ex.Message.ToString(), ex);
        }

    }

    private async Task<(bool, AdminSession?)> GetSessionByEmailAsync(string email)
    {

        FeedResponse<AdminSession> response = await GetUserSessionsFeed(email).ReadNextAsync();

        return (response.ToList().Count > 0, response.FirstOrDefault());
    }


    private async Task<AdminUser?> GetUserByEmailAsync(string email)
    {
        try
        {
            string userByEmailQuery = $"SELECT * FROM {_formStoreDatabaseSettings.AdminUserCollectionName} u WHERE u.email = @email";

            var query = new QueryDefinition(userByEmailQuery)
                .WithParameter("@email", email.ToLower());

            using FeedIterator<AdminUser> feed = _adminUserContainer.GetItemQueryIterator<AdminUser>(
                   queryDefinition: query
                );

            FeedResponse<AdminUser> response = await feed.ReadNextAsync();

            return response.FirstOrDefault();
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException(ex.Message.ToString(), ex);
        }

    }

    private async Task<bool> DeleteUserSessionsAsync(string email)
    {
        bool deletAllSessions = true;
        var userSessionsFeed = GetUserSessionsFeed(email);
        while (userSessionsFeed.HasMoreResults)
        {
            FeedResponse<AdminSession> response = await userSessionsFeed.ReadNextAsync();
            foreach (AdminSession session in response)
            {
                try
                {
                    await _adminSessionContainer.DeleteItemAsync<ItemResponse<AdminSession>>(session.Id, new PartitionKey(session.Id));
                }
                catch (CosmosException ex)
                {
                    deletAllSessions = false;
                    throw new UnexpectedCosmosException(ex.Message.ToString(), ex);
                }

            }
        }
        return deletAllSessions;
    }






}