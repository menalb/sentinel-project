using SentinelProject.Messages;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Core.TransactionRules;

public interface ITransactionProcessingRule
{
    public Task<ProcessTransactionResponse> Process(CreatedTransactionProcessRequest transaction);
}