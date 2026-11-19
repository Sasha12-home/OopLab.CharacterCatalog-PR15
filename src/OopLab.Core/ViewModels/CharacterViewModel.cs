namespace OopLab.Core.ViewModels;

public sealed class CharacterViewModel
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string StatusBadge { get; init; } = string.Empty;
    public string StatusColorHex { get; init; } = "#55CC44";
    public string SpeciesLabel { get; init; } = string.Empty;
    public string GenderLabel { get; init; } = string.Empty;
    public string OriginAndLocation { get; init; } = string.Empty;
    public string RarityStars { get; init; } = string.Empty;
    public int RarityValue { get; init; }
    public int EpisodeAppearances { get; init; }
    public int PopularityScore { get; init; }
    public string TierRank { get; init; } = "A";
    public string StatsSummary { get; init; } = string.Empty;
    public string Biography { get; init; } = string.Empty;
    public string AvatarImageUrl { get; init; } = string.Empty;
    public string? LocalCachedImagePath { get; init; }
    public bool IsPortraitCached { get; init; }
    public string EffectiveImageSource { get; init; } = string.Empty;
    public string SourceBadge { get; init; } = string.Empty;
}
