using FastEndpoints;
using FluentValidation;

namespace SentinelProject.API.Features.ProcessTransaction;

public class ProcessTransactionValidator : Validator<ProcessTransactionRequest>
{
    public ProcessTransactionValidator()
    {
        RuleFor(t => t.Amount).GreaterThan(0);
    }
}
