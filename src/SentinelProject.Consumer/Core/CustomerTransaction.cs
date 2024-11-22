using System;

namespace SentinelProject.Consumer.Core;

public record CustomerTransaction(Guid TransactionId,
    Guid UserId,
    decimal Amount,
    string Country,
    string Merchant,
    string Device,
    string TransactionType,
    DateTime IssuesAt
    );
