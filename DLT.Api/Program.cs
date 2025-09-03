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
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Add services to the container.
        builder.Services.AddScoped<IDriverRepository, DriverRepository>();
        
        //connection String
        // Get connection string from appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("DBConnection");
                //serilogger
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();
        //Fluent Validation
        builder.Services.AddControllers()
        
            .AddFluentValidation(fv =>
        
            {
        
                fv.RegisterValidatorsFromAssembly(AppDomain.CurrentDomain.GetAssemblies()
        
                    .SingleOrDefault(assembly => assembly.GetName().Name == typeof(Program).Assembly.GetName().Name));
        
            });
        builder.Services.AddDbContext<DriverLocationTrackingDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {
        
            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);

        builder.Services.AddDbContext<DriverLocationTrackingSpContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {

                sqlOptions.EnableRetryOnFailure();

            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);
        // Register the UserService
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            // OR restrict to specific origin (better for production)
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000") // frontend URL
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        UnitOfWorkServiceCollectionExtentions.AddUnitOfWork<DriverLocationTrackingDbContext>(builder.Services);
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}