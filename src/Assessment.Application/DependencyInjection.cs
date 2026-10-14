using System.Reflection;
using Assessment.Application.Interfaces;
using Assessment.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Assessment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IDonorService, DonorService>();
        services.AddScoped<IDonationService, DonationService>();
        services.AddScoped<IDonorReportService, DonorReportService>();
        services.AddScoped<IVolunteerService, VolunteerService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenRevocationService, TokenRevocationService>();

        return services;
    }
}
