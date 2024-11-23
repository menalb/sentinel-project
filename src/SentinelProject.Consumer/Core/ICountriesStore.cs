using System.Threading.Tasks;

namespace SentinelProject.Consumer.Core;

public interface ICountriesStore
{
    Task<Country> GetCountry(string name);
}
