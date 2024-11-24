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

        var processResult = await transactionProcessor.Process(message);  

        await context.Publish(processResult switch
        {
            AcceptedProcessTransactionResponse => new AcceptedTransactionResult(message.TransactionId),
            WarningProcessTransactionResponse r => new WarningTransactionResult(message.TransactionId, r.Reason),
            RejectedProcessTransactionResponse r => new RejectedTransactionResult(message.TransactionId, r.Reason),
            _ => throw new ArgumentOutOfRangeException(),
        });
    }
}