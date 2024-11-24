namespace SentinelProject.Messages;

public abstract record PublishedTransactionResult(Guid TransactionId);
public record AcceptedTransactionResult(Guid TransactionId) : PublishedTransactionResult(TransactionId);
public record WarningTransactionResult(Guid TransactionId, string Message) : PublishedTransactionResult(TransactionId);
public record RejectedTransactionResult(Guid TransactionId, string Message) : PublishedTransactionResult(TransactionId);