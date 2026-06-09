using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Match3.Desktop.Scenes;

public class GameOverScene : Scene {
    private Texture2D _pixel = null!;
    private SpriteFont _font = null!;
    private Rectangle _okButton;
    private MouseState _previousMouseState;
    private int _finalScore;

    private const int ButtonWidth = 180;
    private const int ButtonHeight = 70;
    private const string ButtonText = "OK";

    public GameOverScene(Game1 game, int finalScore) : base(game) {
        _finalScore = finalScore;
    }

    public override void LoadContent() {
        _pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _font = Game.Content.Load<SpriteFont>("File");

        int viewportW = Game.GraphicsDevice.Viewport.Width;
        int viewportH = Game.GraphicsDevice.Viewport.Height;
        _okButton = new Rectangle(
            (viewportW - ButtonWidth) / 2,
            (viewportH / 2) + 60,
            ButtonWidth,
            ButtonHeight
        );

        _previousMouseState = Mouse.GetState();
    }

    public override void Update(GameTime gameTime) {
        MouseState currentMouseState = Mouse.GetState();
        bool clicked = currentMouseState.LeftButton == ButtonState.Pressed
                    && _previousMouseState.LeftButton == ButtonState.Released;

        if (clicked && _okButton.Contains(currentMouseState.X, currentMouseState.Y)) {
            Game.SetScene(new MainMenuScene(Game));
        }

        _previousMouseState = currentMouseState;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        int viewportW = Game.GraphicsDevice.Viewport.Width;
        int viewportH = Game.GraphicsDevice.Viewport.Height;

        string gameOverText = "Game Over";
        Vector2 gameOverSize = _font.MeasureString(gameOverText);
        Vector2 gameOverPos = new Vector2(
            (viewportW - gameOverSize.X) / 2,
            (viewportH / 2) - 100
        );
        spriteBatch.DrawString(_font, gameOverText, gameOverPos, Color.Yellow);

        string scoreText = $"Final Score: {_finalScore}";
        Vector2 scoreSize = _font.MeasureString(scoreText);
        Vector2 scorePos = new Vector2(
            (viewportW - scoreSize.X) / 2,
            (viewportH / 2) - 20
        );
        spriteBatch.DrawString(_font, scoreText, scorePos, Color.White);

        MouseState currentMouseState = Mouse.GetState();
        bool hover = _okButton.Contains(currentMouseState.X, currentMouseState.Y);
        Color buttonColor = hover ? Color.LightGreen : Color.ForestGreen;

        Rectangle border = new Rectangle(
            _okButton.X - 4, _okButton.Y - 4,
            _okButton.Width + 8, _okButton.Height + 8
        );
        spriteBatch.Draw(_pixel, border, Color.White);
        spriteBatch.Draw(_pixel, _okButton, buttonColor);

        Vector2 okTextSize = _font.MeasureString(ButtonText);
        Vector2 okTextPos = new Vector2(
            _okButton.X + (_okButton.Width - okTextSize.X) / 2,
            _okButton.Y + (_okButton.Height - okTextSize.Y) / 2
        );
        spriteBatch.DrawString(_font, ButtonText, okTextPos, Color.White);
    }
}
