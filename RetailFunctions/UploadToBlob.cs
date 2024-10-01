using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System;

namespace RetailFunctions
{
    public static class UploadBlob
    {
        [Function("UploadBlob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string containerName = req.Query["containerName"];
            string blobName = req.Query["blobName"];

            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobName))
            {
                return new BadRequestObjectResult("Container name and blob name must be provided.");
            }

            // Get the Azure Storage connection string from environment variables
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Check if the connection string is available
            if (string.IsNullOrEmpty(connectionString))
            {
                log.LogError("AzureWebJobsStorage environment variable is not set.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            try
            {
                // Create the Blob service client
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Ensure the container exists before uploading the blob
                await containerClient.CreateIfNotExistsAsync();

                // Create a Blob client and upload the file
                var blobClient = containerClient.GetBlobClient(blobName);

                using var stream = req.Body;
                await blobClient.UploadAsync(stream, true);

                return new OkObjectResult("Blob uploaded successfully.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error uploading blob: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
