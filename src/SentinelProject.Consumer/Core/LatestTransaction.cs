using System;

namespace SentinelProject.Consumer.Core;

public record LatestTransaction(Guid TransactionId, Guid CustomerId, decimal Amount, DateTime IssuedAt);
