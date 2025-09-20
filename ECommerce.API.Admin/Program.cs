// ---------------------------
// External & Framework Usings
// ---------------------------
using Microsoft.OpenApi.Models;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.API.Admin.Application.Mapping;
// ---------------------------
// Application Layer Usings
// ---------------------------
using ECommerce.API.Admin.Application.Services;
using ECommerce.API.Admin.Application.Validators;
using ECommerce.Application.Services;
// ---------------------------
// Domain Layer Usings
// ---------------------------
using ECommerce.Domain.Entities;
using ECommerce.Domain.Helpers;
using ECommerce.Domain.Repositories;
// ---------------------------
// Infrastructure Layer Usings
// ---------------------------
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Helpers;
using ECommerce.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Add Services to the Container
// ---------------------------

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }
};


    c.AddSecurityRequirement(securityRequirement);
});



// ---------------------------
// Database Configuration
// ---------------------------
builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("cs"),
        sql => sql.MigrationsAssembly("ECommerce.Infrastructure")
    )
);

// ---------------------------
// Repository Registrations
// ---------------------------
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IFileStorage, FileSystemStorage>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();


// ---------------------------
// Service Registrations
// ---------------------------
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<BrandService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductImageService>();
builder.Services.AddScoped<ProductCategoryService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<UserAddressService>();


// ---------------------------
// AutoMapper
// ---------------------------
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// ---------------------------
// FluentValidation
// ---------------------------
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CategoryDtoValidator>();

// Disable automatic model state validation → handled manually
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// ---------------------------
// Identity Configuration
// ---------------------------
builder.Services.AddIdentity<User, Role>(options =>
{
    // Password rules
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User & Sign-in settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ECommerceDbContext>()
.AddDefaultTokenProviders();

// ---------------------------
// JWT Authentication
// ---------------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
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
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
    };
});

// ---------------------------
// Build Application
// ---------------------------
var app = builder.Build();

//----------------------------
// Data seeding
await SeedDataAsync(app.Services);

//----------------------------


// ---------------------------
// Middleware Pipeline
// ---------------------------
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
using (var scope = app.Services.CreateScope())
{
    await SeedDataAsync(scope.ServiceProvider);
}



app.MapControllers();

app.Run();

// ---------------------------
// Seed Initial Data (Roles + Admin User)
// ---------------------------
async Task SeedDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    // Create roles
    var roles = new[]
    {
        new { Name = "SuperAdmin", Description = "Super Administrator with full access" },
        new { Name = "Admin", Description = "Administrator with management access" },
        new { Name = "Customer", Description = "Customer with limited access" }
    };

    foreach (var roleData in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleData.Name))
        {
            await roleManager.CreateAsync(new Role
            {
                Name = roleData.Name,
                Description = roleData.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    // Create SuperAdmin user
    var superAdminEmail = "superadmin@ecommerce.com";
    var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
    if (superAdminUser == null)
    {
        superAdminUser = new User
        {
            UserName = "SuperAdmin",
            Email = superAdminEmail,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,    
            UpdatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(superAdminUser, "SuperAdmin123!");
        await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
    }
}