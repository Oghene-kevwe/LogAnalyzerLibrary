using LogAnalyzerLibrary.Application;
using LogAnalyzerLibrary.Application.ArchiveService;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<ILogsService, LogsService>();
builder.Services.AddScoped<IArchiveService, ArchiveService>();

var app = builder.Build();

//var directories = new[] { "C:\\Amadeologs", "C:\\AWIErrors", "C:\\Loggings", };

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
