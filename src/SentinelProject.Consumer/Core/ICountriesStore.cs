namespace SentinelProject.Consumer.Core;

public interface ICountriesStore
{
    Country GetCountry(string name);
}
