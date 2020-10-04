using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ContosoMovies
{
    public static class ContosoMovies
    {
        [FunctionName("ContosoMovies")]
        public static async Task Run([EventHubTrigger("telemetry", Connection = "bobjacnosqlhack_datagenerator_policy")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();
            string endpoint = "";
            string authKey = "";
            string cosmosConnectionString = "AccountEndpoint=https://bobjacnosqlhack.documents.azure.com:443/;AccountKey=UkNbRQsYnddcoG4mp9GJaEFa5kIr3rCEyX1gbNPzdzPzRlae91xy8zMU6kBb0cxkQRNPbyOFoISAVCeba1n3qA==;";
            string databaseId = "ContosoMoviesChallengeThree";
            string containerId = "Orders";

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    log.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");

                    //var order = JsonSerializer.Deserialize<Order>(messageBody);

                    var order = JsonConvert.DeserializeObject<Order>(messageBody);

                    log.LogInformation("Successfully deserialized Order - V2");

                    order.id = Guid.NewGuid().ToString();
                    order.OrderId = new Random().Next();
                 //   order.OrderDetails.ForEach(x => x.ProductId = new Random().Next());

                    using (CosmosClient client = new CosmosClient(cosmosConnectionString))
                    {
                        var database = client.GetDatabase(databaseId);
                        var container = database.GetContainer(containerId);
                        var cosmosResult = await container.CreateItemAsync<Order>(order);
                    }


                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
