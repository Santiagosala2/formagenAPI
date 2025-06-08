using DTOs;
using FormagenAPI.Exceptions;
using FormagenAPI.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Models;
using MongoDB.Driver;

namespace Services;

public class FormService : IFormService
{
    private readonly Container _formsContainer;
    private readonly FormStoreDatabaseSettings _formStoreDatabaseSettings;

    public FormService(
        IOptions<FormStoreDatabaseSettings> formStoreDatabaseSettings)
    {

        CosmosClient cosmosClient = new(
            formStoreDatabaseSettings.Value.ConnectionString,
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

        Database database = cosmosClient.GetDatabase(formStoreDatabaseSettings.Value.DatabaseName);

        _formsContainer = database.GetContainer(
           formStoreDatabaseSettings.Value.FormCollectionName);

        _formStoreDatabaseSettings = formStoreDatabaseSettings.Value;


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
            throw new CreateFormException("Cosmos Exception", ex);
        }
        catch(Exception ex)
        {
            throw new CreateFormException("Unknown Exception", ex);
        }
    }  
        

    public async Task<bool> CheckFormExistsByNameAsync(string formName)
    {
        string formByNameQuery = $"SELECT * FROM {_formStoreDatabaseSettings.FormCollectionName} f WHERE f.name = @formName";

        var query = new QueryDefinition(formByNameQuery)
        .WithParameter("@formName", formName);

        using FeedIterator<Form> feed = _formsContainer.GetItemQueryIterator<Form>(
            queryDefinition: query
        );

        FeedResponse<Form> response = await feed.ReadNextAsync();

        return response.ToList().Count == 0;
    }

    public async Task<Form> GetFormByIdAsync(string id)
    {
        try
        {
            var form = await _formsContainer.ReadItemAsync<Form>(
                  id: id,
                  partitionKey: new PartitionKey(id)
            );

            if (form == null)
                throw new FormNotFoundException("Form is null");

            return form;
        }
        catch (CosmosException ex)
        {
            throw new FormNotFoundException("Cosmos Exception", ex);
        }
        catch (Exception ex)
        {
            throw new FormNotFoundException("Unknown exception", ex);
        }
    }

    public async Task<List<Form>> GetFormsAsync()
    {
        string formByIdQuery = $"SELECT * FROM {_formStoreDatabaseSettings.FormCollectionName}";

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
            throw new DeleteFormException("Cosmos Exception", ex);
        }
        catch (Exception ex)
        {
            throw new DeleteFormException("Unknown exception", ex);
        }
    }

    public async Task<ItemResponse<Form>> UpdateFormAsync(UpdateFormRequest updateFormRequest)
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
                LastUpdated = DateTime.UtcNow
            };

            ItemResponse<Form> item = await _formsContainer.UpsertItemAsync(updatedForm, new PartitionKey(form.Id));
            return item;
        }
        catch (CosmosException ex)
        {
            throw new UpdateFormException("Cosmos Exception", ex);
        }
        catch (Exception ex)
        {
            throw new UpdateFormException("Unknown exception", ex);
        }
    }
}