using System.Net.Mail;
using Hones.Remit.Api.Apis;
using Hones.Remit.Api.BackgroundServices;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrdersDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHostedService<DatabaseMigrationsService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(new SmtpClient("localhost", 2525));
builder.Services.AddSingleton<IEmailService, LocalSmtpEmailService>();


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
