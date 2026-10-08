using ITInternHub.Models.Entities;
using ITInternHub.Models.ViewModels;

namespace ITInternHub.Services
{
    public interface IAuthService
    {
        Task<List<Role>> GetRegistrableRolesAsync();
        Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string Message, AuthUserResult? UserResult)> ValidateLoginAsync(LoginViewModel model);
    }
}