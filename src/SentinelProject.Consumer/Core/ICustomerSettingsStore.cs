using System;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Core;

public interface ICustomerSettingsStore
{
    Task<Customer>? GetById(Guid Id);  // TODO: Use Option instead of null
}
