using HTApi.Models;
using HTAPI.Data;

namespace HTApi.Services
{
    public interface IValidationService
    {
        ValidationResult ValidateBirthDate(DateTime bd);
        ValidationResult ValidatePassword(string password, string confirmPswd);
        ValidationResult ValidateIfCanRegisterEmail(string email);
        ValidationResult ValidateIfCanSigninEmail(string email);
    }
    public class ValidationService : IValidationService
    {
        private static AppDbContext _db {  get; set; }

        public ValidationService(IServiceProvider sp)
        {
            _db = sp.GetService<AppDbContext>();
        }


        public ValidationResult ValidateBirthDate(DateTime bd)
        {
            ValidationResult result = new ValidationResult
            {
                IsValid = true,
                Messages = []
            };

            if (bd.Date >= DateTime.UtcNow.Date)
            {
                result.Invalidate("Birthdate must be before today.");
            }
            else
            {
                TimeSpan diff = DateTime.UtcNow - bd;

                int age = diff.Days / 365;
                if (age < 14)
                {
                    result.Invalidate("User must be at least 14 years old to register.");
                }
            }

            return result;
        }

        public ValidationResult ValidatePassword(string password, string confirmPswd)
        {
            ValidationResult result = new ValidationResult
            {
                IsValid = true,
                Messages = []
            };

            if (password == null)
            {
                result.Invalidate("Password cannot be null");
            }
            else
            {
                if (password.Length < 8)
                {
                    result.Invalidate("Password should have 8+ characters.");
                }

                if (!password.Any(char.IsUpper))
                {
                    result.Invalidate("Password must contain at least 1 uppercase letter.");
                }
                if (!password.Any(char.IsLower))
                {
                    result.Invalidate("Password must contain at least 1 lowercase letter.");
                }
                if (!password.Any(char.IsNumber))
                {
                    result.Invalidate("Password must contain at least 1 digit.");
                }
                if (confirmPswd == null)
                {
                    result.Invalidate("The field confirm password must be passed.");
                }
                if (confirmPswd != password)
                {
                    result.Invalidate("Passwords don't match.");
                }
            }

            return result;
        }

        public ValidationResult ValidateIfCanRegisterEmail(string email)
        {
            var result = new ValidationResult { IsValid = true, Messages = [] };
            var findUserByEmail = _db.Users.Any(u => u.Email == email);
            if (findUserByEmail)
            {
                result.Invalidate("This email is already registered.");
            }

            return result;
        }

        public ValidationResult ValidateIfCanSigninEmail(string email)
        {
            var result = new ValidationResult { IsValid = true, Messages = [] };
            var findUserByEmail = _db.Users.Any(u => u.Email == email);
            if (!findUserByEmail)
            {
                result.Invalidate("This email is not yet registered.");
            }

            return result;
        }
    }
}
