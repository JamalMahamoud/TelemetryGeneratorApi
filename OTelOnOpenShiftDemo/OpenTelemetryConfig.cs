using System.Diagnostics;

namespace OptelDataGenerator;

public static class OpenTelemetryConfig
{
    public const string ServiceName = "Microlise.OTelOnOpenShiftDemo.Service";
    //will be use to link OTel with .Net counter
    public const string MeterName = "OTelOnOpenShiftDemo.Meter";
    
    public static ActivitySource ActivitySource = new(ServiceName);
}