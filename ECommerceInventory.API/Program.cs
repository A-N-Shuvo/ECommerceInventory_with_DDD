using AutoMapper;
using ECommerceInventory.API.Middleware;
using ECommerceInventory.Application.Interfaces;
using ECommerceInventory.Application.Mappings;
using ECommerceInventory.Application.Services;
using ECommerceInventory.Core.Entities;
using ECommerceInventory.Core.Interfaces;
using ECommerceInventory.Infrastructure.Data;
using ECommerceInventory.Infrastructure.Repositories;
using ECommerceInventory.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------- Services ---------------------

builder.Services.AddControllers();

// DbContext (SQL Server example)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("ECommerceInventory.Infrastructure")
    ));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Dependency Injection: UnitOfWork + Repositories + Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddMemoryCache();

// --------------------- Swagger ---------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Commerce Inventory API",
        Version = "v1",
        Description = "RESTful API for managing products and categories"
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // ✅ Include XML comments for controllers and models
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// --------------------- JWT Authentication ---------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? "please-change-this-secret-to-a-long-one");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// --------------------- Build App ---------------------

var app = builder.Build();

// --------------------- Middleware Pipeline ---------------------

// Exception Middleware should be first to catch all exceptions
app.UseMiddleware<ExceptionMiddleware>();

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce Inventory API V1");
        //c.RoutePrefix = string.Empty; // Swagger UI at root /
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --------------------- Seed Admin User ---------------------
using (var scope = app.Services.CreateScope())
{
    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var existing = uow.Users.GetByEmailAsync("admin@example.com").GetAwaiter().GetResult();
    if (existing == null)
    {
        var password = "Admin123!";
        using var hmac = new HMACSHA512();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        var admin = new User
        {
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "Admin"
        };

        uow.Users.AddAsync(admin).GetAwaiter().GetResult();
        uow.CompleteAsync().GetAwaiter().GetResult();
    }
}

// --------------------- Run ---------------------
app.Run();
