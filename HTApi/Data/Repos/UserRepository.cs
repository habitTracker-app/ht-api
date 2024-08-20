using HTApi.DTOs;
using HTApi.Models.ActionModels;
using HTApi.Models.Exceptions;
using HTAPI.Data;
using HTAPI.Models;
using HTAPI.Models.ActionModels;
using HTAPI.Models.DemographicData;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace HTApi.Data.Repos
{
    public interface IUserRepository
    {
        UserDTO GetUser (int uuid);
        Task<UserDTO> CreateUser(RegisterUser body, Gender gender, Country country);
        Task UpdateUserActiveStatus(User user);
        List<UserDTO> GetAllUsers(int page, int itemsPerPage);
        Task DeleteUser(int uuid);

        Task UpdateUser(User user, UpdateUserInfo data, Gender gender, Country country);
        Task ChangeUserPassword(ChangePassword body, User user);
    }
    public class UserRepository : IUserRepository
    {
        public static AppDbContext? _db;
        public static UserManager<User>? _um;
        public static SignInManager<User>? _sm;

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
                UserName = NormalizeString($"{body.FName}{body.LName}"),
                Gender = gender, 
                Country = country, 
                AcceptedTerms = body.TermsAccepted,
                UserActive = true,
                BirthDate = body.BirthDate,
                FName = body.FName,
                LName = body.LName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            this.SetUID(user);
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
                    errors += $"{err.Description};";
                }
                throw new BadRequestException(errors, 400);
            }
        }
           

        public async Task UpdateUserActiveStatus(User user)
        {
            try
            {
                user.UserActive = !user.UserActive;
                user.UpdatedAt = DateTime.UtcNow;
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<UserDTO> GetAllUsers(int page, int itemsPerPage)
        {
            int userCount = _db.Users.Count();
            if(page > userCount/itemsPerPage) {
                throw new BadRequestException("This page does not exist.", 404);
            }
            if(page < 1)
            {
                throw new BadRequestException("Pages must be 1 or higher.", 406);
            }
            if(itemsPerPage > 50)
            {
                throw new BadRequestException("Can only get a maximum of 50 users per page.", 406);
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
                throw new BadRequestException($"Unable to delete user {uuid}. User not found!", 404);
            }
            try {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
                
            } catch(Exception e) {
                throw new Exception(e.Message);
            }
        }

        public async Task UpdateUser(User user, UpdateUserInfo data, Gender gender, Country country)
        {
            try
            {
                user.Email = data.Email;
                user.NormalizedEmail = this.NormalizeString(data.Email).ToUpper();
                user.LName = data.LName;
                user.FName = data.FName;
                user.BirthDate = data.BirthDate;
                user.Country = country;
                user.Gender = gender;
                user.UserName = this.NormalizeString($"{data.FName}{data.LName}");
                user.NormalizedUserName = this.NormalizeString(user.UserName).ToUpper();

                DateTime now = DateTimeOffset.UtcNow.Date;
                user.UpdatedAt = now;

                _db.Users.Update(user);
                await _db.SaveChangesAsync();
            }catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task ChangeUserPassword(ChangePassword body, User user)
        {
            try
            {
                var result = await _um.ChangePasswordAsync(user, body.OldPassword, body.NewPassword);
                if (!result.Succeeded)
                {
                    List<String> errors = new List<String>();
                    foreach (var error in result.Errors)
                    {
                        errors.Add(error.Description);
                    }
                    throw new BadRequestException(String.Join("; ", errors), 400);
                }
            }
            catch (BadRequestException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }


        private string NormalizeString(string input)
        {
            // Use a regular expression to remove any character that is not a letter or digit
            return Regex.Replace(input, @"[^a-zA-Z0-9]", "");
        }
        private void SetUID(User user)
        {
            if (!_db.Users.Any() || _db.Users.ToList().Count() == 0)
            {
                user.UUID = 10001;
            }
            else
            {
                int maxId = _db.Users.Max(u => u.UUID);
                user.UUID = maxId + 1;
            }
        }
    }
}
