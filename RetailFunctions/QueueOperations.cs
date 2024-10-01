using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace RetailFunctions
{
    public static class ProcessQueueMessage
    {
        [Function("ProcessQueueMessage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string queueName = req.Query["queueName"];
            string message = req.Query["message"];

            if (string.IsNullOrEmpty(queueName) || string.IsNullOrEmpty(message))
            {
                return new BadRequestObjectResult("Queue name and message must be provided.");
            }

            // Get the Azure Storage connection string from environment variable
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Validate the connection string
            if (string.IsNullOrEmpty(connectionString))
            {
                log.LogError("AzureWebJobsStorage environment variable is not set.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            try
            {
                // Create the queue client
                var queueServiceClient = new QueueServiceClient(connectionString);
                var queueClient = queueServiceClient.GetQueueClient(queueName);

                // Ensure the queue exists before adding the message
                await queueClient.CreateIfNotExistsAsync();

                // Add message to the queue
                await queueClient.SendMessageAsync(message);

                return new OkObjectResult($"Message added to queue '{queueName}'");
            }
            catch (Exception ex)
            {
                log.LogError($"Error occurred while processing the queue: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
