using System;

namespace SentinelProject.Consumer.Core;

public abstract record ProcessTransactionResponse(Guid TransactionId);
public record AcceptedProcessTransactionResponse(Guid TransactionId) : ProcessTransactionResponse(TransactionId);
public record WarningProcessTransactionResponse(Guid TransactionId, string Reason) : ProcessTransactionResponse(TransactionId);
public record RejectedProcessTransactionResponse(Guid TransactionId, string Reason) : ProcessTransactionResponse(TransactionId);