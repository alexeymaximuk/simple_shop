using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shop.Shared.Extensions;
using Shop.Shared.Middleware;
using Shop.Shared.Settings;
using Shop.Users.Application.Interfaces;
using Shop.Users.Application.Services;
using Shop.Users.Application.Validators;
using Shop.Users.Domain.Models;
using Shop.Users.Domain.Settings;
using Shop.Users.Infrastructure.Data;
using Shop.Users.Infrastructure.Data.Seed;
using EmailSendingService = Shop.Users.Infrastructure.Email.EmailSendingService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
builder.Configuration.AddEnvironmentVariables();

// debug
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(options =>
        {
            var securityDefinitionScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            };
            options.AddSecurityDefinition("Bearer", securityDefinitionScheme);

            var reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            };
            var securityRequirementsScheme = new OpenApiSecurityScheme {Reference = reference};
            var requirement = new OpenApiSecurityRequirement {{securityRequirementsScheme, []}};
            options.AddSecurityRequirement(requirement);
        }
    );
}

// mvc
builder.Services.AddControllers();

// validators
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserDtoValidator>();

// DI
var jwtSettings = builder.Configuration.GetRequiredSettings<JwtSettings>(JwtSettings.SectionName);
var appSettings = builder.Configuration.GetRequiredSettings<AppSettings>(AppSettings.SectionName);
var emailSettings = builder.Configuration.GetRequiredSettings<EmailSettings>(EmailSettings.SectionName);

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton(appSettings);
builder.Services.AddSingleton(emailSettings);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailSendingService, EmailSendingService>();

builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IProductServiceClient, ProductServiceClient>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IUserService, UserService>();

// connections
builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ProductsService:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("X-Internal-Key", builder.Configuration["InternalApiKey"]);
});

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// jwt
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            };
        }
    );


// app
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    if (!app.Environment.IsEnvironment("Testing"))
    {
        db.Database.Migrate();
    }

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    
    await AdminSeeder.SeedAsync(db, passwordHasher, config);
}

app.Run();

namespace Shop.Users
{
    public partial class Program { }
}