using HTAPI.Models.DemographicData;
using System.Diagnostics.Eventing.Reader;

namespace HTAPI.Data
{
    public class DBInitializer
    {
        private static IServiceProvider _sp;

        private static DefaultData _data = new DefaultData();

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            _sp = serviceProvider;
            await CreateCountries();
            await CreateGenders();
            await CreateCategories();
        }

        private static async Task CreateGenders()
        {
            var db = _sp.GetService<AppDbContext>();
            bool gendersExist = db.Gender.Any();

            if(!gendersExist)
            {
                foreach(var g in _data.Genders)
                {
                    await db.Gender.AddAsync(g);
                }

                await db.SaveChangesAsync();
            }
        } 

        private static async Task CreateCountries()
        {
            var db = _sp.GetService<AppDbContext>();
            bool countriesExist = db.Country.Any();
            if (!countriesExist)
            {
                foreach(var country in _data.Countries)
                {
                    await db.Country.AddAsync(country);
                }
                await db.SaveChangesAsync();
            }
        }

        private static async Task CreateCategories()
        {
            var db = _sp.GetService<AppDbContext>();

            if (!db.ChallengeCategory.Any())
            {
                foreach(var item in _data.ChallengeCategories)
                {
                    await db.ChallengeCategory.AddAsync(item);
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
