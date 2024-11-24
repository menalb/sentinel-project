using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SentinelProject.Consumer.Core;
using SentinelProject.Messages;

namespace SentinelProject.Tests.Core;

public class TransactionProcessTestsCustomerSettings : TransactionProcessTests
{
    [Fact(DisplayName = "When the customer is not in the store, it is rejected")]
    public async Task When_Customer_DoesNot_Exist_It_Is_Rejected()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var transaction = new CreatedTransactionProcessRequest(
            Guid.NewGuid(),
            customerId,
            100,
            "TrustedCountry",
            "Zalando",
            "Mobile",
            "purchase",
            DateTime.UtcNow
            );

        _customerSettingsStore.GetById(customerId).ReturnsNull();

        // Act
        var result = await _processor.Process(transaction);

        //Assert
        Assert.IsType<RejectedProcessTransactionResponse>(result);
        Assert.Equal("Customer not found", (result as RejectedProcessTransactionResponse)!.Reason);
    }

    [Theory(DisplayName = "When the amount is too large for the customer's max transaction amount settings, it is rejected")]
    [InlineData(20, 100)]
    [InlineData(15000, 100000)]
    public async Task When_Amount_Is_Too_Big_For_Customer_It_is_Rejected(decimal customerMaxTransactionAmount, decimal amount)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var transaction = new CreatedTransactionProcessRequest(
            Guid.NewGuid(),
            customerId,
            amount,
            "TrustedCountry",
            "Zalando",
            "Mobile",
            "purchase",
            DateTime.UtcNow
            );

        _customerSettingsStore.GetById(customerId).Returns(new Customer(customerId, "Joe Doe", customerMaxTransactionAmount));

        // Act
        var result = await _processor.Process(transaction);

        //Assert
        Assert.IsType<RejectedProcessTransactionResponse>(result);
        Assert.Equal("Transaction too big", (result as RejectedProcessTransactionResponse)!.Reason);
    }
}
