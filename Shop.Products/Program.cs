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
using Shop.Products.Infrastructure.Consumers;
using Shop.Products.Infrastructure.Data;
using Shop.Shared.Extensions;
using Shop.Shared.Middleware;
using Shop.Shared.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

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

// database
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC 
builder.Services.AddControllers();

// validators
builder.Services.AddValidatorsFromAssemblyContaining<ProductInfoDtoValidator>();

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
            cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
            });
            cfg.ConfigureEndpoints(ctx);
        });
});

var jwtSettings = builder.Configuration.GetRequiredSettings<JwtSettings>(JwtSettings.SectionName);
builder.Services.AddSingleton(jwtSettings);

//JWT
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
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
    if (!app.Environment.IsEnvironment("Testing"))
    {
        db.Database.Migrate();
    }
}

app.Run();

namespace Shop.Products
{
    public partial class Program { }
}