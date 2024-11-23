using MassTransit;
using SentinelProject.Messages;

namespace SentinelProject.API;

public class ProcessedTransactionResultConsumer(ILogger<ProcessedTransactionResultConsumer> logger) : IConsumer<ProcessedTransactionResult>
{
    public Task Consume(ConsumeContext<ProcessedTransactionResult> context)
    {
        logger.LogInformation("Request with transaction Id {TransactionId} consumed, I got it", context.Message.TransactionId);

        return Task.CompletedTask;
    }
}
