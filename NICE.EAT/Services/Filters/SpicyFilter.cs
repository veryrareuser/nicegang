using NICE.EAT.Interfaces;
using NICE.EAT.Models;

namespace NICE.EAT.Services.Filters;

/// <summary>
/// IFilterStrategy implementation #3 — filters restaurants by max spicy level.
/// </summary>
public class SpicyFilter : IFilterStrategy
{
    private readonly int _maxSpicyLevel;

    public string FilterName => "Spicy Filter";

    public SpicyFilter(int maxSpicyLevel)
    {
        _maxSpicyLevel = maxSpicyLevel;
    }

    /// <summary>Keeps only restaurants with spicy level ≤ max preference.</summary>
    public List<Restaurant> Filter(List<Restaurant> restaurants)
    {
        return restaurants.Where(r => r.SpicyLevel <= _maxSpicyLevel).ToList();
    }
}
