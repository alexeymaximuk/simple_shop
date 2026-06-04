using System.Text;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shop.Products.Application.Interfaces;
using Shop.Products.Application.Services;
using Shop.Products.Application.Validators;
using Shop.Products.Extensions;
using Shop.Products.Infrastructure.Consumers;
using Shop.Products.Infrastructure.Data;
using Shop.Shared.Constants;
using Shop.Shared.Extensions;
using Shop.Shared.Interfaces;
using Shop.Shared.Middleware;
using Shop.Shared.Services;
using Shop.Shared.Settings;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

// debug
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerWithJwt();
}

// database
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC 
builder.Services.AddControllers();

// validators
builder.Services.AddValidatorsFromAssemblyContaining<ProductInfoDtoValidator>();

// DI
if (!builder.Environment.IsEnvironment("Testing"))
{
    var redisSettings = builder.Configuration.GetRequiredSettings<RedisSettings>(RedisSettings.SectionName);
    var connectionMultiplexer = ConnectionMultiplexer.Connect(redisSettings.ConnectionUrl);
    builder.Services.AddSingleton(redisSettings);
    builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
    builder.Services.AddSingleton<IRedisSessionService, RedisSessionService>();
}

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCommandService, ProductCommandService>();
builder.Services.AddScoped<IProductQueryService, ProductQueryService>();
builder.Services.AddScoped<IProductExternalServices, ProductExternalServices>();

// messaging
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserDeactivatedConsumer>();
    x.AddConsumer<UserActivatedConsumer>();
    x.AddConsumer<UserDeletedConsumer>();

    if (builder.Environment.IsEnvironment("Testing"))
        x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
    else
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMq:Host"]!, "/", h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"]!);
                h.Password(builder.Configuration["RabbitMq:Password"]!);
            });
            cfg.ConfigureEndpoints(ctx);
        });
});

var jwtSettings = builder.Configuration.GetRequiredSettings<JwtSettings>(JwtSettings.SectionName);
builder.Services.AddSingleton(jwtSettings);

// Jwt
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

namespace Shop.Products
{
    public partial class Program { }
}