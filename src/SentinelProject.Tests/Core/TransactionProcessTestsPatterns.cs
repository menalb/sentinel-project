using NSubstitute;
using SentinelProject.Consumer.Core;
using SentinelProject.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentinelProject.Tests.Core;

public class TransactionProcessTestsPatterns: TransactionProcessTests
{

    [Fact(DisplayName = "When there are many (10) small (amount <= 5) subsequent transactions each within 1 minute of the other, It is accepted with warning")]
    public async Task When_More_Subsequent_Small_Transactions_It_Is_Accepter_With_Warnings()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var transaction = new CreatedTransactionProcessRequest(
            Guid.NewGuid(),
            customerId,
            1,
            "TrustedCountry",
            "Zalando",
            "Mobile",
            "purchase",
            now
            );

        _transactionsStore.GetLatestTransactionsForCustomer(customerId, 9).Returns(
        [
            new(Guid.NewGuid(),customerId, 1, now.AddMinutes(-1)),
            new(Guid.NewGuid(),customerId, 2, now.AddMinutes(-2)),
            new(Guid.NewGuid(),customerId, 3, now.AddMinutes(-3)),
            new(Guid.NewGuid(),customerId, 4, now.AddMinutes(-3.5)),
            new(Guid.NewGuid(),customerId, 4.5M, now.AddMinutes(-4)),
            new(Guid.NewGuid(),customerId, 4.5M, now.AddMinutes(-5)),
            new(Guid.NewGuid(),customerId, 4.5M, now.AddMinutes(-6)),
            new(Guid.NewGuid(),customerId, 4.5M, now.AddMinutes(-7)),
            new(Guid.NewGuid(),customerId, 4.5M, now.AddMinutes(-8))
        ]);

        // Act
        var result = await _processor.Process(transaction);

        //Assert
        Assert.IsType<WarningProcessTransactionResponse>(result);
        Assert.Equal("Many small subsequent transactions", (result as WarningProcessTransactionResponse)!.Reason);
    }

    [Fact(DisplayName = "When there are many (10) not small (at least one > 5) subsequent transactions each within 1 minute of the other, It is accepted")]
    public async Task When_More_Subsequent_Not_SmallTransactions_It_Is_Accepted()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var transaction = new CreatedTransactionProcessRequest(
            Guid.NewGuid(),
            customerId,
            1,
            "TrustedCountry",
            "Zalando",
            "Mobile",
            "purchase",
            now
            );

        _transactionsStore.GetLatestTransactionsForCustomer(customerId, 9).Returns(
        [
            new(Guid.NewGuid(),customerId, 2, now.AddMinutes(-1)),
            new(Guid.NewGuid(),customerId, 2, now.AddMinutes(-2)),
            new(Guid.NewGuid(),customerId, 3, now.AddMinutes(-3)),
            new(Guid.NewGuid(),customerId, 6, now.AddMinutes(-3.5)),
            new(Guid.NewGuid(),customerId, 4.5M, now.AddMinutes(-4)),
            new(Guid.NewGuid(),customerId, 2, now.AddMinutes(-5)),
            new(Guid.NewGuid(),customerId, 2, now.AddMinutes(-6)),
            new(Guid.NewGuid(),customerId, 2, now.AddMinutes(-7)),
            new(Guid.NewGuid(),customerId, 2, now.AddMinutes(-8)),
        ]);

        // Act
        var result = await _processor.Process(transaction);

        //Assert
        Assert.IsType<AcceptedProcessTransactionResponse>(result);
    }
}
