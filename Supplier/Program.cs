using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Supplier;
using Supplier.Business;
using Supplier.Services;
using Types;


await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<SupplierBusiness>();
        services.AddDbContext<CodeObjectContext>(options => options.UseNpgsql(context.Configuration["CONNECTIONSTRING"]).EnableSensitiveDataLogging());
        services.AddSingleton<RabbitMQMessageSender>();
        // Add services to the container.
        services.AddSingleton<DatabaseSettings>(Startup.SetupMongoConfig(context.Configuration));
        services.AddSingleton<Repository>();
        services.AddHostedService<Supplier.Supplier>();
    })
    .RunConsoleAsync();



return;