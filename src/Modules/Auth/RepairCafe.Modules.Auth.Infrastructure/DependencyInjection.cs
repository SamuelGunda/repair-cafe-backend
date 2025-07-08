using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RepairCafe.Modules.Auth.Application.Commands;
using RepairCafe.Modules.Auth.Application.Services;
using RepairCafe.Modules.Auth.Domain.Entities;
using RepairCafe.Modules.Auth.Infrastructure.Configurations;
using RepairCafe.Modules.Auth.Infrastructure.Persistence;
using RepairCafe.Modules.Auth.Infrastructure.Services;
using RepairCafe.Shared.Infrastructure.ModuleRegistry;
using RepairCafe.Shared.Infrastructure.Persistence;
using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Modules.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));
        
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<JwtSettings>>().Value);
        
        ModuleRegistry.RegisterDomainAssembly(typeof(User).Assembly);
        ModuleRegistry.RegisterApplicationAssembly(typeof(RegisterCommand).Assembly);
        
        services.AddScoped<ITokenService, TokenService>();
        
        services.AddDbContext<AuthDbContext>((sp, options) =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();

            if (env.IsDevelopment())
            {
                var dbPath = Path.Join(AppContext.BaseDirectory, "auth.db");
                options.UseSqlite($"Data Source={dbPath}");
                Console.WriteLine($"Using SQLite database at: {dbPath}");
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            }
        });
        
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AuthDbContext>());
        services.AddScoped<IOutboxDbContext>(provider => provider.GetRequiredService<AuthDbContext>());
        
        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
            
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddDefaultTokenProviders();
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var jwtSettings = serviceProvider.GetRequiredService<JwtSettings>();
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        return services;
    }
}
