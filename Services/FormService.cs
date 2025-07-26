using System.Net;
using DTOs.Form;
using FormagenAPI.Exceptions;
using FormagenAPI.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Models.Form;
using Models;
using MongoDB.Driver;
using Models.User;
using Models.Admin;

namespace Services;

public class FormService : IFormService
{
    private readonly Container _formsContainer;

    private readonly Container _responseContainer;
    private readonly DatabaseSettings _databaseSettings;

    private readonly IUserService _userService;

    private readonly IAdminService _adminService;

    public FormService(
        IOptions<DatabaseSettings> databaseSettings, IUserService userService, IAdminService adminService)
    {

        CosmosClient cosmosClient = new(
            databaseSettings.Value.ConnectionString,
            new CosmosClientOptions
            {

                UseSystemTextJsonSerializerWithOptions = new System.Text.Json.JsonSerializerOptions()
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    AllowOutOfOrderMetadataProperties = true,
                    WriteIndented = true
                }

            }

        );

        Database database = cosmosClient.GetDatabase(databaseSettings.Value.DatabaseName);

        _formsContainer = database.GetContainer(
           databaseSettings.Value.FormCollectionName);
        _responseContainer = database.GetContainer(
           databaseSettings.Value.ResponseCollectionName);

        _databaseSettings = databaseSettings.Value;

        _userService = userService;

