using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SentinelProject.Consumer.Core;
using SentinelProject.Consumer.Core.TransactionRules;
using SentinelProject.Messages;

namespace SentinelProject.Tests.Core;

public class TransactionProcessTests
{
    private readonly ICountriesStore _countriesStore;
    private readonly ICustomerSettingsStore _customerSettingsStore;
    private readonly ITransactionsStore _transactionsStore;

    private readonly TransactionProcessor _processor;
    public TransactionProcessTests()
    {
        _countriesStore = Substitute.For<ICountriesStore>();
        _customerSettingsStore = Substitute.For<ICustomerSettingsStore>();
        _transactionsStore = Substitute.For<ITransactionsStore>();

        _processor = new TransactionProcessor(
            [
            new TransactionCustomerSettingsProcessor(_customerSettingsStore),
            new TransactionCountryProcessor(_countriesStore),
            new TransactionPatternProcessor(_transactionsStore)
            ],
            _transactionsStore
            );

        _countriesStore.GetCountry(Arg.Any<string>()).Returns(new Country("TrustedLocation", 1));
        _customerSettingsStore.GetById(Arg.Any<Guid>()).Returns(new Customer(Guid.NewGuid(), "Joe Doe", 150));
    }

    [Theory(DisplayName = "When it is from an hostile country (trust rate < 0.3) It is rejected")]
    [InlineData(-1)]
    [InlineData(0.1)]
    [InlineData(0.2)]
    [InlineData(0.3)]
    public async Task When_It_Is_From_Hostile_Country_It_is_Rejected(float trustRate)
    {
        // Arrange
        var hostileCountry = "HostileCountry";
        var transaction = new CreatedTransactionProcessRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            50,
            hostileCountry,
            "Zalando",
            "Mobile",
            "purchase",
            DateTime.UtcNow
            );

        _countriesStore.GetCountry(transaction.Country).Returns(new Country(hostileCountry, trustRate));

        // Act
        var result = await _processor.Process(transaction);

        //Assert
        Assert.IsType<RejectedProcessTransactionResponse>(result);
        Assert.Equal("Hostile country", (result as RejectedProcessTransactionResponse)!.Reason);
    }

    [Theory(DisplayName = "When it is from a medium trust country (trust rate between 0.3 and 0.5) It is accepted with warning")]
    [InlineData(0.31)]
    [InlineData(0.4)]
    [InlineData(0.5)]
    public async Task When_It_Is_From_Medium_Trust_Country_It_is_Accepted_With_Warning(float trustRate)
    {
        // Arrange
        var warningCountry = "WarningCountry";
        var transaction = new CreatedTransactionProcessRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            50,
            warningCountry,
            "Zalando",
            "Mobile",
            "purchase",
            DateTime.UtcNow
            );

        _countriesStore.GetCountry(transaction.Country).Returns(new Country(warningCountry, trustRate));

        // Act
        var result = await _processor.Process(transaction);

        //Assert
        Assert.IsType<WarningProcessTransactionResponse>(result);
        Assert.Equal("Medium trust country", (result as WarningProcessTransactionResponse)!.Reason);
    }

    [Theory(DisplayName = "When it is from a trusted country (trust rate greater than 0.5) It is accepted")]
    [InlineData(0.6)]
    [InlineData(0.7)]
    [InlineData(0.9)]
    [InlineData(1)]
    public async Task When_It_Is_From_Trusted_Country_It_is_Accepted(float trustRate)
    {
        // Arrange
        var trustedCountry = "TrustedCountry";
        var transaction = new CreatedTransactionProcessRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            50,
            trustedCountry,
            "Zalando",
            "Mobile",
            "purchase",
            DateTime.UtcNow
            );

        _countriesStore.GetCountry(transaction.Country).Returns(new Country(trustedCountry, trustRate));

        // Act
        var result = await _processor.Process(transaction);

        //Assert
        Assert.IsType<AcceptedProcessTransactionResponse>(result);
    }

    [Fact(DisplayName = "When the the customer is not in the store, it is rejected")]
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
    public async Task When_More_Subsequent_Not_SmallTransactions_It_Is_Accepter_With_Warnings()
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

    [Fact(DisplayName = "Accepted transactions are stored")]
    public async Task Accepted_Transactions_Are_Stored()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var transaction = new CreatedTransactionProcessRequest(
            Guid.NewGuid(),
            customerId,
            1,
            "TrustedCountry",
            "Zalando",
            "Mobile",
            "purchase",
            DateTime.UtcNow
            );
        // Act
        var result = await _processor.Process(transaction);

        //Assert
        Assert.IsType<AcceptedProcessTransactionResponse>(result);
        await _transactionsStore
            .Received()
            .Store(
            new CustomerTransaction(
                transaction.TransactionId,
                transaction.UserId,
                transaction.Amount,
                transaction.Country,
                transaction.Merchant,
                transaction.Device,
                transaction.TransactionType,
                transaction.IssuesAt
                )
            );
    }
}

