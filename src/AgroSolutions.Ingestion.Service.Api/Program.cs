using Asp.Versioning;
using AgroSolutions.Ingestion.Service.Api.Middlewares;
using AgroSolutions.Ingestion.Service.Application.Interfaces;
using AgroSolutions.Ingestion.Service.Application.AppServices;
using AgroSolutions.Ingestion.Service.Infra.MongoDb;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

#region MongoDB

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase("agrosolutions_sensores");
builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);
builder.Services.AddScoped<IMongoDbService, MongoDbService>();

#endregion

#region RabbitMQ / MassTransit

builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["MessageBroker:Host"] ?? "localhost";
        var rabbitMqUsername = builder.Configuration["MessageBroker:Username"] ?? "admin";
        var rabbitMqPassword = builder.Configuration["MessageBroker:Password"] ?? "admin123";

        cfg.Host("localhost", 5672, "/", hostConfigurator =>
        {
            hostConfigurator.Username("admin");
            hostConfigurator.Password("admin123");
        });
    });
});

#endregion

#region DIs

builder.Services.AddScoped<IIngestionAppService, IngestionAppService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
#endregion

#region Swagger

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Serviço de Ingestão de Dados - AgroSolutions", Version = "v1", Description = "Este serviço recebe dados de sensores IoT." });
    c.EnableAnnotations();
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Insira o token JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            new[] { "Bearer" }
        }
    };

    c.AddSecurityRequirement(securityRequirement);
});

#endregion

#region Logging
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
#endregion

#region Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});
#endregion

#region Jwt
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? string.Empty)),
        RoleClaimType = ClaimTypes.Role,
    };
});

builder.Services.AddAuthorization();
#endregion

var app = builder.Build();

#region Middlewares

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

#endregion

app.Run();

public partial class Program { }

public class JwtSettings
{
    public string SecretKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpirationMinutes { get; set; }
}
