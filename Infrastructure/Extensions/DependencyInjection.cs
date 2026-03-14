using Domain.Model.Dto.Compra;
using Domain.Model.Dto.Venta;
using FluentValidation;
using Infrastructure.Kafka;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Services;
using Infrastructure.Services.Interface;
using Infrastructure.Services.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<CompraRepository>();
            services.AddScoped<VentaRepository>();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<CompraService>();
            services.AddScoped<VentaService>();
            services.AddScoped<IMasterData, MasterDataValidationService>();
            services.AddScoped<AdminApiClient>();
            services.AddScoped<IAdminCompraApiClient>(sp => sp.GetRequiredService<AdminApiClient>());
            services.AddScoped<IAdminVentaApiClient>(sp => sp.GetRequiredService<AdminApiClient>());
            services.AddScoped<IInventoryClient, InventoryApiClient>();
            services.AddScoped<IAuthApiClient, AuthApiClient>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<Compras>, CompraValidator>();
            services.AddScoped<IValidator<Ventas>, VentaValidator>();
            return services;
        }
    }
}
