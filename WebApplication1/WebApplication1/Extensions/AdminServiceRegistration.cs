using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1.Extensions
{
    public static class AdminServiceRegistration
    {
        public static IServiceCollection AddAdminServices(this IServiceCollection services)
        {
            // Đăng ký các Repository và Service dành riêng cho Admin ở đây.
            // Ví dụ:
            // services.AddScoped<IAdminService, AdminService>();
            
            return services;
        }
    }
}
