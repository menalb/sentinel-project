using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace SentinelProject.API.Features;

public record AnalyzeTransactionRequest(Guid TransactionId, Guid UserId, decimal Amount, string Location, string Merchant, string Device, string TransactionType);

public record AnalyzeTransactionResponse(string ResponseUrl);

[HttpPost("transactions")]
[AllowAnonymous]
public class ProcessTransaction: Endpoint<AnalyzeTransactionRequest, AnalyzeTransactionResponse>
{
    public override Task<AnalyzeTransactionResponse> ExecuteAsync(AnalyzeTransactionRequest req, CancellationToken ct)
    {
        return base.ExecuteAsync(req, ct);
    }
}

public class AnalyzeTransactionValidator : Validator<AnalyzeTransactionRequest>
{
    public AnalyzeTransactionValidator()
    {
        RuleFor(t => t.Amount).GreaterThan(0);
    }
}
