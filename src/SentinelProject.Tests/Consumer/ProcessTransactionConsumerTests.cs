using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using SentinelProject.Consumer;
using SentinelProject.Consumer.Consumers;
using SentinelProject.Consumer.Core;
using SentinelProject.Messages;
using Xunit.Abstractions;

namespace SentinelProject.Tests.Consumer;

public class ProcessTransactionConsumerTests(ITestOutputHelper output)
{
    //[Fact]
    //public async Task Tests()
    //{
    //    await using var provider = new ServiceCollection()
    //        .AddTransient<ITransactionProcessor, TransactionProcessor>()
    //        .AddScoped<ITransactionProcessor, TransactionProcessor>()
    //        .AddScoped<ITransactionsStore, TransactionsStore>()
    //        .AddScoped<ICustomerSettingsStore, CustomerSettingsStore>()
    //        .AddMassTransitTestHarness(x =>
    //        {
    //            x.AddConsumer<ProcessTransactionConsumer>();
    //        })
    //        .BuildServiceProvider(true);

    //    var harness = provider.GetRequiredService<ITestHarness>();

    //    await harness.Start();

    //    try
    //    {
    //        var bus = harness.Bus;

    //        await bus.Publish<CreatedTransactionProcessRequest>(
    //            new(
    //                Guid.NewGuid(),
    //                Guid.NewGuid(),
    //                50,
    //                "okay country",
    //                "Zalando",
    //                "Mobile",
    //                "purchase",
    //                DateTime.UtcNow
    //                )
    //            );

    //        var consumerHarness = harness.GetConsumerHarness<ProcessTransactionConsumer>();
    //        var consumed = consumerHarness.Consumed;
    //        Assert.True(await consumerHarness.Consumed.Any<CreatedTransactionProcessRequest>());

    //        output.WriteLine($"Consumed messages: {consumed.Count()}");
    //    }
    //    finally
    //    {
    //        await harness.Stop();
    //        await provider.DisposeAsync();
    //    }
    //}
}
