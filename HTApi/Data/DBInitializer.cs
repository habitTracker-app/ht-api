using HTAPI.Models.DemographicData;
using System.Diagnostics.Eventing.Reader;

namespace HTAPI.Data
{
    public class DBInitializer
    {
        private static IServiceProvider _sp;
        private static AppDbContext _db;

        private static DefaultData _data = new DefaultData();

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            _sp = serviceProvider;
            _db = _sp.GetService<AppDbContext>() ?? throw new Exception("Service unavailable.");

            await CreateCountries();
            await CreateGenders();
            await CreateCategories();
            await CreateFriendshipStatuses();
        }

        private static async Task CreateGenders()
        {
            bool gendersExist = _db.Gender.Any();

            if (!gendersExist)
            {
                foreach (var g in _data.Genders)
                {
                    await _db.Gender.AddAsync(g);
                }

                await _db.SaveChangesAsync();
            }
        }

        private static async Task CreateCountries()
        {
            bool countriesExist = _db.Country.Any();
            if (!countriesExist)
            {
                foreach (var country in _data.Countries)
                {
                    await _db.Country.AddAsync(country);
                }
                await _db.SaveChangesAsync();
            }
        }

        private static async Task CreateCategories()
        {

            if (!_db.ChallengeCategory.Any())
            {
                foreach (var item in _data.ChallengeCategories)
                {
                    await _db.ChallengeCategory.AddAsync(item);
                }
                await _db.SaveChangesAsync();
            }
        }

        private static async Task CreateFriendshipStatuses()
        {
            if (!_db.FriendshipStatus.Any())
            {
                foreach (var item in _data.FriendshipStatuses)
                {
                    await _db.FriendshipStatus.AddAsync(item);
                }
                await _db.SaveChangesAsync();
            }
        }
    }
}
