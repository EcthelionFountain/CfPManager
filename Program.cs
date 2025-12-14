using CfpScraperWorker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // 1. Register the HttpClient
        services.AddHttpClient<ScraperService>(); 
        
        // 2. Register the ScraperService itself
        services.AddSingleton<ScraperService>(); 
        
        // 3. Register the Worker as a background service
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
