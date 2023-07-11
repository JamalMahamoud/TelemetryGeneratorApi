using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OptelDataGenerator;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.OpenTelemetry(opt =>
    {
        opt.Endpoint = "http://localhost:4317";
    })
     .WriteTo.Console()
     .Enrich.FromLogContext()
     .CreateLogger();

// Add services to the container.
var resource = ResourceBuilder.CreateDefault().AddService(OpenTelemetryConfig.ServiceName);
//this will be attach to all the logs that goes out  

builder.Services.AddControllers();
builder.Logging.ClearProviders();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenTelemetry()
    .WithTracing(b =>
    {
        b
            .AddConsoleExporter()
            .AddSource(OpenTelemetryConfig.ServiceName)
            .SetResourceBuilder(resource)
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint =
                    new Uri(
                        "http://<collector-address>:<port>"); // Specify the address and port of your OpenTelemetry Collector.
            });
    })
    .WithMetrics(metricsProviderBuilder =>
            metricsProviderBuilder
                .AddMeter(OpenTelemetryConfig.Meter.Name)
                .AddConsoleExporter()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint =
                        new Uri(
                            "http://<collector-address>:<port>"); // Specify the address and port of your OpenTelemetry Collector.
                })
    );
    builder.Logging.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resource)
            .AddConsoleExporter()
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint =
                    new Uri(
                        "http://<collector-address>:<port>"); // Specify the address and port of your OpenTelemetry Collector.
            });
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();