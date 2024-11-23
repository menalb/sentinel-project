namespace SentinelProject.Messages;

public record ProcessedTransactionResult(
    Guid TransactionId,
    string Result,
    string Message
    );