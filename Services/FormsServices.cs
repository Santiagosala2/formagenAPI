using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.Azure.Cosmos;
using Models;

namespace Services;

public class FormsService
{
    private readonly Container _formsContainer;
    private readonly FormStoreDatabaseSettings _formStoreDatabaseSettings;

    public FormsService(
        IOptions<FormStoreDatabaseSettings> formStoreDatabaseSettings)
    {

        CosmosClient cosmosClient = new(
            formStoreDatabaseSettings.Value.ConnectionString,
            new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
            }

        );

        Database database = cosmosClient.GetDatabase(formStoreDatabaseSettings.Value.DatabaseName);

        _formsContainer = database.GetContainer(
           formStoreDatabaseSettings.Value.FormCollectionName);

        _formStoreDatabaseSettings = formStoreDatabaseSettings.Value;


    }

    public async Task CreateAsync(Form newForm) => await _formsContainer.UpsertItemAsync<Form>(newForm);
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

    public async Task<Form?> GetFormByIdAsync(string id)
    {
        string formByIdQuery = $"SELECT * FROM {_formStoreDatabaseSettings.FormCollectionName} f WHERE f.id = @id";

        var query = new QueryDefinition(formByIdQuery)
        .WithParameter("@id", id);

        using FeedIterator<Form> feed = _formsContainer.GetItemQueryIterator<Form>(
           queryDefinition: query
        );

        FeedResponse<Form> response = await feed.ReadNextAsync();

        return response.FirstOrDefault();
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
        var item = await _formsContainer.DeleteItemAsync<ItemResponse<Form>>(id, new PartitionKey(id));
        return item;
    }



}