using NICE.EAT.Models;

namespace NICE.EAT.Interfaces;

/// <summary>
/// INTERFACE #2 — Strategy pattern for filtering restaurants.
/// Implementations: BudgetFilter, MoodFilter, SpicyFilter, VeganFilter.
/// Used polymorphically in RecommendationService.
/// </summary>
public interface IFilterStrategy
{
    /// <summary>User-friendly name of this filter.</summary>
    string FilterName { get; }

    /// <summary>
    /// Applies this filter to the input list and returns the filtered subset.
    /// </summary>
    List<Restaurant> Filter(List<Restaurant> restaurants);
}
