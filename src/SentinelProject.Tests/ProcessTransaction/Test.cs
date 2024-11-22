using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ReturnsExtensions;
using SentinelProject.Messages;

namespace SentinelProject.Tests.ProcessTransaction;

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
            _customerSettingsStore,
            _countriesStore,
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
    public void When_It_Is_From_Hostile_Country_It_is_Rejected(float trustRate)
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

        _countriesStore.GetCountry(transaction.Location).Returns(new Country(hostileCountry, trustRate));

        // Act
        var result = _processor.Process(transaction);

        //Assert
        Assert.Equal(ProcessTransactionResults.Rejected, result.Result);
        Assert.Equal("Hostile country", result.Message);
    }

    [Theory(DisplayName = "When it is from a medium trust country (trust rate between 0.3 and 0.5) It is accepted with warning")]
    [InlineData(0.31)]
    [InlineData(0.4)]
    [InlineData(0.5)]
    public void When_It_Is_From_Medium_Trust_Country_It_is_Accepted_With_Warning(float trustRate)
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

        _countriesStore.GetCountry(transaction.Location).Returns(new Country(warningCountry, trustRate));

        // Act
        var result = _processor.Process(transaction);

        //Assert
        Assert.Equal(ProcessTransactionResults.Warning, result.Result);
        Assert.Equal("Medium trust country", result.Message);
    }

    [Theory(DisplayName = "When it is from a trusted country (trust rate greater than 0.5) It is accepted")]
    [InlineData(0.6)]
    [InlineData(0.7)]
    [InlineData(0.9)]
    [InlineData(1)]
    public void When_It_Is_From_Trusted_Country_It_is_Accepted(float trustRate)
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

        _countriesStore.GetCountry(transaction.Location).Returns(new Country(trustedCountry, trustRate));

        // Act
        var result = _processor.Process(transaction);

        //Assert
        Assert.Equal(ProcessTransactionResults.Accepted, result.Result);
    }

    [Fact(DisplayName = "When the the customer is not in the store, it is rejected")]
    public void When_Customer_DoesNot_Exist_It_Is_Rejected()
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
        var result = _processor.Process(transaction);

        //Assert
        Assert.Equal(ProcessTransactionResults.Rejected, result.Result);
        Assert.Equal("Customer not found", result.Message);
    }

    [Theory(DisplayName = "When the amount is too large for the customer's max transaction amount settings, it is rejected")]
    [InlineData(10, 100)]
    [InlineData(15000, 100000)]
    public void When_Amount_Is_Too_Big_For_Customer_It_is_Rejected(decimal customerMaxTransactionAmount, decimal amount)
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
        var result = _processor.Process(transaction);

        //Assert
        Assert.Equal(ProcessTransactionResults.Rejected, result.Result);
        Assert.Equal("Transaction too big", result.Message);
    }

    [Fact(DisplayName = "When there are many (5) small (amount <= 5) subsequent transactions each within 1 minute of the other, It is accepted with warning")]
    public void When_More_Subsequent_Small_Transactions_It_Is_Accepter_With_Warnings()
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
            DateTime.UtcNow
            );

        _transactionsStore.GetLatestTransactionsForCustomer(customerId, 5).Returns(
        [
            new(Guid.NewGuid(),customerId, 1, now.AddMinutes(-1)),
            new(Guid.NewGuid(),customerId, 2, now.AddMinutes(-2)),
            new(Guid.NewGuid(),customerId, 3, now.AddMinutes(-3)),
            new(Guid.NewGuid(),customerId, 4, now.AddMinutes(-3.5)),
            new(Guid.NewGuid(),customerId, 4.5M, now.AddMinutes(-5))
        ]);

        // Act
        var result = _processor.Process(transaction);

        //Assert
        Assert.Equal(ProcessTransactionResults.Warning, result.Result);
        Assert.Equal("Many small subsequent transactions", result.Message);
    }

    [Fact(DisplayName = "When there are many (5) not small (at least one > 5) subsequent transactions each within 1 minute of the other, It is accepted")]
    public void When_More_Subsequent_Not_SmallTransactions_It_Is_Accepter_With_Warnings()
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
            DateTime.UtcNow
            );

        _transactionsStore.GetLatestTransactionsForCustomer(customerId, 5).Returns(
        [
            new(Guid.NewGuid(),customerId, 1, now.AddMinutes(-1)),
            new(Guid.NewGuid(),customerId, 2, now.AddMinutes(-2)),
            new(Guid.NewGuid(),customerId, 3, now.AddMinutes(-3)),
            new(Guid.NewGuid(),customerId, 6, now.AddMinutes(-3.5)),
            new(Guid.NewGuid(),customerId, 4.5M, now.AddMinutes(-5))
        ]);

        // Act
        var result = _processor.Process(transaction);

        //Assert
        Assert.Equal(ProcessTransactionResults.Accepted, result.Result);
    }
}

public interface ITransactionsStore
{
    IReadOnlyList<LatestTransaction> GetLatestTransactionsForCustomer(Guid customerId, int howMany);
}

public record LatestTransaction(Guid TransactionId, Guid CustomerId, decimal Amount, DateTime IssuedAt);
public interface ICountriesStore
{
    Country GetCountry(string name);
}
public record Country(string Name, float TrustRate);

public interface ICustomerSettingsStore
{
    Customer? GetById(Guid Id);  // TODO: Use Option instead of null
}
public record Customer(Guid Id, string Name, decimal MaxTransactionAmount);
public class TransactionProcessor(
    ICustomerSettingsStore customerSettingsStore,
    ICountriesStore countryStore,
    ITransactionsStore transactionsStore
    )
{
    public ProcessTransactionResponse Process(CreatedTransactionProcessRequest transaction)
    {
        var customerSettings = customerSettingsStore.GetById(transaction.UserId);

        if (customerSettings == null)
        {
            return new ProcessTransactionResponse(
                transaction.TransactionId,
                ProcessTransactionResults.Rejected,
                "Customer not found"
                );
        }

        if (transaction.Amount > customerSettings.MaxTransactionAmount)
        {
            return new ProcessTransactionResponse(
                transaction.TransactionId,
                ProcessTransactionResults.Rejected,
                "Transaction too big"
                );
        }

        var country = countryStore.GetCountry(transaction.Location);
        if (country.TrustRate <= 0.3f)
        {
            return new ProcessTransactionResponse(
                transaction.TransactionId,
                ProcessTransactionResults.Rejected,
                "Hostile country"
                );
        }

        if (country.TrustRate > 0.3f && country.TrustRate <= 0.5)
        {
            return new ProcessTransactionResponse(
                transaction.TransactionId,
                ProcessTransactionResults.Warning,
                "Medium trust country"
                );
        }

        var latestTransactions = transactionsStore.GetLatestTransactionsForCustomer(transaction.UserId, 5);

        if (latestTransactions.Count == 5)
        {
            var time = latestTransactions[0].IssuedAt.Subtract(latestTransactions[latestTransactions.Count - 1].IssuedAt);
            if (time.Minutes < 5 && latestTransactions.All(t=>t.Amount <= 5))
            {
                return new ProcessTransactionResponse(
                  transaction.TransactionId,
                  ProcessTransactionResults.Warning,
                  "Many small subsequent transactions"
                  );
            }
        }

        return new ProcessTransactionResponse(transaction.TransactionId, ProcessTransactionResults.Accepted);
    }
}

public enum ProcessTransactionResults
{
    Accepted,
    Rejected,
    Warning
}

public record ProcessTransactionResponse(Guid TransactionId, ProcessTransactionResults Result, string? Message = "");
