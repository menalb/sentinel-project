using System;
using System.Collections.Generic;

namespace SentinelProject.Consumer.Core;

public interface ITransactionsStore
{
    IReadOnlyList<LatestTransaction> GetLatestTransactionsForCustomer(Guid customerId, int howMany);
    void Store(CustomerTransaction transaction);
}
