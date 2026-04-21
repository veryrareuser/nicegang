namespace NICE.EAT.Services;

/// <summary>
/// Abstract base class for all services.
/// PARENT CLASS #5 — parent of RecommendationService, OrderService, PopularityService, WaitTimeService.
/// </summary>
public abstract class BaseService
{
    /// <summary>Name of the service for logging and display.</summary>
    public string ServiceName { get; }

    protected BaseService(string serviceName)
    {
        ServiceName = serviceName;
    }

    /// <summary>
    /// Validates whether the service is ready to operate.
    /// Override in subclasses to add specific validation.
    /// </summary>
    public virtual bool IsReady()
    {
        return true;
    }

    /// <summary>Simple log helper.</summary>
    protected void Log(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  [{ServiceName}] {message}");
        Console.ResetColor();
    }
}
