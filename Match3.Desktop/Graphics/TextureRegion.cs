using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Match3.Desktop.Graphics;

/// <summary>
/// Описывает прямоугольную область внутри текстуры (атласа).
/// Позволяет работать с одним спрайтом из большого PNG, как с независимой картинкой.
/// </summary>
public class TextureRegion {
    public Texture2D Texture { get; set; } = null!;
    public Rectangle SourceRectangle { get; set; }

    public int Width => SourceRectangle.Width;
    public int Height => SourceRectangle.Height;

    public TextureRegion() { }

    public TextureRegion(Texture2D texture, int x, int y, int width, int height) {
        Texture = texture;
        SourceRectangle = new Rectangle(x, y, width, height);
    }

    /// <summary>
    /// Рисует регион в указанной позиции с заданным цветом.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color) {
        spriteBatch.Draw(Texture, position, SourceRectangle, color);
    }

    /// <summary>
    /// Рисует регион в произвольном прямоугольнике (с автоматическим масштабированием).
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle, Color color) {
        spriteBatch.Draw(Texture, destinationRectangle, SourceRectangle, color);
    }
}
