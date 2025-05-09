using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Logging;
using System.Text;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories;
using Appointment.Infrastructure.Redis;

IdentityModelEventSource.ShowPII = true;
var builder = WebApplication.CreateBuilder(args);

// 🔌 Load PostgreSQL connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🧱 Register repositories
builder.Services.AddScoped<AppointmentRepository>();

// Add JWT Auth
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]);

Console.WriteLine("🔐 JWT KEY: " + jwtConfig["Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,       // you can enable this later if you want
            ValidateAudience = false,     // same as above
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key), // HS256 key here
        };

        options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"❌ JWT FAILED: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var claims = context.Principal?.Claims
                        .Select(c => $"{c.Type}: {c.Value}")
                        .ToList();

                    Console.WriteLine("✅ JWT token validated");
                    if (claims != null)
                    {
                        Console.WriteLine("🔐 Claims:");
                        foreach (var claim in claims)
                            Console.WriteLine($"   - {claim}");
                    }

                    return Task.CompletedTask;
                }
            };
});

builder.Services.AddAuthorization();

// Swagger & Controllers
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Appointment Service API",
        Version = "v1",
        Description = "API for managing doctor appointments"
    });

    // Add JWT Auth scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter: **Bearer &lt;your-token&gt;**"
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

builder.Services.AddSingleton<RedisPublisher>();

builder.WebHost.UseUrls("http://0.0.0.0:5243");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
