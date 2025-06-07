using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.Azure.Cosmos;
using Models;
using System.Net.Http.Headers;
using System.Net;

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

    public async Task CreateAsync(Form newForm) => await _formsContainer.UpsertItemAsync<Form>(newForm);
    public async Task<(bool, HttpStatusCode)> CheckFormExistsByNameAsync(string formName)
    {
        try
        {
            string formByNameQuery = $"SELECT * FROM {_formStoreDatabaseSettings.FormCollectionName} f WHERE f.name = @formName";

            var query = new QueryDefinition(formByNameQuery)
            .WithParameter("@formName", formName);

            using FeedIterator<Form> feed = _formsContainer.GetItemQueryIterator<Form>(
               queryDefinition: query
            );

            FeedResponse<Form> response = await feed.ReadNextAsync();

            return (response.ToList().Count == 0, response.StatusCode);
        }
        catch (CosmosException ex)
        {
            return (false, ex.StatusCode);
        }
    }

    public async Task<(Form?, HttpStatusCode)> GetFormByIdAsync(string id)
    {

        try
        {
            Form form = await _formsContainer.ReadItemAsync<Form>(
                  id: id,
                  partitionKey: new PartitionKey(id)
            );
            return (form, HttpStatusCode.OK);

        }
        catch (CosmosException ex)
        {
            return (null, ex.StatusCode);
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
        var item = await _formsContainer.DeleteItemAsync<ItemResponse<Form>>(id, new PartitionKey(id));
        return item;
    }

    public async Task<ItemResponse<Form>> UpdateFormAsync(Form form, DateTime created)
    {

        form.Created = created;
        ItemResponse<Form> item = await _formsContainer.UpsertItemAsync<Form>(form, new PartitionKey(form.Id));
        return item;
    }



}