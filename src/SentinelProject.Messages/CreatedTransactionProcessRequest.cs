namespace SentinelProject.Messages;
public record CreatedTransactionProcessRequest(
    Guid TransactionId,
    Guid UserId,
    decimal Amount,
    string Country,
    string Merchant,
    string Device,
    string TransactionType,
    DateTime IssuesAt
    );
