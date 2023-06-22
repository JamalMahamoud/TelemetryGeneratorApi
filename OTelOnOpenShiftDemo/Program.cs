using System.Diagnostics.Metrics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OptelDataGenerator;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//this will be attach to all the logs that goes out  

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// add tracing
 builder.Services.AddOpenTelemetry()
     .WithTracing(tracerProviderBuilder =>
         tracerProviderBuilder
             .AddOtlpExporter()
             .AddSource(OpenTelemetryConfig.ActivitySource.Name)
             .ConfigureResource(resource => resource
                 .AddService(OpenTelemetryConfig.ServiceName))
             .AddAspNetCoreInstrumentation()
             .AddJaegerExporter()
             .AddConsoleExporter()
         )
     
     //add metrics
     .WithMetrics(metricProviderBuilder => 
         metricProviderBuilder
             .ConfigureResource(resource => resource
                 .AddService(OpenTelemetryConfig.ServiceName))
             .AddConsoleExporter()
             .AddOtlpExporter()
             //Meter name come from OpenTelemetryConfig.cs
             .AddMeter(OpenTelemetryConfig.MeterName)
     );

builder.Logging.ClearProviders();

builder.Logging.AddOpenTelemetry(options =>
{
    options.AddConsoleExporter();
    

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
