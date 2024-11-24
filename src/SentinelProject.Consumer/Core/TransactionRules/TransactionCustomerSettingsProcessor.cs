using SentinelProject.Messages;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Core.TransactionRules;

public class TransactionCustomerSettingsProcessor(ICustomerSettingsStore customerSettingsStore) : ITransactionProcessingRule
{
    public async Task<ProcessTransactionResponse> Process(CreatedTransactionProcessRequest transaction)
    {
        var customerSettings = await customerSettingsStore.GetById(transaction.UserId);

        if (customerSettings == null)
        {
            return new RejectedProcessTransactionResponse(
                transaction.TransactionId,
                "Customer not found"
                );
        }

        if (transaction.Amount > customerSettings.MaxTransactionAmount)
        {
            return new RejectedProcessTransactionResponse(
                transaction.TransactionId,
                "Transaction too big"
                );
        }
        return new AcceptedProcessTransactionResponse(transaction.TransactionId);
    }
}
