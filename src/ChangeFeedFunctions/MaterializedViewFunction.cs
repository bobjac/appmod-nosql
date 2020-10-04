using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ChangeFeedFunctions
{
    public static class MaterializedViewFunction
    {
        [FunctionName("MaterializedViewFunction")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "ContosoMoviesChallengeThree",
            collectionName: "MaterializedViews",
            ConnectionStringSetting = "DBConnectionString",
            CreateLeaseConnectionIfNotExists = true,
            LeaseCollectionName = "materializedViewLeases")]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }
        }
    }
}
