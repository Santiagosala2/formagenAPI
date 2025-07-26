using Microsoft.Extensions.Options;
using Models;
using Helpers;
using System.Text.Json;
using FormagenAPI.Services;
using System.Net;
using FormagenAPI.Exceptions;
using DTOs.User;
using Microsoft.Azure.Cosmos;
using User = Models.User.User;

namespace Services;

public class UserService : IUserService
{
    private readonly Container _userSessionContainer;

    private readonly Container _userContainer;
    private readonly DatabaseSettings _databaseSettings;
    private readonly IEmailService _emailService;

    public UserService(
        IOptions<DatabaseSettings> formStoreDatabaseSettings,
        IEmailService emailService
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

        _emailService = emailService;


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
                UseUntil = DateTime.UtcNow.AddMinutes(5),
                UserId = user.Id
            };

            session!.OTP = otp;
            await CreateSessionAsync(session);
        }
        else
        {
            otp = usedSession!.OTP;
        }

        await _emailService.SendOTPEmail(email, otp);
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

    public async Task<User> CreateUserAsync(CreateUser userRequest)
    {
        var userExists = await GetUserByEmailAsync(userRequest.Email);

        if (userExists is not null)
        {
            throw new UserEmailNotUniqueException("User email is not unique");
        }

        try
        {
            User user = new()
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

    public async Task<ItemResponse<User>> UpdateUserAsync(UpdateUser updateRequest)
    {
        try
        {
            var user = await GetUserByIdAsync(updateRequest.Id);
            var updatedUser = new User()
            {
                Id = user.Id,
                Name = updateRequest.Name,
                Email = user.Email,
                LastUpdated = DateTime.UtcNow
            };
            var response = await _userContainer.UpsertItemAsync<User>(updatedUser, new PartitionKey(user.Id));
            return response;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }

    public async Task<User> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await _userContainer.ReadItemAsync<User>(
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
            var deleteUserResponse = await _userContainer.DeleteItemAsync<ItemResponse<User>>(user.Id, new PartitionKey(user.Id));
            return true;

        }
        catch (CosmosException ex)
        {

            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }


    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            string getAllUsersQuery = $"SELECT * FROM {_databaseSettings.UserCollectionName}";

            var query = new Microsoft.Azure.Cosmos.QueryDefinition(getAllUsersQuery);

            using Microsoft.Azure.Cosmos.FeedIterator<User> feed = _userContainer.GetItemQueryIterator<User>(
               queryDefinition: query
            );

            List<User> users = new();
            while (feed.HasMoreResults)
            {
                Microsoft.Azure.Cosmos.FeedResponse<User> response = await feed.ReadNextAsync();
                foreach (User user in response)
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


    private async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            string userByEmailQuery = $"SELECT * FROM {_databaseSettings.UserCollectionName} u WHERE u.email = @email";

            var query = new QueryDefinition(userByEmailQuery)
                .WithParameter("@email", email.ToLower());

            using FeedIterator<User> feed = _userContainer.GetItemQueryIterator<User>(
                   queryDefinition: query
                );

            FeedResponse<User> response = await feed.ReadNextAsync();

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