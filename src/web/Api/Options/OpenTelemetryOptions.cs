namespace api.Options;

public class OpenTelemetryOptions
{
    public const string Section = "OpenTelemetry";

    public string ServiceName { get; set; } = string.Empty;
    public string ExporterEndpoint { get; set; } = string.Empty;
}
