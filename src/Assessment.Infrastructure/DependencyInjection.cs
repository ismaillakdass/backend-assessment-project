using System.Text;
using Assessment.Domain.Entities.Identity;
using Assessment.Domain.Interfaces;
using Assessment.Infrastructure.Persistence;
using Assessment.Infrastructure.Repositories;
using Assessment.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Assessment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=losev_assessment.db";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        // Identity Configuration
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Repositories & Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IDonorRepository, DonorRepository>();
        services.AddScoped<IDonationRepository, DonationRepository>();
        services.AddScoped<IVolunteerRepository, VolunteerRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // JWT Authentication
        var jwtSecret = configuration["Jwt:Secret"] ?? "SuperSecretKeyForAssessmentTestingPurposes123!#";
        var key = Encoding.UTF8.GetBytes(jwtSecret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? "LosevAssessmentApi",
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"] ?? "LosevAssessmentClient",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Authorization & Permission Policy Provider
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            // Explicit policies can also be defined if desired
            options.AddPolicy("DonationsApprovePolicy", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Donations.Approve)));
        });

        return services;
    }
}
