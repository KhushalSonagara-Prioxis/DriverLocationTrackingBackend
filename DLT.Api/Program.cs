using DLT.Api.Helper;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Implementation;
using DLT.Service.Repository.Interface;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
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
                    "http://192.168.1.119:3000"
                )
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
        });

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

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
