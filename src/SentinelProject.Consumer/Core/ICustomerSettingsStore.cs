using System;

namespace SentinelProject.Consumer.Core;

public interface ICustomerSettingsStore
{
    Customer? GetById(Guid Id);  // TODO: Use Option instead of null
}
