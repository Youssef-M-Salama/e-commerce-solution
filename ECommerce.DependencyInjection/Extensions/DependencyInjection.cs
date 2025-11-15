using ECommerce.API.Admin.Application.Mapping;
using ECommerce.API.Admin.Application.Services;
using ECommerce.API.Admin.Application.Validators;
using ECommerce.Application.Services;
using ECommerce.Application.Shared;
using ECommerce.Customer.API.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Helpers;
using ECommerce.Domain.Interfaces.Repositories;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Helpers;
using ECommerce.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ECommerce.DependencyInjection.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddECommerceCore(this IServiceCollection services, IConfiguration configuration)
        {
            // ---------------------------
            // Database
            // ---------------------------
            services.AddDbContext<ECommerceDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("cs"),
                    sql => sql.MigrationsAssembly("ECommerce.Infrastructure")
                )
            );

            // ---------------------------
            // Repositories
            // ---------------------------
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<IFileStorage, FileSystemStorage>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserAddressRepository, UserAddressRepository>();
            services.AddScoped<IWishlistRepository, WishlistRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();

            // ---------------------------
            // Services (only core, not Admin-specific)
            // ---------------------------
            services.AddScoped<CategoryService>();
            services.AddScoped<BrandService>();
            services.AddScoped<ProductService>();
            services.AddScoped<ProductImageService>();
            services.AddScoped<ProductCategoryService>();
            services.AddScoped<UserAddressService>();
            services.AddScoped<CustomerAuthService>();
            services.AddScoped<WishlistService>();
            services.AddScoped<CartService>();
            services.AddScoped<CustomerOrderService>();
            services.AddScoped<AdminOrderService>();
            // ---------------------------
            // Email Service
            services.AddScoped<IEmailService, SmtpEmailService>();
            // ---------------------------
            // Admin Services
            // ---------------------------
            services.AddScoped<AdminUserService>();

            // ---------------------------
            // FluentValidation
            // ---------------------------
            services.AddFluentValidationAutoValidation();//its must be in the program.cs?
            services.AddValidatorsFromAssemblyContaining<CategoryDtoValidator>();


            // ---------------------------
            // AutoMapper
            // ---------------------------
            services.AddAutoMapper(typeof(AutoMapperProfiles));

            // ---------------------------
            // Helpers
            // ---------------------------
            services.AddScoped<JwtTokenHelper>();

            return services;
        }

        public static IServiceCollection AddECommerceIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            // ---------------------------
            // Identity
            // ---------------------------
            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ECommerceDbContext>()
            .AddDefaultTokenProviders();

            // ---------------------------
            // JWT Authentication
            // ---------------------------
            var jwtSettings = configuration.GetSection("JwtSettings");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)
                    )
                };
            });

            return services;
        }
    }
}
