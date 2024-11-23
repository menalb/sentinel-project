using MassTransit;
using MongoDB.Driver;
using SentinelProject.API.Features.ProcessTransaction;
using SentinelProject.Messages;

namespace SentinelProject.API;

public class ProcessedTransactionResultConsumer(
    IMongoCollection<StoredProcessTransactionRequest> transactionsCollection,
    ILogger<ProcessedTransactionResultConsumer> logger
    ) : IConsumer<ProcessedTransactionResult>
{
    public async Task Consume(ConsumeContext<ProcessedTransactionResult> context)
    {
        var message = context.Message;
        logger.LogInformation("Request with transaction Id {TransactionId} consumed, I got it", context.Message.TransactionId);

        await transactionsCollection.UpdateOneAsync(
            Builders<StoredProcessTransactionRequest>.Filter.Eq(t => t.ProcessRequest.TransactionId, message.TransactionId),
            Builders<StoredProcessTransactionRequest>.Update
            .Set(t => t.Status, "processed")
            .Set(t => t.CompletedAt, DateTime.UtcNow)
            .Set(t=>t.Result, new ProcessResult
            {
                Outcome = message.Result,
                Message = message.Result != "accepted" ? message.Message : "",
            })
        );
    }
}
