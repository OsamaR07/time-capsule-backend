using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Application.Mappings;
using TimeCapsule.Application.Services;
using TimeCapsule.Infrastructure.BackgroundServices;
using TimeCapsule.Infrastructure.Data;
using TimeCapsule.Infrastructure.Repositories;
using TimeCapsule.Infrastructure.Services;

namespace TimeCapsule.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration config)
    {
        var secret = config["JWT_SECRET"] ?? "default_secret_key_at_least_32_chars_long";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = "timecapsule-auth",
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICapsuleService, CapsuleService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        var host = config["DB_HOST"] ?? "localhost";
        var port = config["DB_PORT"] ?? "5432";
        var name = config["DB_NAME"] ?? "timecapsule";
        var user = config["DB_USERNAME"] ?? "postgres";
        var pass = config["DB_PASSWORD"] ?? "postgres";
        var connectionString = $"Host={host};Port={port};Database={name};Username={user};Password={pass}";

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITwilioService, TwilioService>();
        services.AddHttpClient<IWppService, WppConnectService>();
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<ICapsuleRepository, CapsuleRepository>();

        services.AddHostedService<CapsuleSchedulerService>();

        return services;
    }
}
