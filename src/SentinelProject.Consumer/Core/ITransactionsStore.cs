using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Core;

public interface ITransactionsStore
{
    Task<IReadOnlyList<LatestTransaction>> GetLatestTransactionsForCustomer(Guid customerId, int howMany);
    Task Store(CustomerTransaction transaction);
}
