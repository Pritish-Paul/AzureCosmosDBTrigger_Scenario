using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace StudentProcessorViaCosmosDB
{
    public class Student
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
    public static class Function1
    {

        public static readonly string _endpointUrl = "https://studententry.documents.azure.com:443/";
        public static readonly string _primaryKey = "yKZkUFZylIA7Z44PO2eMKj9G54CqdtZjNAFRYSrFk60lEibmB3OhjaNSrwiEtha4OXH0tmq8tWNEBhqeaRoxDw==";
        public static readonly string _databaseId = "Students";
        public static readonly string _containerId = "Studentcontainer";
        public static CosmosClient cosmosClient = new CosmosClient(_endpointUrl, _primaryKey);

        [FunctionName("Studentprocessor")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "Students",
            collectionName: "Markscontainer",
            ConnectionStringSetting = "cosmosdbtvk_DOCUMENTDB",
            LeaseCollectionName = "leases",CreateLeaseCollectionIfNotExists =true)]IReadOnlyList<Document> input, ILogger log)
        {


            Dictionary<string, string> studdata = new Dictionary<string, string>();
            var id = "";
            var percent = "";
            foreach (Document doc in input)
            {

                studdata.Add(doc.GetPropertyValue<string>("id"), doc.GetPropertyValue<string>("percent"));
            }

            foreach (KeyValuePair<string, string> item in studdata)
            {
                id = item.Key;
                percent = item.Value;
            }


            log.LogTrace("started processing");
            log.LogInformation("started processing");
            //await StatusChangeAsync(id, percent);
            var container2 = cosmosClient.GetContainer(_databaseId, _containerId);
            ItemResponse<Student> studentresponse = await container2.ReadItemAsync<Student>(id, new Microsoft.Azure.Cosmos.PartitionKey(id));

            if (Convert.ToInt32(percent) > 60)
            {
                var itemBody = studentresponse.Resource;
                itemBody.Status = "Pass";
                studentresponse = await container2.ReplaceItemAsync<Student>(itemBody, itemBody.id, new Microsoft.Azure.Cosmos.PartitionKey(itemBody.id));
            }
            else if (Convert.ToInt32(percent) < 60)
            {
                var itemBody = studentresponse.Resource;
                itemBody.Status = "Fail";
                studentresponse = await container2.ReplaceItemAsync<Student>(itemBody, itemBody.id, new Microsoft.Azure.Cosmos.PartitionKey(itemBody.id));

            }
            log.LogTrace("completed processing");
            log.LogInformation("completed processing");

            // replace the item with the updated content




        }
    }
}
