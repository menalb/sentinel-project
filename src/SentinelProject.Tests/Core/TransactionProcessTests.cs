using NSubstitute;
using SentinelProject.Consumer.Core;
using SentinelProject.Consumer.Core.TransactionRules;
using SentinelProject.Messages;

namespace SentinelProject.Tests.Core;

public abstract class TransactionProcessTests
{
    protected readonly ICountriesStore _countriesStore;
    protected readonly ICustomerSettingsStore _customerSettingsStore;
    protected readonly ITransactionsStore _transactionsStore;

    protected readonly TransactionProcessor _processor;
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
}

public class TransactionProcessTestsProcess : TransactionProcessTests
{
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
