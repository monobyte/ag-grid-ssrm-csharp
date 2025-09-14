using BondTradingApi.Hubs;
using BondTradingApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSingleton<IBondService, BondService>();
builder.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
builder.Services.AddHostedService<TickSimulationService>();

var app = builder.Build();

app.UseCors();
app.UseRouting();

app.MapHub<BondHub>("/bondhub");

app.MapGet("/", () => "Bond Trading API is running");
app.MapGet("/test", () => new { status = "ok", timestamp = DateTime.UtcNow });

app.Run();