using MassTransit;
using SentinelProject.Messages;
using System;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Consumers;
public class ProcessTransactionConsumer : IConsumer<CreatedTransactionProcessRequest>
{
    public Task Consume(ConsumeContext<CreatedTransactionProcessRequest> context)
    {
        Console.WriteLine($"Request with transaction Id {context.Message.TransactionId} consumed, I got it");
        return Task.CompletedTask;
    }
}