using MongoDB.Driver;
using SentinelProject.Consumer.Core;
using System.Threading.Tasks;

namespace SentinelProject.Consumer.Infrastructure;

public class CountriesStore(IMongoDatabase database) : ICountriesStore
{
    private readonly IMongoCollection<StoredCountry> _countriesCollection =
      database.GetCollection<StoredCountry>("countries");

    public async Task<Country> GetCountry(string name)
    {
        var country = await _countriesCollection
            .Find(c => c.Name == name)
            .FirstOrDefaultAsync();

        return new Country(country.Name, country.TrustRate);
    }
}
