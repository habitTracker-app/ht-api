using HTAPI.Data;
using HTAPI.Models.DemographicData;

namespace HTApi.Data.Repos
{
    public interface ICountryRepository
    {
        Country? GetCountry(int countryId);
    }
    public class CountryRepository : ICountryRepository
    {
        private static AppDbContext _db;
        public CountryRepository(IServiceProvider sp) {
            _db = sp.GetRequiredService<AppDbContext>();
        }

        public Country? GetCountry(int countryId)
        {
            if (_db.Country.Any(c => c.Id == countryId))
            {
                return _db.Country.Find(countryId);
            }
            return null;
        }
    }
}
