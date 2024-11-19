using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SentinelProject.API.Features.ProcessTransaction;
public record ProcessTransactionRequest(Guid TransactionId, Guid UserId, decimal Amount, string Location, string Merchant, string Device, string TransactionType);
public record ProcessTransactionResponse(string ResponseUrl);

[HttpPost("transactions")]
public class ProcessTransactionEndpoint : Endpoint<ProcessTransactionRequest, Accepted>
{
    public override async Task<Accepted> ExecuteAsync(ProcessTransactionRequest req, CancellationToken ct)
    {
        var result = await Task.FromResult(new ProcessTransactionResponse($"transactions/{req.TransactionId}"));
        return TypedResults.Accepted($"transactions/{req.TransactionId}");
    }
}
