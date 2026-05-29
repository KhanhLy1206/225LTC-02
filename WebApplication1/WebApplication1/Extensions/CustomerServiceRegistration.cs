using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1.Extensions
{
    public static class CustomerServiceRegistration
    {
        public static IServiceCollection AddCustomerServices(this IServiceCollection services)
        {
            // Đăng ký các Repository và Service dành riêng cho Khách hàng ở đây.
            // Ví dụ:
            // services.AddScoped<ICustomerService, CustomerService>();
            
            return services;
        }
    }
}
