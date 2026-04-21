namespace NICE.EAT.Services;

/// <summary>
/// Simulates restaurant wait times using a random distribution.
/// Extends BaseService (Parent #5).
/// </summary>
public class WaitTimeService : BaseService
{
    private const int MinWait = 5;
    private const int MaxWait = 45;

    public WaitTimeService() : base("WaitTimeService") { }

    /// <summary>Returns a simulated wait time in minutes.</summary>
    public int Simulate(int restaurantId)
    {
        return Random.Shared.Next(MinWait, MaxWait + 1);
    }

    /// <summary>Returns a verbal description of the wait tier.</summary>
    public static string GetTier(int minutes) => minutes switch
    {
        <= 10 => "Fast",
        <= 25 => "Normal",
        _     => "Busy"
    };
}
