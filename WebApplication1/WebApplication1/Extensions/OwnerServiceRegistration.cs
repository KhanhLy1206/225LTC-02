using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1.Extensions
{
    public static class OwnerServiceRegistration
    {
        public static IServiceCollection AddOwnerServices(this IServiceCollection services)
        {
            // Đăng ký các Repository và Service dành riêng cho Chủ bãi xe ở đây.
            // Ví dụ:
            // services.AddScoped<IOwnerService, OwnerService>();
            
            return services;
        }
    }
}
