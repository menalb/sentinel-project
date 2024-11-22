using System;

namespace SentinelProject.Consumer.Core;

public record Customer(Guid Id, string Name, decimal MaxTransactionAmount);
