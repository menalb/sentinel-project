using FastEndpoints;

namespace SentinelProject.API.Features.GetResult;

public record GetProcessTransactionResultRequest(Guid TransactionId);
public record GetProcessTransactionResultResponse(bool Ok);

[HttpGet("transactions/{transactionId}")]
public class GetProcessTransactionResultEndpoint : Endpoint<GetProcessTransactionResultRequest, GetProcessTransactionResultResponse>
{
    public override async Task<GetProcessTransactionResultResponse> ExecuteAsync(GetProcessTransactionResultRequest req, CancellationToken ct)
    {
        return await Task.FromResult(new GetProcessTransactionResultResponse(false));
    }
}
