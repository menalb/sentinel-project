using NSubstitute;
using SentinelProject.Messages;

namespace SentinelProject.Tests.ProcessTransaction;

public class TransactionProcessTests
{
    private readonly ICountriesStore _countriesStore;

    private readonly TransactionProcessor _processor;
    public TransactionProcessTests()
    {
        _countriesStore = Substitute.For<ICountriesStore>();
        _processor = new TransactionProcessor(_countriesStore);
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
}

public interface ICountriesStore
{
    Country GetCountry(string name);
}
public record Country(string Name, float TrustRate);

public class TransactionProcessor(ICountriesStore countryStore)
{
    public ProcessTransactionResponse Process(CreatedTransactionProcessRequest transaction)
    {
        var result = countryStore.GetCountry(transaction.Location);
        if (result.TrustRate <= 0.3f)
        {
            return new ProcessTransactionResponse(
                transaction.TransactionId,
                ProcessTransactionResults.Rejected,
                "Hostile country"
                );
        }

        if (result.TrustRate > 0.3f && result.TrustRate <= 0.5)
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
