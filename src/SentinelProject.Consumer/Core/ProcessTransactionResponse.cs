using System;

namespace SentinelProject.Consumer.Core;

public record ProcessTransactionResponse(Guid TransactionId, ProcessTransactionResults Result, string? Message = "");