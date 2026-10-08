using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProductCatalog.Api.Infrastructure;
using ProductCatalog.Api.Models.Api;
using ProductCatalog.Api.Models.Configuration;

var builder = WebApplication.CreateBuilder(args);

InjectionModule.ConfigureServices(builder.Services, builder.Configuration);

builder.Services.AddControllers(config =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    config.Filters.Add(new AuthorizeFilter(policy));
    config.Filters.Add<RestResponseExceptionFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = string.Join(
            " ",
            context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m)));

        return new OkObjectResult(
            RestResponse<object>.CreateErrorResponse(
                string.IsNullOrWhiteSpace(message) ? "Validation failed." : message));
    };
});

var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
          ?? throw new InvalidOperationException("Missing Jwt configuration.");

if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product Catalog API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter Bearer token after login."
    });

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
            .WithHeaders("Content-Type", "Idempotency-Key", "Accept", "Authorization")
            .WithMethods("GET", "POST", "OPTIONS")
            .WithExposedHeaders("Idempotency-Replayed");
    });
});

var app = builder.Build();

ProductCatalogSeeder.Seed(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("AngularDev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
