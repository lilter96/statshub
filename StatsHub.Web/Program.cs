using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StatsHub.Application.Handlers;
using StatsHub.Application.Validators;
using StatsHub.Infrastructure.Data;
using Web.Hubs;
using Web.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<StatsHubDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddMemoryCache();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<BrandRevenueQueryHandler>();
    cfg.RegisterServicesFromAssemblyContaining<OrdersSyncedSignalRHandler>();
});
builder.Services.AddValidatorsFromAssembly(typeof(OrderDtoValidator).Assembly);
builder.Services.AddSignalR();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddCors(opts =>
{
    opts.AddPolicy("AllowReactClient", policy =>
    {
        policy
            .WithOrigins("https://localhost:44476", "https://localhost:7065", "http://localhost:5065")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StatsHub API",
        Version = "v1"
    });
});

builder.Services.AddScoped<ApiExceptionMiddleware>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StatsHubDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseSpaStaticFiles();
    app.UseSpa(spa => { spa.Options.SourcePath = "ClientApp"; });
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "StatsHub API V1");
        options.RoutePrefix = "swagger";
    });

    Console.WriteLine("Swagger UI available at https://localhost:7065/swagger/index.html");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowReactClient");

app.MapHub<RevenueHub>("/hubs/revenue");

app.UseMiddleware<ApiExceptionMiddleware>();

app.MapControllerRoute(
    "default",
    "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
