using Hones.Remit.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.BackgroundServices;

public class DatabaseMigrationsService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationsService> _logger;

    public DatabaseMigrationsService(IServiceProvider serviceProvider, ILogger<DatabaseMigrationsService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database migrations started");
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await dbContext.Database.MigrateAsync(stoppingToken);
        _logger.LogInformation("Database migrations completed");
    }
}