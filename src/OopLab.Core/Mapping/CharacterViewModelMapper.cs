using OopLab.Core.Models;
using OopLab.Core.ViewModels;

namespace OopLab.Core.Mapping;

public sealed class CharacterViewModelMapper
{
    public CharacterViewModel ToViewModel(CharacterProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        int popularityScore = CalculatePopularityScore(profile);
        string tier = popularityScore switch
        {
            >= 900 => "S+",
            >= 750 => "S",
            >= 550 => "A",
            _ => "B"
        };

        string speciesFull = string.IsNullOrWhiteSpace(profile.SubType)
            ? profile.Species
            : $"{profile.Species} ({profile.SubType})";

        bool hasValidCache = !string.IsNullOrWhiteSpace(profile.LocalAvatarPath)
                             && File.Exists(profile.LocalAvatarPath);

        return new CharacterViewModel
        {
            Id = profile.Id,
            DisplayName = profile.Name,
            Subtitle = $"{speciesFull} • {profile.Gender} • Епізодів: {profile.EpisodeCount}",
            StatusBadge = profile.Status.ToUpperInvariant(),
            StatusColorHex = ResolveStatusHex(profile.Status),
            SpeciesLabel = speciesFull,
            GenderLabel = profile.Gender,
            OriginAndLocation = $"Походження: {profile.OriginWorld} → Локація: {profile.CurrentLocation}",
            RarityStars = new string('★', Math.Clamp(profile.Rarity, 1, 5)),
            RarityValue = profile.Rarity,
            EpisodeAppearances = profile.EpisodeCount,
            PopularityScore = popularityScore,
            TierRank = tier,
            StatsSummary = $"HP: {profile.BaseHp} | ATK: {profile.BaseAtk} | Рейтинг: {popularityScore} ({tier})",
            Biography = string.IsNullOrWhiteSpace(profile.Description)
                ? $"{profile.Name} — {speciesFull.ToLowerInvariant()}, походить із {profile.OriginWorld}."
                : profile.Description,
            AvatarImageUrl = profile.AvatarUrl,
            LocalCachedImagePath = hasValidCache ? profile.LocalAvatarPath : null,
            IsPortraitCached = hasValidCache,
            EffectiveImageSource = hasValidCache ? profile.LocalAvatarPath! : profile.AvatarUrl,
            SourceBadge = profile.IsCustom ? "Користувацький" : "Rick & Morty API"
        };
    }

    public IReadOnlyList<CharacterViewModel> ToViewModels(IEnumerable<CharacterProfile> profiles)
        => profiles.Select(ToViewModel).ToList();

    private static int CalculatePopularityScore(CharacterProfile p)
    {
        double statusBonus = p.Status.Equals("Alive", StringComparison.OrdinalIgnoreCase) ? 1.1 : 1.0;
        double raw = (p.EpisodeCount * 14.0 + p.BaseHp * 0.02 + p.BaseAtk * 0.6) * statusBonus;
        return (int)Math.Round(raw);
    }

    private static string ResolveStatusHex(string status) => status.Trim().ToLowerInvariant() switch
    {
        "alive" => "#55CC44",
        "dead" => "#D63D2E",
        _ => "#9E9E9E"
    };
}
