using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OptelDataGenerator;

public static class OpenTelemetryConfig
{
    public const string ServiceName = "Microlise.OTelOnOpenShiftDemo.Service";

    //will be use to link OTel with .Net counter
    public static readonly Meter Meter = new(ServiceName);

    public static ActivitySource ActivitySource = new(ServiceName);
    

    public const string AddedProductId = "AddedProductID";
    public const string AddedProductName = "AddedProductName";
}