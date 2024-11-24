using SentinelProject.Consumer.Core.TransactionRules;
using SentinelProject.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Core;

public interface ITransactionProcessor
{
    Task<ProcessTransactionResponse> Process(CreatedTransactionProcessRequest transaction);
}

public class TransactionProcessor(
    IList<ITransactionProcessingRule> rules,
    ITransactionsStore transactionsStore
    ) : ITransactionProcessor
{
    public async Task<ProcessTransactionResponse> Process(CreatedTransactionProcessRequest transaction)
    {
        foreach (var rule in rules)
        {
            var result = await rule.Process(transaction);
            if (result is not AcceptedProcessTransactionResponse)
            {
                return result;
            }
        }

        await transactionsStore.Store(new(
            transaction.TransactionId,
            transaction.UserId,
            transaction.Amount,
            transaction.Country,
            transaction.Merchant,
            transaction.Device,
            transaction.TransactionType,
            transaction.IssuesAt
            ));

        return new AcceptedProcessTransactionResponse(transaction.TransactionId);
    }
}