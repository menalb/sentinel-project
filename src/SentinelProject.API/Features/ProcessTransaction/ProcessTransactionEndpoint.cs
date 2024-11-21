using FastEndpoints;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using SentinelProject.Messages;

namespace SentinelProject.API.Features.ProcessTransaction;
public record ProcessTransactionRequest(Guid TransactionId, Guid UserId, decimal Amount, string Location, string Merchant, string Device, string TransactionType);
public record ProcessTransactionResponse(string ResponseUrl);

[HttpPost("transactions")]
public class ProcessTransactionEndpoint(IPublishEndpoint publishEndpoint) : Endpoint<ProcessTransactionRequest, Accepted>
{
    public override async Task<Accepted> ExecuteAsync(ProcessTransactionRequest req, CancellationToken ct)
    {
        var message = new CreatedTransactionProcessRequest(req.TransactionId, req.UserId, req.Amount, req.Location, req.Merchant, req.Device, req.TransactionType);
        await publishEndpoint.Publish(message, ct);

        return TypedResults.Accepted($"transactions/{req.TransactionId}");
    }
}
