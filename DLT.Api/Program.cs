using System.Security.Claims;
using System.Text;
using Common;
using DLT.Api.Helper;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Implementation;
using DLT.Service.Repository.Interface;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.Models.SpDbContext;
using Serilog;

namespace DLT.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<IDriverRepository, DriverRepository>();
        builder.Services.AddScoped<ITripRepository, TripRepository>();
        builder.Services.AddScoped<ILocationRepository, LocationRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        
        //JWT service 
        builder.Services.AddSingleton<AuthenticationRepository>();

        
        // Connection String
        var connectionString = builder.Configuration.GetConnectionString("DBConnection");

        // Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .CreateLogger();

        // FluentValidation
        builder.Services.AddControllers()
            .AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssembly(AppDomain.CurrentDomain.GetAssemblies()
                    .SingleOrDefault(assembly => assembly.GetName().Name == typeof(Program).Assembly.GetName().Name));
            });

        builder.Services.AddDbContext<DriverLocationTrackingDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);
    
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
 
            // Add JWT Bearer Auth
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token like: Bearer {your token}"
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
                    new string[] {}
                }
            });
        });
        
        builder.Services.AddDbContext<DriverLocationTrackingSpContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);

        // ✅ Register CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            // (Better for production)
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(
                    "http://localhost:3000",
                    "http://192.168.1.119:3000",
                    "http://192.168.1.119:3000"
                )
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
        });
        // Add JWT services

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal.Identity as ClaimsIdentity;
                        var userSid = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var role = identity.FindFirst(ClaimTypes.Role)?.Value;
                        if (int.TryParse(role, out int roleId))
                        {
                            var roleName = ((StatusEnum)roleId).ToString(); // e.g. "Admin"

                            // replace numeric claim with string
                            identity.RemoveClaim(identity.FindFirst(ClaimTypes.Role));
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                        }
                        context.HttpContext.Items["UserSID"] = userSid;
                        context.HttpContext.Items["Role"] = role;
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        return Task.CompletedTask;
                    },
                };
            });
    
        builder.Services.AddAuthorization();

        UnitOfWorkServiceCollectionExtentions.AddUnitOfWork<DriverLocationTrackingDbContext>(builder.Services);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseHttpsRedirection();

        // ✅ Enable CORS (use AllowAll for dev)
        app.UseCors("AllowAll");
        // Or use specific allowed origins in production:
        // app.UseCors("AllowFrontend");
        
        app.UseAuthentication(); 
        app.UseAuthorization();
        
        app.MapControllers();
        app.Run();
    }
}
