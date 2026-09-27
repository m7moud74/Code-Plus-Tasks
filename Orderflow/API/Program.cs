using System.Text.Json;
using APP;
using APP.Common.Diagnostics;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService(DiagnosticsConfig.ServiceName))

    .WithTracing(tracing =>
    {
        _ = tracing
            .AddSource(DiagnosticsConfig.ServiceName)
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddRedisInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4317");
            });
    })

    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter(DiagnosticsConfig.ServiceName)
            .AddAspNetCoreInstrumentation()
            .AddPrometheusExporter();
    });



builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("cs")!,
        name: "sql_server")
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis")!,
        name: "redis_cache");


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();


app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            Status = report.Status.ToString(),

            Components = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration.ToString()
            }),

            TotalDuration = report.TotalDuration.ToString()
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHangfireDashboard("/hangfire");
app.UseSerilogRequestLogging();
app.MapPrometheusScrapingEndpoint();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
