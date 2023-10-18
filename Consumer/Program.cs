using Consumer;
using Consumer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Types;


await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<DatabaseSettings>(Startup.SetupMongoConfig(context.Configuration));
        services.AddSingleton<Repository>();
        services.AddHostedService<Consumer.Consumer>();
    })
    .RunConsoleAsync();


return;