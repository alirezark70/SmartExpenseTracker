using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;
using SmartExpenseTracker.Infra.Mapping.Contracts;
using SmartExpenseTracker.Infra.Mapping.Services;
using SmartExpenseTracker.Infra.Persistence.Configuration;
using SmartExpenseTracker.Infra.Persistence.Context;
using SmartExpenseTracker.Infra.Persistence.Services.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Extensions.DependencyInjection
{
    public static class PersistenceServiceRegistrer
    {
        public static IServiceCollection RegisterPersistenceService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IJwtSettings, JwtSettings>();

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // تنظیمات رمز عبور
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                // تنظیمات کاربر
                options.User.RequireUniqueEmail = true;

                // تنظیمات قفل شدن حساب
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            }) .AddEntityFrameworkStores<WriteDbContext>().AddDefaultTokenProviders();


            IConfigurationSection jwtSetting = configuration.GetSection(JwtSettings.SectionName);

            var jwtSettingsValue = jwtSetting.Get<JwtSettings>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // در production باید true باشد
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettingsValue!.Issuer,
                    ValidAudience = jwtSettingsValue.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettingsValue.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }
    }
}
