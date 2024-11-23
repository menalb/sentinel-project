using MassTransit;
using SentinelProject.Consumer.Core;
using SentinelProject.Messages;
using System;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Consumers;
public class ProcessTransactionConsumer(ITransactionProcessor transactionProcessor) : IConsumer<CreatedTransactionProcessRequest>
{
    public Task Consume(ConsumeContext<CreatedTransactionProcessRequest> context)
    {
        Console.WriteLine($"Request with transaction Id {context.Message.TransactionId} consumed, I got it");

        var result = transactionProcessor.Process(context.Message);

        return Task.CompletedTask;
    }
}