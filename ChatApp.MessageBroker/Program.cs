using ChatApp.Core.Domain;
using ChatApp.Core.Domain.Options;
using ChatApp.MessageBroker.Kafka;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.Configure<KafkaOption>(options => configuration.GetSection(nameof(KafkaOption)).Bind(options));

var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHostedService<KafkaConsumer>();

var app = builder.Build();

app.Run();
