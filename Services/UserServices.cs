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
using DTOs.User;

namespace Services;

public class UserService : IUserService
{
    private readonly Container _userSessionContainer;

    private readonly Container _userContainer;
    private readonly DatabaseSettings _databaseSettings;

    private readonly EmailServiceSettings _emailServiceSettings;

    private readonly HttpClient _emailClient;

    public record EmailServicePayload(string email, string otp);

    public UserService(
        IOptions<DatabaseSettings> formStoreDatabaseSettings,
        IOptions<EmailServiceSettings> emailServiceSettings
        )
    {

        CosmosClient cosmosClient = new(
            formStoreDatabaseSettings.Value.ConnectionString,
            new CosmosClientOptions
            {

                UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }

            }

        );

        _emailClient = new HttpClient();
        _emailServiceSettings = emailServiceSettings.Value;


        Database database = cosmosClient.GetDatabase(formStoreDatabaseSettings.Value.DatabaseName);

        _userSessionContainer = database.GetContainer(
           formStoreDatabaseSettings.Value.UserSessionCollectionName);

        _userContainer = database.GetContainer(
           formStoreDatabaseSettings.Value.UserCollectionName);

        _databaseSettings = formStoreDatabaseSettings.Value;


    }

    public async Task<bool> SendOTPAsync(string email)
    {
        var user = await this.GetUserByEmailAsync(email);
        bool otpSent = false;

        if (user is null)
        {
            // tell the user that account does not exist
            return otpSent;
        }

        // try to send an otp that has already been created - in which ExpireDate isnt expired, Use = false and UseUntil < 5 mins
        var usedSession = await GetUsedSessions(email);
        string otp;
        if (usedSession is null)
        {
            // prepare otp
            otp = OtpGenerator.GenerateOtp();
            // replace current otp
            Session session = new()
            {
                Id = Guid.NewGuid().ToString(),
                OTP = otp,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                UseUntil = DateTime.UtcNow.AddMinutes(5)
            };

            session!.OTP = otp;
            await CreateSessionAsync(session);
        }
        else
        {
            otp = usedSession!.OTP;
        }

        var payload = new StringContent(JsonSerializer.Serialize(new EmailServicePayload(email, otp)));
        // trigger send otp service
        await _emailClient.PostAsync(_emailServiceSettings.FlowURI, payload);
        otpSent = true;
        return otpSent;
    }

    public async Task<(bool, Session?)> VerifyOTPAsync(string email, string otp)
    {
        var (userExists, session) = await this.GetSessionByEmailAsync(email);

        if (userExists)
        {
            if (session!.OTP.ToLower() == otp.ToLower())
            {
                session.Used = true;
                await _userSessionContainer.UpsertItemAsync<Session>(session);
                return (true, session);
            }
        }

        return (false, null);
    }

    public async Task CreateSessionAsync(Session newUserSession) => await _userSessionContainer.UpsertItemAsync<Session>(newUserSession);

    public async Task<Session> GetSessionByIdAsync(string sessionId)
    {
        try
        {
            var session = await _userSessionContainer.ReadItemAsync<Session>(sessionId, new PartitionKey(sessionId));
            return session;
        }
        catch (CosmosException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserSessionNotFoundException("Session is not found", ex);
            }
            else
            {
                throw new UnexpectedCosmosException("Cosmos Exception", ex);
            }
        }
    }

    public async Task<Models.User.User> CreateUserAsync(CreateUser userRequest)
    {
        var userExists = await GetUserByEmailAsync(userRequest.Email);

        if (userExists is not null)
        {
            throw new UserEmailNotUniqueException("Models.User.User email is not unique");
        }

        try
        {
            Models.User.User user = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = userRequest.Name,
                Email = userRequest.Email.ToLower(),
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            await _userContainer.CreateItemAsync(user, new PartitionKey(user.Id));

            return user;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }

    public async Task<ItemResponse<Models.User.User>> UpdateUserAsync(UpdateUser updateRequest)
    {
        try
        {
            var user = await GetUserByIdAsync(updateRequest.Id);
            var updatedUser = new Models.User.User()
            {
                Id = user.Id,
                Name = updateRequest.Name,
                Email = user.Email,
                LastUpdated = DateTime.UtcNow
            };
            var response = await _userContainer.UpsertItemAsync<Models.User.User>(updatedUser, new PartitionKey(user.Id));
            return response;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }

    public async Task<Models.User.User> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await _userContainer.ReadItemAsync<Models.User.User>(
                  id: id,
                  partitionKey: new PartitionKey(id)
            );

            return user;
        }
        catch (CosmosException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException("User is not found", ex);
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
            var deleteUserSessions = await DeleteUserSessionsAsync(user!.Email);
            if (!deleteUserSessions)
            {
                // throw new UserSessionsCouldNotDeleteException("Could not delete admin user sessions");
            }
            var deleteUserResponse = await _userContainer.DeleteItemAsync<ItemResponse<Models.User.User>>(user.Id, new PartitionKey(user.Id));
            return true;

        }
        catch (CosmosException ex)
        {

            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }


    public async Task<List<Models.User.User>> GetUsersAsync()
    {
        try
        {
            string getAllUsersQuery = $"SELECT * FROM {_databaseSettings.UserCollectionName}";

            var query = new QueryDefinition(getAllUsersQuery);

            using FeedIterator<Models.User.User> feed = _userContainer.GetItemQueryIterator<Models.User.User>(
               queryDefinition: query
            );

            List<Models.User.User> users = new();
            while (feed.HasMoreResults)
            {
                FeedResponse<Models.User.User> response = await feed.ReadNextAsync();
                foreach (Models.User.User user in response)
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

    private FeedIterator<Session> GetUserSessionsFeed(string email)
    {
        try
        {
            // check if the admin session store that the user exists
            string userByEmailQuery = $"SELECT * FROM {_databaseSettings.UserSessionCollectionName} f WHERE f.email = @email ORDER BY f.created DESC";

            var query = new QueryDefinition(userByEmailQuery)
                .WithParameter("@email", email.ToLower());

            using FeedIterator<Session> feed = _userSessionContainer.GetItemQueryIterator<Session>(
                   queryDefinition: query
                );

            return feed;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException(ex.Message.ToString(), ex);
        }

    }

    private async Task<(bool, Session?)> GetSessionByEmailAsync(string email)
    {

        FeedResponse<Session> response = await GetUserSessionsFeed(email).ReadNextAsync();

        return (response.ToList().Count > 0, response.FirstOrDefault());
    }


    private async Task<Models.User.User?> GetUserByEmailAsync(string email)
    {
        try
        {
            string userByEmailQuery = $"SELECT * FROM {_databaseSettings.UserCollectionName} u WHERE u.email = @email";

            var query = new QueryDefinition(userByEmailQuery)
                .WithParameter("@email", email.ToLower());

            using FeedIterator<Models.User.User> feed = _userContainer.GetItemQueryIterator<Models.User.User>(
                   queryDefinition: query
                );

            FeedResponse<Models.User.User> response = await feed.ReadNextAsync();

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
            FeedResponse<Session> response = await userSessionsFeed.ReadNextAsync();
            foreach (Session session in response)
            {
                try
                {
                    await _userSessionContainer.DeleteItemAsync<ItemResponse<Session>>(session.Id, new PartitionKey(session.Id));
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

    private async Task<Session?> GetUsedSessions(string email)
    {
        try
        {
            var useUntil = DateTime.UtcNow.AddMinutes(-4.6).ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            var expiresAt = DateTime.UtcNow.AddMinutes(-40).ToString("s", System.Globalization.CultureInfo.InvariantCulture);

            string userByEmailQuery = $@"
                   SELECT * FROM {_databaseSettings.UserSessionCollectionName} s
                   WHERE s.email = @email and s.expiresAt > @expiresAt and s.useUntil > @useUntil and s.used = false
                   ORDER BY s.created DESC";

            var query = new QueryDefinition(userByEmailQuery)
                .WithParameter("@email", email.ToLower())
                .WithParameter("@expiresAt", expiresAt)
                .WithParameter("@useUntil", useUntil);

            using FeedIterator<Session> feed = _userSessionContainer.GetItemQueryIterator<Session>(
                   queryDefinition: query
                );

            FeedResponse<Session> response = await feed.ReadNextAsync();

            return response.FirstOrDefault();
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException(ex.Message.ToString(), ex);
        }

    }



}