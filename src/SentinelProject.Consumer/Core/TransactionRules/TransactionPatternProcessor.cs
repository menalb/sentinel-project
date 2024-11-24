using SentinelProject.Messages;
using System.Linq;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Core.TransactionRules;

public class TransactionPatternProcessor(ITransactionsStore transactionsStore) : ITransactionProcessingRule
{
    public async Task<ProcessTransactionResponse> Process(CreatedTransactionProcessRequest transaction)
    {
        if (transaction.Amount < 5)
        {
            var latestTransactions = await transactionsStore.GetLatestTransactionsForCustomer(transaction.UserId, 9);

            if (latestTransactions.Count == 9)
            {
                var time = transaction.IssuesAt.Subtract(latestTransactions[latestTransactions.Count - 1].IssuedAt);
                if (time.Minutes < 10 && latestTransactions.All(t => t.Amount <= 5))
                {
                    return new WarningProcessTransactionResponse(
                      transaction.TransactionId,
                      "Many small subsequent transactions"
                      );
                }
            }
        }
        return new AcceptedProcessTransactionResponse(transaction.TransactionId);
    }
}
