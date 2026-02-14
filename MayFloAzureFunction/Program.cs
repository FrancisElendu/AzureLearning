using MayFloAzureFunction.Data;
using MayFloAzureFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

string connectionString = Environment.GetEnvironmentVariable("LocalSqlDataBase");
//string connectionString = Environment.GetEnvironmentVariable("AzureSqlDataBase");

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddDbContext<ApplicationDbContext> (context => context.UseSqlServer(connectionString));

builder.Build().Run();
