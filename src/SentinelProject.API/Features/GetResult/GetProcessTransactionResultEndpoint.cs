using FastEndpoints;
using MongoDB.Bson;
using MongoDB.Driver;
using SentinelProject.API.Features.ProcessTransaction;

namespace SentinelProject.API.Features.GetResult;

public record GetProcessTransactionResultRequest(string Id);
public record GetProcessTransactionResultResponse(string Id, string Outcome, string Message);

[HttpGet("transactions/{id}")]
public class GetProcessTransactionResultEndpoint(
    IMongoCollection<StoredProcessTransactionRequest> transactionsCollection
    )
    : Endpoint<GetProcessTransactionResultRequest, GetProcessTransactionResultResponse>
{
    public override async Task HandleAsync(GetProcessTransactionResultRequest req, CancellationToken ct)
    {
        var processId = ObjectId.Parse(req.Id);
        var t = await transactionsCollection
            .Find(t => t.Id == processId)
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (t == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(
            new GetProcessTransactionResultResponse(
                req.Id,
                t.Result?.Outcome ?? "",
                t.Result?.Message ?? ""
                ),
            cancellation: ct
        );
    }
}
