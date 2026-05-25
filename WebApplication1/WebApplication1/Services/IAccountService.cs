using WebApplication1.Models.Entities;
using WebApplication1.Models.ViewModels;

namespace WebApplication1.Services
{
    public interface IAccountService
    {
        Task<bool> RegisterCustomerAsync(RegisterViewModel model);
        Task<TaiKhoan?> ValidateLoginAsync(string usernameOrEmail, string password);
    }
}
