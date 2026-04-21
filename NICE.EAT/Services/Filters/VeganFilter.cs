using NICE.EAT.Interfaces;
using NICE.EAT.Models;

namespace NICE.EAT.Services.Filters;

/// <summary>
/// IFilterStrategy implementation #4 — filters restaurants to vegan-only.
/// </summary>
public class VeganFilter : IFilterStrategy
{
    public string FilterName => "Vegan Filter";

    /// <summary>Keeps only restaurants flagged as vegan.</summary>
    public List<Restaurant> Filter(List<Restaurant> restaurants)
    {
        return restaurants.Where(r => r.IsVegan).ToList();
    }
}
