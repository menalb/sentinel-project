using SentinelProject.Messages;
using System.Linq;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Core;

public interface ITransactionProcessor
{
    Task<ProcessTransactionResponse> Process(CreatedTransactionProcessRequest transaction);
}
public class TransactionProcessor(
    ICustomerSettingsStore customerSettingsStore,
    ICountriesStore countryStore,
    ITransactionsStore transactionsStore
    ) : ITransactionProcessor
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

        var country = await countryStore.GetCountry(transaction.Country);
        if (country.TrustRate <= 0.3f)
        {
            return new RejectedProcessTransactionResponse(
                transaction.TransactionId,
                "Hostile country"
                );
        }

        if (country.TrustRate > 0.3f && country.TrustRate <= 0.5)
        {
            return new WarningProcessTransactionResponse(
                transaction.TransactionId,
                "Medium trust country"
                );
        }

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