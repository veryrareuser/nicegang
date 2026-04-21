using NICE.EAT.Interfaces;
using NICE.EAT.Models;

namespace NICE.EAT.Services.Filters;

/// <summary>
/// IFilterStrategy implementation #1 — filters restaurants by maximum budget.
/// </summary>
public class BudgetFilter : IFilterStrategy
{
    private readonly decimal _maxBudget;

    public string FilterName => "Budget Filter";

    public BudgetFilter(decimal maxBudget)
    {
        _maxBudget = maxBudget;
    }

    /// <summary>Keeps only restaurants with average price ≤ budget.</summary>
    public List<Restaurant> Filter(List<Restaurant> restaurants)
    {
        return restaurants.Where(r => r.AveragePrice <= _maxBudget).ToList();
    }
}
