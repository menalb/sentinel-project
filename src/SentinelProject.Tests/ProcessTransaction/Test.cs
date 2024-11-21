using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SentinelProject.Messages;

namespace SentinelProject.Tests.ProcessTransaction;

public class TransactionProcessTests
{
    private readonly ICountriesStore _countriesStore;
    private readonly ICustomerSettingsStore _customerSettingsStore;

    private readonly TransactionProcessor _processor;
    public TransactionProcessTests()
    {
        _countriesStore = Substitute.For<ICountriesStore>();
        _customerSettingsStore = Substitute.For<ICustomerSettingsStore>();

        _processor = new TransactionProcessor(_customerSettingsStore, _countriesStore);

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
            "purchase"
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
            "purchase"
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
            "purchase"
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
            "purchase"
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
            "purchase"
            );

        _customerSettingsStore.GetById(customerId).Returns(new Customer(customerId, "Joe Doe", customerMaxTransactionAmount));

        // Act
        var result = _processor.Process(transaction);

        //Assert
        Assert.Equal(ProcessTransactionResults.Rejected, result.Result);
        Assert.Equal("Transaction too big", result.Message);
    }
}

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
    ICountriesStore countryStore
    )
{
    public ProcessTransactionResponse Process(CreatedTransactionProcessRequest transaction)
    {
        var customerSettings = customerSettingsStore.GetById(transaction.UserId);

        if( customerSettings == null)
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
