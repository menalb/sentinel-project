using SentinelProject.API.Features.ProcessTransaction;

namespace SentinelProject.Tests.Validators;

public class AnalyzeTransactionValidatorTests
{
    private readonly ProcessTransactionValidator validator = new();

    [Theory(DisplayName = "Invalid Transaction Amount")]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidTransactionAmount(decimal amount)
    {
        // Arrange
        var request = new ProcessTransactionRequest(Guid.NewGuid(), Guid.NewGuid(), amount, "Rome", "Merchant1", "Desktop", "Online");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Amount));
    }

    [Fact(DisplayName ="Valid Transaction")]
    public void ValidTransaction()
    {
        // Arrange
        var request = new ProcessTransactionRequest(Guid.NewGuid(), Guid.NewGuid(), 15, "Rome", "Merchant1", "Desktop", "Online");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }
}
