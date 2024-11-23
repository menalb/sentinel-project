using FastEndpoints;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SentinelProject.Messages;

namespace SentinelProject.API.Features.ProcessTransaction;
public record ProcessTransactionRequest(
    [property: BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)] Guid TransactionId,
    [property: BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)] Guid UserId,
    decimal Amount,
    string Location,
    string Merchant,
    string Device,
    string TransactionType,
    DateTime IssuedAt
    );
public record ProcessTransactionResponse(string ResponseUrl);

[HttpPost("transactions")]
public class ProcessTransactionEndpoint(
    IMongoCollection<StoredProcessTransactionRequest> transactionsCollection,
    IPublishEndpoint publishEndpoint
    ) : Endpoint<ProcessTransactionRequest, Accepted>
{    
    public override async Task<Accepted> ExecuteAsync(ProcessTransactionRequest req, CancellationToken ct)
    {
        var message = new CreatedTransactionProcessRequest(
            req.TransactionId,
            req.UserId,
            req.Amount,
            req.Location,
            req.Merchant,
            req.Device,
            req.TransactionType,
            req.IssuedAt
            );

        var stored = new StoredProcessTransactionRequest
        {
            ProcessRequest = req,
            Status = "requested",
            CreatedAt = DateTime.UtcNow
        };
        await transactionsCollection.InsertOneAsync(stored, cancellationToken: ct);

        await publishEndpoint.Publish(message, ct);

        return TypedResults.Accepted($"transactions/{stored.Id}");
    }
}
