using MassTransit;
using Microsoft.Extensions.Logging;
using SentinelProject.Consumer.Core;
using SentinelProject.Messages;
using System;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Consumers;
public class ProcessTransactionConsumer(ITransactionProcessor transactionProcessor, ILogger<ProcessTransactionConsumer> logger) : IConsumer<CreatedTransactionProcessRequest>
{
    public async Task Consume(ConsumeContext<CreatedTransactionProcessRequest> context)
    {
        var message = context.Message;
        logger.LogInformation("Request with transaction Id {TransactionId} consumed, I got it", context.Message.TransactionId);

        var processResult = transactionProcessor.Process(message);

        var response = processResult switch
        {
            AcceptedProcessTransactionResponse => new ProcessedTransactionResult(message.TransactionId, "accepted", ""),
            RejectedProcessTransactionResponse r => new ProcessedTransactionResult(message.TransactionId, "rejected", r.Reason),
            WarningProcessTransactionResponse r => new ProcessedTransactionResult(message.TransactionId, "warning", r.Reason),
            _ => throw new ArgumentOutOfRangeException(),
        };

        await context.Publish(response);
    }
}