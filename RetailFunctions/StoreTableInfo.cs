using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace RetailFunctions
{
    public static class StoreTableInfo
    {
        [Function("StoreTableInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string tableName = req.Query["tableName"];
            string partitionKey = req.Query["partitionKey"];
            string rowKey = req.Query["rowKey"];
            string data = req.Query["data"];

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey) || string.IsNullOrEmpty(data))
            {
                return new BadRequestObjectResult("Table name, partition key, row key, and data must be provided.");
            }

            // Get the connection string from the environment variable
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Validate the connection string
            if (string.IsNullOrEmpty(connectionString))
            {
                log.LogError("AzureWebJobsStorage environment variable is not set.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            try
            {
                // Create the Table Service client
                var serviceClient = new TableServiceClient(connectionString);
                var tableClient = serviceClient.GetTableClient(tableName);

                // Ensure the table exists
                await tableClient.CreateIfNotExistsAsync();

                // Create the entity and add it to the table
                var entity = new TableEntity(partitionKey, rowKey) { ["Data"] = data };
                await tableClient.AddEntityAsync(entity);

                return new OkObjectResult("Data added to table");
            }
            catch (Exception ex)
            {
                log.LogError($"Error occurred while adding data to table: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
