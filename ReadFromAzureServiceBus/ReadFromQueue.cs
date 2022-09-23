using System;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Container = Microsoft.Azure.Cosmos.Container;

namespace ReadFromAzureServiceBus
{
    public class ReadFromQueue
    {
        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("EndPointUri");
        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
        private CosmosClient cosmosClient;
        private Database database;
        private Container container;
        private string databaseId = "ToDoList";
        private string containerId = "Items";
        [FunctionName("ReadFromQueue")]
        public async Task RunAsync([ServiceBusTrigger("demo", Connection = "QueueConnectionSting")]string myQueueItem, ILogger log)
        {
            try
            {
                log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
                cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "pubsubdb" });
                database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                container = await database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");

                log.LogInformation("new db and container created");
                await AddItemsToContainerAsync(log, myQueueItem); ;
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.InnerException.ToString());
            }
            

        }

        private async Task AddItemsToContainerAsync(ILogger log, string myQueueItem)
        {
            
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<string> EntryToCOsmos = await container.CreateItemAsync<string>(JObject.Parse(myQueueItem).ToString(), null);

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                log.LogInformation("Created item in database with id: {0} Operation consumed {1} RUs.\n", myQueueItem, EntryToCOsmos.RequestCharge);
            
        }
    }
}
