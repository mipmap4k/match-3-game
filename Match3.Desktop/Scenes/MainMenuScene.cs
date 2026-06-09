using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Match3.Desktop.Scenes;

public class MainMenuScene : Scene {
    private Texture2D _pixel = null!;
    private SpriteFont _font = null!;
    private Rectangle _playButton;
    private MouseState _previousMouseState;

    private const int ButtonWidth = 240;
    private const int ButtonHeight = 90;
    private const string ButtonText = "Start";

    public MainMenuScene(Game1 game) : base(game) { }

    public override void LoadContent() {
        _pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _font = Game.Content.Load<SpriteFont>("File");

        int viewportW = Game.GraphicsDevice.Viewport.Width;
        int viewportH = Game.GraphicsDevice.Viewport.Height;
        _playButton = new Rectangle(
            (viewportW - ButtonWidth) / 2,
            (viewportH - ButtonHeight) / 2,
            ButtonWidth,
            ButtonHeight
        );
        _previousMouseState = Mouse.GetState();
    }

    public override void Update(GameTime gameTime) {
        MouseState currentMouseState = Mouse.GetState();
        bool clicked = currentMouseState.LeftButton == ButtonState.Pressed
                    && _previousMouseState.LeftButton == ButtonState.Released;

        if (clicked && _playButton.Contains(currentMouseState.X, currentMouseState.Y)) {
            Game.SetScene(new GameScene(Game));
        }

        _previousMouseState = currentMouseState;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        MouseState currentMouseState = Mouse.GetState();
        bool hover = _playButton.Contains(currentMouseState.X, currentMouseState.Y);
        Color buttonColor = hover ? Color.LightGreen : Color.ForestGreen;

        Rectangle border = new Rectangle(
            _playButton.X - 4, _playButton.Y - 4,
            _playButton.Width + 8, _playButton.Height + 8
        );
        spriteBatch.Draw(_pixel, border, Color.White);
        spriteBatch.Draw(_pixel, _playButton, buttonColor);

        Vector2 textSize = _font.MeasureString(ButtonText);
        Vector2 textPos = new Vector2(
            _playButton.X + (_playButton.Width - textSize.X) / 2,
            _playButton.Y + (_playButton.Height - textSize.Y) / 2
        );
        spriteBatch.DrawString(_font, ButtonText, textPos, Color.White);
    }
}
