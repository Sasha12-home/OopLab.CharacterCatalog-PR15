using OopLab.Core.Rendering;

namespace OopLab.Core.Models;

public class CharacterProfile : IRenderable<CharacterProfile>
{
    private IRenderer<CharacterProfile>? _defaultRenderer;

    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..6];
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Alive";
    public string Species { get; set; } = "Human";
    public string SubType { get; set; } = string.Empty;
    public string Gender { get; set; } = "Unknown";
    public string OriginWorld { get; set; } = "Earth (C-137)";
    public string CurrentLocation { get; set; } = "Unknown";
    public int EpisodeCount { get; set; } = 1;
    public int Rarity { get; set; } = 3;
    public int BaseHp { get; set; } = 10000;
    public int BaseAtk { get; set; } = 250;
    public string Description { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string? LocalAvatarPath { get; set; }
    public bool IsCustom { get; set; }

    public void UseRenderer(IRenderer<CharacterProfile> renderer)
    {
        _defaultRenderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public void Render(IRenderer<CharacterProfile> renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.Render(this);
    }

    public void Render()
    {
        if (_defaultRenderer is null)
            throw new InvalidOperationException("Рендерер за замовчуванням не налаштований.");
        _defaultRenderer.Render(this);
    }
}
