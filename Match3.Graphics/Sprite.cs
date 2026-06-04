using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Match3.Graphics;

/// <summary>
/// Готовый к отрисовке спрайт с набором визуальных свойств:
/// цвет-тоник, поворот, масштаб, точка опоры, эффекты, слой.
/// Объединяет регион текстуры и параметры её рендеринга.
/// </summary>
public class Sprite {
    public TextureRegion Region { get; set; } = null!;
    public Color Color { get; set; } = Color.White;
    public float Rotation { get; set; } = 0.0f;
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;
    public float LayerDepth { get; set; } = 0.0f;

    public float Width => Region.Width * Scale.X;
    public float Height => Region.Height * Scale.Y;

    public Sprite() { }

    public Sprite(TextureRegion region) {
        Region = region;
    }

    /// <summary>
    /// Устанавливает точку опоры в центр текстурного региона.
    /// Полезно, когда нужно вращать/масштабировать относительно центра, а не угла.
    /// </summary>
    public void CenterOrigin() {
        Origin = new Vector2(Region.Width, Region.Height) * 0.5f;
    }

    /// <summary>
    /// Рисует спрайт в указанной позиции с учётом всех визуальных свойств.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Vector2 position) {
        spriteBatch.Draw(
            Region.Texture,
            position,
            Region.SourceRectangle,
            Color,
            Rotation,
            Origin,
            Scale,
            Effects,
            LayerDepth
        );
    }
}
