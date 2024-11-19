using FastEndpoints;
using FluentValidation;

namespace SentinelProject.API.Features.GetResult;

public class GetProcessTransactionResultRequestValidator : Validator<GetProcessTransactionResultRequest>
{
    public GetProcessTransactionResultRequestValidator()
    {
        RuleFor(t => t.TransactionId).NotEmpty();
    }
}
