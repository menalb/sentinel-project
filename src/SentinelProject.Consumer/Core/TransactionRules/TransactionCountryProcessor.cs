using SentinelProject.Messages;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Core.TransactionRules;

public class TransactionCountryProcessor(ICountriesStore countryStore) : ITransactionProcessingRule
{
    public async Task<ProcessTransactionResponse> Process(CreatedTransactionProcessRequest transaction)
    {
        var country = await countryStore.GetCountry(transaction.Country);
        if(country == null)
        {
            return new RejectedProcessTransactionResponse(
               transaction.TransactionId,
               "Country not found"
               );
        }
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

        return new AcceptedProcessTransactionResponse(transaction.TransactionId);
    }
}
