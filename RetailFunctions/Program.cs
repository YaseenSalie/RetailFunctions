using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() // Use this for default function configurations
    .ConfigureServices(services =>
    {
        // Add Application Insights for monitoring
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .ConfigureWebJobs(b =>
    {
        // Register Blob and Queue storage bindings
        b.AddAzureStorageBlobs();   // For Blob Storage functions
        b.AddAzureStorageQueues();  // For Queue Storage functions
    })
    .Build();

host.Run();
