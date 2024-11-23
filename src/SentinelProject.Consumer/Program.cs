using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using SentinelProject.Consumer.Core;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace SentinelProject.Consumer;

public class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services
                .AddScoped<ITransactionProcessor, TransactionProcessor>()
                .AddScoped<ITransactionsStore, TransactionsStore>()
                .AddScoped<ICustomerSettingsStore, CustomerSettingsStore>()
                .AddScoped<ICountriesStore, CountriesStore>()
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
}

public class CustomerSettingsStore : ICustomerSettingsStore
{
    public Customer GetById(Guid Id)
    {
        return new Customer(Id, "Name", 150);
    }
}

public class CountriesStore : ICountriesStore
{
    public Country GetCountry(string name)
    {
        return new Country(name, 0.15f);
    }
}

public class TransactionsStore : ITransactionsStore
{
    public IReadOnlyList<LatestTransaction> GetLatestTransactionsForCustomer(Guid customerId, int howMany)
    {
        return new List<LatestTransaction>();
    }

    public void Store(CustomerTransaction transaction)
    {
        
    }
}
