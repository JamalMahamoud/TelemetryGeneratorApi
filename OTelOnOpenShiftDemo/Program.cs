using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OptelDataGenerator;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
// Add services to the container.
var resource = ResourceBuilder.CreateDefault().AddService(OpenTelemetryConfig.ServiceName);
//this will be attach to all the logs that goes out  

builder.Services.AddControllers();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// add tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource(OpenTelemetryConfig.ActivitySource.Name)
            .SetResourceBuilder(resource)
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(
                opt =>
                {
                    opt.Endpoint = new Uri("http://localhost:4317");
                    opt.Protocol = OtlpExportProtocol.Grpc;
                })
            .AddConsoleExporter()
    )

    //add metrics
    .WithMetrics(metricProviderBuilder =>
        metricProviderBuilder
            .SetResourceBuilder(resource)
            .AddConsoleExporter()
            .AddOtlpExporter()
            .AddOtlpExporter(
                opt =>
                {
                    opt.Endpoint = new Uri("http://localhost:4312");
                    opt.Protocol = OtlpExportProtocol.Grpc;
                })
            //Meter name come from OpenTelemetryConfig.cs
            .AddMeter(OpenTelemetryConfig.MeterName)
    );



// builder.Logging.AddOpenTelemetry(options =>
// {
//     options.AddConsoleExporter();
//     options.SetResourceBuilder(resource);
// });


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