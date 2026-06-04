using System.Text;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shop.Shared.Constants;
using Shop.Shared.Extensions;
using Shop.Shared.Interfaces;
using Shop.Shared.Middleware;
using Shop.Shared.Services;
using Shop.Shared.Settings;
using Shop.Users.Application.Interfaces;
using Shop.Users.Application.Services;
using Shop.Users.Application.Validators;
using Shop.Users.Domain.Models;
using Shop.Users.Domain.Settings;
using Shop.Users.Extensions;
using Shop.Users.Infrastructure.Data;
using Shop.Users.Infrastructure.Data.Seed;
using Shop.Users.Infrastructure.Publishers;
using StackExchange.Redis;
using EmailSendingService = Shop.Users.Infrastructure.Email.EmailSendingService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
builder.Configuration.AddEnvironmentVariables();

// debug
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerWithJwt();
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

if (!builder.Environment.IsEnvironment("Testing"))
{
    var redisSettings = builder.Configuration.GetRequiredSettings<RedisSettings>(RedisSettings.SectionName);
    var connectionMultiplexer = ConnectionMultiplexer.Connect(redisSettings.ConnectionUrl);
    builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
    builder.Services.AddSingleton<IRedisSessionService, RedisSessionService>();
    builder.Services.AddSingleton(redisSettings);
}

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailSendingService, EmailSendingService>();

builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IUserEventPublisher, UserEventPublisher>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IUserService, UserService>();

// messaging
builder.Services.AddMassTransit(x =>
{
    if (builder.Environment.IsEnvironment("Testing"))
        x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
    else
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"]!);
                h.Password(builder.Configuration["RabbitMq:Password"]!);
            });
            cfg.ConfigureEndpoints(ctx);
        });
});

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// jwt
builder.Services.AddJwtAuth(jwtSettings);

// app
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}
else 
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsEnvironment("Testing"))
    app.UseSessionValidation();

app.MapControllers();

await app.InitialiseDatabaseAsync();

app.Run();

namespace Shop.Users
{
    public partial class Program { }
}