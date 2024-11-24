using NSubstitute;
using SentinelProject.Consumer.Core;
using SentinelProject.Messages;

namespace SentinelProject.Tests.Core;

public class TransactionProcessTestsCountry : TransactionProcessTests
{
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
}
