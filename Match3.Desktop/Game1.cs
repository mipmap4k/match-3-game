using Match3.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Match3.Desktop;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Board _board;
    private Texture2D _pixel;
    private const int CellSize = 64;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _board = new Board();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] {Color.White});


        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        for (int col=0; col < Board.Cols; col++) {
            for (int row=0; row < Board.Rows; row++) {
                _spriteBatch.Draw(_pixel,new Rectangle(col * CellSize, row * CellSize, CellSize - 4, CellSize - 4), GemToColor(_board.GetCell(row, col)));
            }
        }
        _spriteBatch.End();
        base.Draw(gameTime);
    }
    private static Color GemToColor(GemType gem) => gem switch
{
    GemType.Red    => Color.Red,
    GemType.Green  => Color.Green,
    GemType.Blue   => Color.Blue,
    GemType.Yellow => Color.Yellow,
    GemType.Empty  => Color.DimGray,
    _              => Color.Magenta
};
}
