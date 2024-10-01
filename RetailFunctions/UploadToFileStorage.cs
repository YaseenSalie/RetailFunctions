using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System;

namespace RetailFunctions
{
    public static class UploadFile
    {
        [Function("UploadFile")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string shareName = req.Query["shareName"];
            string fileName = req.Query["fileName"];

            if (string.IsNullOrEmpty(shareName) || string.IsNullOrEmpty(fileName))
            {
                return new BadRequestObjectResult("Share name and file name must be provided.");
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
                // Create the Share service client
                var shareServiceClient = new ShareServiceClient(connectionString);
                var shareClient = shareServiceClient.GetShareClient(shareName);

                // Ensure the file share exists
                await shareClient.CreateIfNotExistsAsync();

                // Get the root directory and file client
                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                // Upload the file to Azure Files
                using var stream = req.Body;
                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadAsync(stream);

                return new OkObjectResult("File uploaded to Azure Files successfully.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error uploading file to Azure Files: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
