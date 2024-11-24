using MassTransit;
using MongoDB.Driver;
using SentinelProject.API.Features.ProcessTransaction;
using SentinelProject.Messages;

namespace SentinelProject.API.Consumers;

public class RejectedTransactionResultConsumer(
    IMongoCollection<StoredProcessTransactionRequest> transactionsCollection,
    ILogger<RejectedTransactionResultConsumer> logger
    ) : IConsumer<RejectedTransactionResult>
{
    public async Task Consume(ConsumeContext<RejectedTransactionResult> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing Rejected Response with transaction Id {TransactionId}", context.Message.TransactionId);

        await transactionsCollection.UpdateOneAsync(
            Builders<StoredProcessTransactionRequest>.Filter.Eq(t => t.ProcessRequest.TransactionId, message.TransactionId),
            Builders<StoredProcessTransactionRequest>.Update
            .Set(t => t.Status, "processed")
            .Set(t => t.CompletedAt, DateTime.UtcNow)
            .Set(t => t.Result, new ProcessResult
            {
                Outcome = "Rejected",
                Message = message.Message,
            })
        );
    }
}
