using NICE.EAT.Data;
using NICE.EAT.Interfaces;
using NICE.EAT.Models;
using NICE.EAT.Utilities;

namespace NICE.EAT.Services;

/// <summary>
/// Core recommendation logic.
/// Extends BaseService. Uses IFilterStrategy polymorphically (POLYMORPHISM DEMO).
/// </summary>
public class RecommendationService : BaseService
{
    private readonly RestaurantRepository _restaurantRepo;
    private readonly MenuItemRepository   _menuRepo;
    private readonly WaitTimeService      _waitService;

    // ── COMPOSITION: service HAS-MANY IFilterStrategy ───────────────────────
    private readonly List<IFilterStrategy> _filters = new();

    public RecommendationService(
        RestaurantRepository restaurantRepo,
        MenuItemRepository menuRepo,
        WaitTimeService waitService)
        : base("RecommendationService")
    {
        _restaurantRepo = restaurantRepo;
        _menuRepo       = menuRepo;
        _waitService    = waitService;
    }

    /// <summary>Adds a filter strategy to the pipeline.</summary>
    public void AddFilter(IFilterStrategy filter)
    {
        _filters.Add(filter);
        Log($"Added filter: {filter.FilterName}");
    }

    /// <summary>Clears all filter strategies for a fresh search.</summary>
    public void ClearFilters() => _filters.Clear();

    /// <summary>
    /// Fetches all restaurants, applies each IFilterStrategy polymorphically,
    /// loads menus (composition), injects wait times, and returns ranked results.
    ///
    /// === POLYMORPHISM DEMO ===
    /// Each filter in _filters is a different IFilterStrategy implementation,
    /// but we call filter.Filter() uniformly through the interface.
    /// </summary>
    public List<Restaurant> GetRecommendations()
    {
        // 1. Load all restaurants from DB
        var restaurants = _restaurantRepo.GetAll();

        // 2. Apply each filter strategy POLYMORPHICALLY
        foreach (IFilterStrategy filter in _filters)
        {
            int before = restaurants.Count;
            restaurants = filter.Filter(restaurants);  // ← POLYMORPHISM
            Log($"{filter.FilterName}: {before} → {restaurants.Count} restaurants");
        }

        // 3. Load menu items into each restaurant (COMPOSITION: has-many)
        foreach (var r in restaurants)
        {
            r.Menu = _menuRepo.GetByRestaurant(r.Id);
            r.WaitTimeMinutes = _waitService.Simulate(r.Id);
        }

        // 4. Sort by rating descending
        return restaurants.OrderByDescending(r => r.Rating).ToList();
    }

    /// <summary>
    /// Demonstrates IRecommendable polymorphism — scores both Restaurant and FoodItem
    /// using the same interface method.
    /// </summary>
    public void PrintRecommendationScores(List<Restaurant> restaurants, decimal budget, int spicyPref)
    {
        Log("=== Recommendation Scores (IRecommendable polymorphism) ===");
        foreach (var r in restaurants)
        {
            // Restaurant implements IRecommendable
            IRecommendable recommendable = r;
            Log($"  {recommendable.GetDisplayInfo()} → Score: {recommendable.GetRecommendationScore(budget, spicyPref):F0}");

            // FoodItems also implement IRecommendable
            foreach (var item in r.Menu.OfType<FoodItem>())
            {
                IRecommendable foodRec = item;
                Log($"    {foodRec.GetDisplayInfo()} → Score: {foodRec.GetRecommendationScore(budget, spicyPref):F0}");
            }
        }
    }
}
