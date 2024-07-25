using HTApi.DTOs;
using HTAPI.Data;
using HTAPI.Models;
using HTAPI.Models.ActionModels;
using HTAPI.Models.DemographicData;
using Microsoft.AspNetCore.Identity;

namespace HTApi.Data.Repos
{
    public interface IUserRepository
    {
        UserDTO GetUser (int uuid);
        Task<UserDTO> CreateUser(RegisterUser body, Gender gender, Country country);
        Task UpdateUserActiveStatus(User user);
        Task<List<UserDTO>> GetAllUsers(int page, int itemsPerPage);
        Task DeleteUser(int uuid);
    }
    public class UserRepository : IUserRepository
    {
        public static AppDbContext _db;
        public static UserManager<User> _um;
        public static SignInManager<User> _sm;

        public UserRepository(IServiceProvider sp)
        {

            _db = sp.GetService<AppDbContext>();
            _um = sp.GetService<UserManager<User>>();
            _sm = sp.GetService<SignInManager<User>>();
        }
        public UserDTO GetUser (int uuid)
        {
            User user = _db.Users.First(u => u.UUID == uuid);

            UserDTO dto = new UserDTO(user);

            return dto;
        }

        public async Task<UserDTO> CreateUser(RegisterUser body, Gender gender, Country country)
        {
            User? user = null;

            user = new User
            {
                Email = body.Email,
                UserName = $"{body.FName}{body.LName}",
                Gender = gender, // todo
                Country = country, // todo
                AcceptedTerms = body.TermsAccepted,
                UserActive = true,
                BirthDate = body.BirthDate,
                FName = body.FName,
                LName = body.LName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            user.SetUID(_db);
            var res = await _um.CreateAsync(user, body.Password);
            if (res.Succeeded)
            {
                return new UserDTO(user);
            }
            else
            {
                string errors = "";
                foreach (var err in res.Errors)
                {
                    errors += $"{err.Description}--";
                }
                throw new Exception(errors);
            }
        }
           

        public async Task UpdateUserActiveStatus(User user)
        {
            user.ChangeUserActiveStatus();
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task<List<UserDTO>> GetAllUsers(int page, int itemsPerPage)
        {
            int userCount = _db.Users.Count();
            if(page > userCount/itemsPerPage) {
                throw new Exception("This page does not exist.");
            }
            if(page < 1)
            {
                throw new Exception("Pages must be 1 or higher.");
            }
            if(itemsPerPage > 50)
            {
                throw new Exception("Can only get a maximum of 50 users per page.");
            }

            int countToSkip = itemsPerPage * (page - 1);

            List<User> users = _db.Users.Skip(countToSkip).ToList();

            List<UserDTO> dtos = [];
            foreach (var user in users)
            {
                dtos.Add(new UserDTO(user));
            }

            return dtos;
        }

        public async Task DeleteUser(int uuid)
        {
            User? user = _db.Users.FirstOrDefault(u => u.UUID == uuid);

            if(user == null)
            {
                throw new Exception($"Unable to delete user {uuid}. User not found!");
            }
            try {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
                
            } catch(Exception e) {
                throw new Exception(e.Message);
            }

        }
    }
}
