using OopLab.Core.Models;

namespace OopLab.Core.Specifications;

public sealed class CharacterFilterCriteria
{
    public string? Status { get; set; }
    public string? Species { get; set; }
    public string? Gender { get; set; }
    public int? MinRarity { get; set; }
    public bool? OnlyCachedPortraits { get; set; }
    public string? SearchText { get; set; }
    public string? SortBy { get; set; }

    public bool IsSatisfiedBy(CharacterProfile c)
    {
        if (!string.IsNullOrWhiteSpace(Status) &&
            !c.Status.Equals(Status.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(Species) &&
            !c.Species.Contains(Species.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(Gender) &&
            !c.Gender.Equals(Gender.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        if (MinRarity.HasValue && c.Rarity < MinRarity.Value)
            return false;

        if (OnlyCachedPortraits == true &&
            (string.IsNullOrWhiteSpace(c.LocalAvatarPath) || !File.Exists(c.LocalAvatarPath)))
            return false;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var q = SearchText.Trim();
            bool matches = c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                           || c.Id.Equals(q, StringComparison.OrdinalIgnoreCase)
                           || c.OriginWorld.Contains(q, StringComparison.OrdinalIgnoreCase)
                           || c.Description.Contains(q, StringComparison.OrdinalIgnoreCase);
            if (!matches) return false;
        }

        return true;
    }
}
