using NICE.EAT.Models;

namespace NICE.EAT.Interfaces;

/// <summary>
/// INTERFACE #1 — Implemented by entities that can be recommended to users.
/// Implementations: Restaurant, FoodItem.
/// </summary>
public interface IRecommendable
{
    /// <summary>Returns a formatted display string for the recommendation UI.</summary>
    string GetDisplayInfo();

    /// <summary>
    /// Calculates a recommendation score (0–100) based on user preferences.
    /// Higher = better match.
    /// </summary>
    double GetRecommendationScore(decimal budget, int spicyPreference);
}
