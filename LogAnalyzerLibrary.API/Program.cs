using LogAnalyzerLibrary.Application;
using LogAnalyzerLibrary.Application.ArchiveService;
using LogAnalyzerLibrary.Integration.CloudinaryIntegration;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<ILogsService, LogsService>();
builder.Services.AddScoped<IArchiveService, ArchiveService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

builder.Host.UseSerilog((context, configuration) =>

    configuration
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.File("Logs/LA-API- .log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
    .WriteTo.Console(outputTemplate: "[{Timestamp:dd-MM HH:mm:ss} {Level:u3}] |{SourceContext}| {NewLine}{Message:lj}{NewLine}{Exception}")
);

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