        _adminService = adminService;


    }

    public async Task<CreateFormResponse> CreateFormAsync(CreateFormRequest createFormRequest)
    {
        var formNameIsUnique = await CheckFormExistsByNameAsync(createFormRequest.Name);

        if (!formNameIsUnique)
        {
            throw new FormNameIsNotUniqueException("Form name is not unique");
        }

        try
        {

            Form form = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = createFormRequest.Name,
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            await _formsContainer.UpsertItemAsync<Form>(form);

            CreateFormResponse formResponse = new()
            {
                Id = form.Id,
                Name = form.Name
            };

            return formResponse;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }


    public async Task<bool> CheckFormExistsByNameAsync(string formName)
    {
        string formByNameQuery = $"SELECT * FROM {_databaseSettings.FormCollectionName} f WHERE f.name = @formName";

        var query = new QueryDefinition(formByNameQuery)
        .WithParameter("@formName", formName);

        using FeedIterator<Form> feed = _formsContainer.GetItemQueryIterator<Form>(
            queryDefinition: query
        );

        FeedResponse<Form> response = await feed.ReadNextAsync();

        return response.ToList().Count == 0;
    }

    public async Task<Form> GetFormByIdAsync(string id, Session? session = null)
    {
        try
        {
            var form = await _formsContainer.ReadItemAsync<Form>(
                  id: id,
                  partitionKey: new PartitionKey(id)
            );

            if (session is not null)
            {
                if (!session.IsAdmin)
                {
                    var userAccess = form.Resource.SharedUsers.FirstOrDefault(u => u.Email == session.Email);
                    if (userAccess == null)
                    {
                        throw new UnauthorizedAccessException("External user does not have access");
                    }
                }
            }

            return form;
        }
        catch (CosmosException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new FormNotFoundException("Form is not found", ex);
            }
            else
            {
                throw new UnexpectedCosmosException("Cosmos Exception", ex);
            }

        }

    }

    public async Task<List<Form>> GetFormsAsync()
    {

        try
        {
            string formByIdQuery = $"SELECT * FROM {_databaseSettings.FormCollectionName}";

            var query = new QueryDefinition(formByIdQuery);

            using FeedIterator<Form> feed = _formsContainer.GetItemQueryIterator<Form>(
               queryDefinition: query
            );

            List<Form> items = new();
            while (feed.HasMoreResults)
            {
                FeedResponse<Form> response = await feed.ReadNextAsync();
                foreach (Form item in response)
                {
                    items.Add(item);
                }
            }

            return items;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }



    }

    public async Task<ItemResponse<Form>> DeleteFormByIdAsync(string id)
    {
        var form = GetFormByIdAsync(id);

        try
        {
            var item = await _formsContainer.DeleteItemAsync<ItemResponse<Form>>(id, new PartitionKey(id));
            return item;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }

    public async Task<ItemResponse<Form>> UpdateFormAsync(SaveFormRequest updateFormRequest)
    {
        var form = await GetFormByIdAsync(updateFormRequest.Id);

        if (updateFormRequest.Name != form.Name)
        {
            var formNameIsUnique = await CheckFormExistsByNameAsync(updateFormRequest.Name);

            if (!formNameIsUnique)
            {
                throw new FormNameIsNotUniqueException("Form name is not unique");
            }
        }

        try
        {
            var updatedForm = new Form()
            {
                Id = updateFormRequest.Id,
                Name = updateFormRequest.Name,
                Title = updateFormRequest.Title,
                Description = updateFormRequest.Description,
                Questions = updateFormRequest.Questions,
                Created = form.Created,
                SharedUsers = form.SharedUsers,
                LastUpdated = DateTime.UtcNow
            };

            ItemResponse<Form> item = await _formsContainer.UpsertItemAsync(updatedForm, new PartitionKey(form.Id));
            return item;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }



    public async Task<ItemResponse<Form>> ShareFormAsync(ShareFormRequest shareFormRequest)
    {
        try
        {
            var form = await GetFormByIdAsync(shareFormRequest.Id);
            foreach (var user in shareFormRequest.Users)
            {
                await _userService.GetUserByIdAsync(user.Id);
            }

            List<SharedUser> sharedUsers = form.SharedUsers.Concat(shareFormRequest.Users).DistinctBy(u => u.Id).ToList();

            var updatedForm = new Form()
            {
                Id = form.Id,
                Name = form.Name,
                Title = form.Title,
                Description = form.Description,
                Questions = form.Questions,
                Created = form.Created,
                SharedUsers = sharedUsers,
                LastUpdated = DateTime.UtcNow
            };

            ItemResponse<Form> updatedform = await _formsContainer.UpsertItemAsync(updatedForm, new PartitionKey(form.Id));
            return updatedform;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }

    public async Task<ItemResponse<Form>> RemoveAccessFormAsync(RemoveAccessFormRequest removeAccessFormRequest)
    {
        try
        {
            var form = await GetFormByIdAsync(removeAccessFormRequest.Id);

            var user = await _userService.GetUserByIdAsync(removeAccessFormRequest.UserId);

            List<SharedUser> sharedUsers = form.SharedUsers.Where(u => u.Id != user.Id).ToList();

            var updatedForm = new Form()
            {
                Id = form.Id,
                Name = form.Name,
                Title = form.Title,
                Description = form.Description,
                Questions = form.Questions,
                Created = form.Created,
                SharedUsers = sharedUsers,
                LastUpdated = DateTime.UtcNow
            };

            ItemResponse<Form> updatedform = await _formsContainer.UpsertItemAsync(updatedForm, new PartitionKey(form.Id));
            return updatedform;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }

    public async Task<bool> SubmitFormAsync(SubmitFormRequest submitFormRequest)
    {
        try
        {
            var form = await GetFormByIdAsync(submitFormRequest.Id);
            BaseUser user = submitFormRequest.User.IsAdmin ?
                               await _adminService.GetUserByIdAsync(submitFormRequest.User.UserId) :
                               await _userService.GetUserByIdAsync(submitFormRequest.User.UserId);

            var formResponse = new FormResponse
            {
                Id = Guid.NewGuid().ToString(),
                FormId = form.Id,
                User = new SharedUser()
                {
                    Email = user.Email,
                    Id = user.Id,
                    Name = user.Name
                },
                Questions = submitFormRequest!.Questions,
                Created = DateTime.UtcNow
            };

            ItemResponse<FormResponse> submitform = await _responseContainer.CreateItemAsync(formResponse);
            return true;
        }
        catch (CosmosException ex)
        {
            throw new UnexpectedCosmosException("Cosmos Exception", ex);
        }
    }

}