using HTAPI.Data;
using HTAPI.Models.DemographicData;
using System.Runtime.Intrinsics.Arm;

namespace HTApi.Data.Repos
{
    public interface IGenderRepository
    {
        Gender? GetGender(int genderId);
    }
    public class GenderRepository : IGenderRepository
    {
        public static AppDbContext _db;

        public GenderRepository(IServiceProvider sp)
        {
            _db = sp.GetService<AppDbContext>();
        }


        public Gender? GetGender(int genderId) {
            if (_db.Gender.Any(g => g.Id == genderId))
            {
                return _db.Gender.Find(genderId);
            }
            return null;
        }
    }
}
