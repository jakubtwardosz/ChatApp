using ChatApp.API.Extensions;
using ChatApp.Core.Domain;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddConfiguration(configuration);
builder.Services.AddServices();

builder.Services.AddOptions(configuration);

var origin = configuration.GetValue<string>("Orgin") ?? throw new NullReferenceException("Origin is not set in configuration");
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        {
            builder.WithOrigins(origin)

                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed((host) => true)
                .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseAuthentication();

app.MapControllers();

app.Run();
