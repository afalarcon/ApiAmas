using System.Threading.RateLimiting;
using System.Text;
using Amas.Api.Configuration;
using Amas.Api.Contracts;
using Amas.Api.Options;
using Amas.Application;
using Amas.Infrastructure;
using Amas.Infrastructure.Redis;
using Amas.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste only the JWT token. Swagger will send it as Bearer."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
    });
});
builder.Services.AddOpenApi();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection("RateLimiting"));

var rateLimitOptions = builder.Configuration.GetSection("RateLimiting").Get<RateLimitOptions>() ?? new RateLimitOptions();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.Failure("Too many requests. Try again later."),
            cancellationToken);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartition(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.GlobalPermitLimit,
                Window = TimeSpan.FromSeconds(rateLimitOptions.GlobalWindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddFixedWindowLimiter("auth-login", limiter =>
    {
        limiter.PermitLimit = rateLimitOptions.LoginPermitLimit;
        limiter.Window = TimeSpan.FromSeconds(rateLimitOptions.LoginWindowSeconds);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });

    options.AddPolicy("contact-public", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartition(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.ContactPermitLimit,
                Window = TimeSpan.FromSeconds(rateLimitOptions.ContactWindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AmasCors", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
{
    throw new InvalidOperationException("Jwt:Secret is required.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
        };
    });

var postgres = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();
var redisConnection = RedisConnectionStringBuilder.Build(redisOptions);

builder.Services.AddHealthChecks()
    .AddNpgSql(postgres, name: "postgres")
    .AddRedis(redisConnection, name: "redis");

var app = builder.Build();

var swaggerEnabled = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Enabled");
if (swaggerEnabled)
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

var mediaStorageOptions = builder.Configuration.GetSection("MediaStorage").Get<MediaStorageOptions>() ?? new MediaStorageOptions();
if (string.Equals(mediaStorageOptions.Provider, "Local", StringComparison.OrdinalIgnoreCase))
{
    var mediaRoot = LocalImageStorage.GetRootPath(mediaStorageOptions.LocalPath);
    Directory.CreateDirectory(mediaRoot);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(mediaRoot),
        RequestPath = GetMediaRequestPath(mediaStorageOptions.PublicBaseUrl)
    });
}

app.UseCors("AmasCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

static string GetMediaRequestPath(string publicBaseUrl)
{
    if (Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var uri))
    {
        return uri.AbsolutePath;
    }

    return publicBaseUrl.StartsWith('/') ? publicBaseUrl : $"/{publicBaseUrl}";
}

static string GetClientPartition(HttpContext context)
{
    var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    var clientIp = forwardedFor?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
        ?? context.Connection.RemoteIpAddress?.ToString();

    return string.IsNullOrWhiteSpace(clientIp) ? "unknown-client" : clientIp;
}

public partial class Program;
