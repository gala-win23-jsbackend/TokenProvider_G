using Infrastructure.Data.Contexts;
using Infrastructure.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDbContextFactory<DataContext>(options =>
        {
            options.UseSqlServer(Environment.GetEnvironmentVariable("SQL_database"));
        });

        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<CookieGenerator>();
    })
    .Build();

host.Run();
