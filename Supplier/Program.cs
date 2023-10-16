using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Supplier;
using Supplier.Business;
using Supplier.Services;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<SupplierBusiness>();
        services.AddSingleton<RabbitMQMessageSender>();
        services.AddHostedService<Supplier.Supplier>();
    })
    .RunConsoleAsync();


return;