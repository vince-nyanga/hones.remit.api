using System.Net.Mail;
using System.Reflection;
using Hones.Remit.Api.Apis;
using Hones.Remit.Api.BackgroundServices;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrdersDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMassTransit(configurator =>
{
    configurator.SetKebabCaseEndpointNameFormatter();
    
    var entryAssembly = Assembly.GetEntryAssembly();
    configurator.AddConsumers(entryAssembly);
    
    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", 9520, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ConfigureEndpoints(context);
    });

});

builder.Services.AddHostedService<DatabaseMigrationsService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<SmtpClient>(_ => new SmtpClient("localhost", 2525));
builder.Services.AddScoped<IEmailService, LocalSmtpEmailService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGroup("/api")
    .MapOrders();

app.Run();
