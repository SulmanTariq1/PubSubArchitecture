using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.Azure.ServiceBus;

namespace ReadBlobStorageAzureFunction
{
    public class ReadBlobStoarge
    {
        [FunctionName("ReadBlobStoarge")]
        public async Task Run([BlobTrigger("demo/{name}", Connection = "storageConnectionString")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            StreamReader reader = new StreamReader(myBlob, Encoding.UTF8);
            string text = reader.ReadToEnd();
            var queueClient = new QueueClient(Environment.GetEnvironmentVariable("AzureServiceBus"), "demo");
            string messageBody = JsonSerializer.Serialize(text);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));

            await queueClient.SendAsync(message);
        }
    }
}
