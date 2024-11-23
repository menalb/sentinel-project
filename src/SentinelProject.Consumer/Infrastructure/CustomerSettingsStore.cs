using MongoDB.Driver;
using SentinelProject.Consumer.Core;
using System;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Infrastructure;

public class CustomerSettingsStore(IMongoDatabase database) : ICustomerSettingsStore
{
    private readonly IMongoCollection<StoredCustomer> _customersCollection =
        database.GetCollection<StoredCustomer>("customers");

    public async Task<Customer?> GetById(Guid Id)
    {
        var customer = await _customersCollection
                    .Find(c => c.CustomerId == Id)
                    .FirstOrDefaultAsync();

        if(customer == null)
        {
            return null;
        }
        return new Customer(customer.CustomerId, customer.Name, customer.MaxTransactionAmount);
    }
}
