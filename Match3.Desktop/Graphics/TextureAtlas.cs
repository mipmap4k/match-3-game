using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Match3.Desktop.Graphics;

/// <summary>
/// Хранит набор именованных регионов одной большой текстуры (спрайт-листа).
/// Позволяет получать спрайты по строковому имени, например "gem_red".
/// </summary>
public class TextureAtlas {
    private readonly Dictionary<string, TextureRegion> _regions = new();

    public Texture2D Texture { get; set; } = null!;

    public TextureAtlas() { }

    public TextureAtlas(Texture2D texture) {
        Texture = texture;
    }

    /// <summary>
    /// Регистрирует прямоугольную область с указанным именем.
    /// </summary>
    public void AddRegion(string name, int x, int y, int width, int height) {
        var region = new TextureRegion(Texture, x, y, width, height);
        _regions.Add(name, region);
    }

    /// <summary>
    /// Возвращает регион по имени. Бросит исключение, если имя не зарегистрировано.
    /// </summary>
    public TextureRegion GetRegion(string name) => _regions[name];

    /// <summary>
    /// Создаёт новый Sprite, привязанный к региону с указанным именем.
    /// </summary>
    public Sprite CreateSprite(string regionName) {
        var region = GetRegion(regionName);
        return new Sprite(region);
    }
}
