using NICE.EAT.Interfaces;
using NICE.EAT.Models;
using NICE.EAT.Utilities;

namespace NICE.EAT.Services.Filters;

/// <summary>
/// IFilterStrategy implementation #2 — filters restaurants by mood tags.
/// </summary>
public class MoodFilter : IFilterStrategy
{
    private readonly List<string> _moodTags;

    public string FilterName => "Mood Filter";

    public MoodFilter(string mood)
    {
        _moodTags = MoodMapper.ResolveTags(mood);
    }

    /// <summary>
    /// Keeps only restaurants whose mood_tags contain at least one matching tag.
    /// If mood is unrecognized (no tags), passes all restaurants through.
    /// </summary>
    public List<Restaurant> Filter(List<Restaurant> restaurants)
    {
        if (_moodTags.Count == 0) return restaurants;

        return restaurants
            .Where(r => r.GetMoodTagList().Any(t => _moodTags.Contains(t)))
            .ToList();
    }
}
