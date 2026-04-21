using NICE.EAT.Data;
using NICE.EAT.Models;

namespace NICE.EAT.Services;

/// <summary>
/// Reports on menu popularity using aggregate SQL queries (GROUP BY).
/// Extends BaseService (Parent #5).
/// </summary>
public class PopularityService : BaseService
{
    private readonly MenuItemRepository _menuRepo;

    public PopularityService(MenuItemRepository menuRepo)
        : base("PopularityService")
    {
        _menuRepo = menuRepo;
    }

    /// <summary>Returns the top N most-ordered menu items across all restaurants.</summary>
    public List<MenuItem> GetTopMenuItems(int topN = 5)
    {
        return _menuRepo.GetMostPopular(topN);
    }
}
