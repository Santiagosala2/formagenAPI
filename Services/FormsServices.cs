using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.Azure.Cosmos;
using Models;

namespace Services;

public class FormsService
{
    private readonly Container _formsContainer;

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


    }

    public async Task CreateAsync(Form newForm) =>

        await _formsContainer.UpsertItemAsync<Form>(newForm);


}