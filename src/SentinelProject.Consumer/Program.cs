using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using SentinelProject.Consumer.Core;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SentinelProject.Consumer.Infrastructure;
using MongoDB.Bson.Serialization.Conventions;

namespace SentinelProject.Consumer;

public class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(static (hostContext, services) =>
            {
                var connectionString = hostContext.Configuration.GetConnectionString("Transactions");

                var database = InitDatabase(connectionString);

                services
                .AddLogging(builder => builder.AddConsole())
                .AddScoped<ITransactionProcessor, TransactionProcessor>()
                .AddScoped<ITransactionsStore, TransactionsStore>()
                .AddScoped<ICustomerSettingsStore, CustomerSettingsStore>()
                .AddScoped<ICountriesStore, CountriesStore>()
                .AddScoped<TransactionCustomerSettingsProcessor>()
                .AddScoped<TransactionCountryProcessor>()
                .AddScoped<TransactionPatternProcessor>()
                .AddSingleton<IMongoDatabase>(database)
                .AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();

                    var entryAssembly = Assembly.GetEntryAssembly();

                    x.AddConsumers(entryAssembly);

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                });
            });

    private static IMongoDatabase InitDatabase(string connectionString)
    {
        var client = new MongoClient(connectionString);
        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };
        ConventionRegistry.Register(
            "Camel Case Convention",
            pack,
            t => true
            );
        IMongoDatabase database = client.GetDatabase("sentinel-transactions");

        InitCollections(database);

        return database;
    }

    private static void InitCollections(IMongoDatabase database)
    {
        try
        {
            var countriesCollection = database.GetCollection<StoredCountry>("countries");

            var countriesIndexModel = new CreateIndexModel<StoredCountry>(
                Builders<StoredCountry>.IndexKeys.Ascending(m => m.Name),
                new CreateIndexOptions
                {
                    Unique = true,
                    Name = "Country_Name"
                });
            countriesCollection.Indexes.CreateOne(countriesIndexModel);           

            var customersCollection = database.GetCollection<StoredCustomer>("customers");

            var customersIndexModel = new CreateIndexModel<StoredCustomer>(
                Builders<StoredCustomer>.IndexKeys.Ascending(m => m.CustomerId),
                new CreateIndexOptions
                {
                    Unique = true,
                    Name = "Customer_Id"
                });
            customersCollection.Indexes.CreateOne(customersIndexModel);

            var countries = new List<StoredCountry>
            {
                new()
                {
                    Name = "Trusted Country",
                    TrustRate = 1,
                },
                new()
                {
                    Name = "Medium Trust Country",
                    TrustRate = 0.4f,
                },
                new()
                {
                    Name = "Hostile Country",
                    TrustRate = 0.1f,
                }
            };

            countriesCollection.InsertManyAsync(countries);

            var customers = new List<StoredCustomer>
            {
                new() {
                    CustomerId = Guid.Parse("f2887467-a266-4554-9b8c-51d8e52c7771"),
                    Name = "Paolo Rossi",
                    MaxTransactionAmount = 100,
                },
                new() {
                    CustomerId = Guid.Parse("819af267-9ac2-4121-85d1-5bf6eab0cb25"),
                    Name = "Mario Verdi",
                    MaxTransactionAmount = 520,
                },
                new() {
                    CustomerId = Guid.Parse("d4620576-783d-4a64-bf68-1f386ccfeb14"),
                    Name = "Franco Romano",
                    MaxTransactionAmount = 50,
                }
            };
            customersCollection.InsertManyAsync(customers);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}