using ECommerce.API.Admin.Application.Mapping;
using ECommerce.API.Admin.Application.Validators;
using ECommerce.Application.Shared;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Controllers
// ---------------------------
builder.Services.AddControllers();

// ---------------------------
// Swagger/OpenAPI
// ---------------------------
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

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    });
});
builder.Services.AddScoped<IEmailService, SmtpEmailService>();



// Disable automatic model state validation → handled manually
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// ---------------------------
// Custom DependencyInjection Layer
// ---------------------------
builder.Services.AddECommerceCore(builder.Configuration);
builder.Services.AddECommerceIdentity(builder.Configuration);

// ---------------------------
// Build Application
// ---------------------------
var app = builder.Build();

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

app.MapControllers();

// ---------------------------
// Data Seeding (Roles + Admin User)
// ---------------------------
using (var scope = app.Services.CreateScope())
{
    await SeedDataAsync(scope.ServiceProvider);
}

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
