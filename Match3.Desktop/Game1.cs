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
    private MouseState _previousMouseState;
    private bool _hasSelection = false;
    private int _selectedRow;
    private int _selectedCol;
    private const int CellSize = 64;
    private (int row, int col) PixelToCell(int mouseX, int mouseY) {
        var (offsetX, offsetY) = GetBoardOffset();
        var reliableX = mouseX - offsetX;
        var reliableY = mouseY - offsetY;
        if (reliableX < 0 || reliableY < 0) return (-1, -1);
        int col = reliableX / CellSize;
        int row = reliableY / CellSize;
        if (row >= Board.Rows || col >= Board.Cols) return (-1, -1);
        return (row, col);
    }
    private (int offsetX, int offsetY) GetBoardOffset() {
    int boardWidth = Board.Cols * CellSize;
    int boardHeight = Board.Rows * CellSize;
    int offsetX = (GraphicsDevice.Viewport.Width - boardWidth) / 2;
    int offsetY = (GraphicsDevice.Viewport.Height - boardHeight) / 2;
    return (offsetX, offsetY);
    }

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

        MouseState currentMouseState = Mouse.GetState();
        bool clicked = currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
         if (clicked) {
            var (row, col) = PixelToCell(currentMouseState.X, currentMouseState.Y);
            if (!_hasSelection) {
                _selectedCol = col;
                _selectedRow = row;
                _hasSelection = true;
            } else {
                bool tryMove = _board.TryMakeMove(_selectedRow, _selectedCol, row, col);
                if (tryMove) {
                    _board.CycleTick();
                } else {
                    Window.Title = "Invalid ZOMBE";
                }
                _hasSelection = false;
            }
        }
        _previousMouseState = currentMouseState;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var (offsetX, offsetY) = GetBoardOffset();
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        for (int col=0; col < Board.Cols; col++) {
            for (int row=0; row < Board.Rows; row++) {
                _spriteBatch.Draw(_pixel,new Rectangle( 
                    offsetX + col * CellSize, 
                    offsetY + row * CellSize, 
                    CellSize - 4, 
                    CellSize - 4), 
                    GemToColor(_board.GetCell(row, col)));
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
