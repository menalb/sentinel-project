using FastEndpoints;
using FluentValidation;
using MongoDB.Bson;

namespace SentinelProject.API.Features.GetResult;

public class GetProcessTransactionResultRequestValidator : Validator<GetProcessTransactionResultRequest>
{
    public GetProcessTransactionResultRequestValidator()
    {
        RuleFor(t => t.Id)
            .NotEmpty()
            .Must(x => ObjectId.TryParse(x, out ObjectId id));
    }
}
