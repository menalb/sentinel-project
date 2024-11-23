using MassTransit.Initializers;
using MongoDB.Driver;
using SentinelProject.Consumer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Infrastructure;

public class TransactionsStore(IMongoDatabase database) : ITransactionsStore
{
    private readonly IMongoCollection<StoredCustomerTransaction> _customerTransactionsCollection = 
        database.GetCollection<StoredCustomerTransaction>("customer-transactions");

    public async Task<IReadOnlyList<LatestTransaction>> GetLatestTransactionsForCustomer(Guid customerId, int howMany)
    {
        var transactions = await _customerTransactionsCollection.Find(t => t.CustomerId == customerId).ToListAsync();
        return transactions
            .Select(t => new LatestTransaction(t.TransactionId, t.CustomerId, t.Amount, t.IssuesAt))
            .ToList();
    }

    public async Task Store(CustomerTransaction transaction)
    {
        var stored = new StoredCustomerTransaction
        {
            TransactionId = transaction.TransactionId,
            CustomerId = transaction.CustomerId,
            Amount = transaction.Amount,
            Country = transaction.Country,
            Merchant = transaction.Merchant,
            Device = transaction.Device,
            TransactionType = transaction.TransactionType,
            IssuesAt = transaction.IssuesAt,
        };

        await _customerTransactionsCollection.InsertOneAsync(stored);
    }
}