using Match3.Logic;
using Match3.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace Match3.Desktop;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Board _board;
    private Texture2D _pixel;
    private MouseState _previousMouseState;
    private int _selectedRow;
    private int _selectedCol;
    private float _time = 0f;
    private TextureAtlas _gemAtlas;
    private TextureRegion _backgroundBlur;
    private Animation _explosionAnimation = null!;
    private List<(AnimatedSprite sprite, Vector2 position, float delay)> _activeAnimations = new();
    private List<(int row, int col, Cell cell, PositionTween tween)> _movingCells = new();
    private bool _pendingCycle = false;
    private GameState _state = GameState.Idle;
    private const int SpriteSize = 100;
    private const int CellSize = 64;
    private const int ExplosionFrameSize = 100;
    private const int ExplosionFrameCount = 4;
    private const bool ExplosionFramesHorizontal = true;
    private const float SwapDuration = 0.25f;
  
    private enum GameState {Idle, Selected, Resolving};
    private void GrayscaleRegion(Texture2D texture, int startX, int startY, int width, int height) {
        Color[] data = new Color[texture.Width * texture.Height];
        texture.GetData(data);
        
        for (int y = startY; y < startY + height; y++) {
            for (int x = startX; x < startX + width; x++) {
                int idx = y * texture.Width + x;
                Color c = data[idx];
                byte gray = (byte)(c.R + c.G + c.B);
                data[idx] = new Color(gray, gray, gray, c.A);
            }
        }
        
        texture.SetData(data);
    }
    private void AddGem(string name, int row, int col) {
        _gemAtlas.AddRegion(name, col * SpriteSize, row * SpriteSize, SpriteSize, SpriteSize);
    }
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
    private static Color GemColorToTint(GemColor color) {
        return color switch {
            GemColor.Orange => Color.Orange,
            GemColor.Blue => Color.Blue,
            GemColor.Red => Color.Red,
            GemColor.Green => Color.Green,
            GemColor.Purple => Color.Purple,
            _ => Color.Black
        };
    }
    private static string GetRegionName(GemColor color, BonusType bonus) {
        string colorPart = color  switch {
            GemColor.Orange => "orange",
            GemColor.Blue => "blue",
            GemColor.Red => "red",
            GemColor.Green => "green",
            GemColor.Purple => "purple",
            _ => "orange"
        };
        string bonusPart = bonus switch {
            BonusType.LineH => "LineH",
            BonusType.LineV => "LineV",
            BonusType.Bomb => "Bomb",
            _ => ""
        };
        return colorPart + bonusPart;
    }
    private void SpawnAnimationsFromEvents() {
        foreach (var (row, col, bonus) in _board.LastTickEvents) {
            switch (bonus) {
                case BonusType.Bomb:
                    for (int r = row - 1; r <= row + 1; r++) {
                        for (int c = col - 1; c <= col + 1; c++) {
                            if (r >= 0 && r < Board.Rows && c >= 0 && c < Board.Cols) {
                                AddExplosion(r, c, delay: 0f);
                            }
                        }
                    } break;
                case BonusType.LineH:
                    for (int c = 0; c < Board.Cols; c++) {
                        float delay = Math.Abs(c - col) * 0.07f; 
                        AddExplosion(row, c, delay);
                    } break;
                case BonusType.LineV:
                    for (int r = 0; r < Board.Rows; r++) {
                        float delay = Math.Abs(r - row) * 0.07f;
                        AddExplosion(r, col, delay);
                    } break;
            }
        }
        _board.LastTickEvents.Clear();
    }

    private void StartSwapAnimation(int rowA, int colA, int rowB, int colB, Cell cellA, Cell cellB) {
        var (offsetX, offsetY) = GetBoardOffset();
        Vector2 posA = CellToPixel(rowA, colA, offsetX, offsetY);
        Vector2 posB = CellToPixel(rowB, colB, offsetX, offsetY);

        _movingCells.Add((rowB, colB, cellA, new PositionTween(posA, posB, SwapDuration)));
        _movingCells.Add((rowA, colA, cellB, new PositionTween(posB, posA, SwapDuration)));
    }

    private Vector2 CellToPixel(int row, int col, int offsetX, int offsetY) {
        return new Vector2(offsetX + col * CellSize, offsetY + row * CellSize);
    }

    private void AddExplosion(int row, int col, float delay) {
        var (offsetX, offsetY) = GetBoardOffset();
        var sprite = new AnimatedSprite(_explosionAnimation) { Loop = false };
        sprite.CenterOrigin();
        var position = new Vector2(
            offsetX + col * CellSize + CellSize / 2,
            offsetY + row * CellSize + CellSize / 2
        );
        _activeAnimations.Add((sprite, position, delay));
    }


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.PreferredBackBufferWidth = 960;
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
        Texture2D gemSheet = Content.Load<Texture2D>("assets_candy");
        GrayscaleRegion(gemSheet, 0, 4 * SpriteSize, 5 * SpriteSize, 3 * SpriteSize);
        Texture2D background = Content.Load<Texture2D>("background_blur");
        _backgroundBlur = new TextureRegion (
            background,
            0,
            0,
            background.Width,
            background.Height
        );
        _gemAtlas = new TextureAtlas(gemSheet);
        AddGem("orange", 0, 0);
        AddGem("blue", 0, 1);
        AddGem("red", 0, 2);
        AddGem("green", 0, 3);
        AddGem("purple", 0, 4);
        AddGem("orangeLineH", 4, 0);
        AddGem("blueLineH", 4, 0);
        AddGem("redLineH", 4, 0);
        AddGem("greenLineH", 4, 0);
        AddGem("purpleLineH", 4, 0);
         AddGem("orangeLineV", 4, 1);
        AddGem("blueLineV", 4, 1);
        AddGem("redLineV", 4, 1);
        AddGem("greenLineV", 4, 1);
        AddGem("purpleLineV", 4, 1);
        AddGem("orangeBomb", 4, 2);
        AddGem("blueBomb", 4, 2);
        AddGem("redBomb", 4, 2);
        AddGem("greenBomb", 4, 2);
        AddGem("purpleBomb", 4, 2);
        Texture2D explosionSheet = Content.Load<Texture2D>("explotion");
        var explosionFrames = new List<TextureRegion>();
        for (int i = 0; i < ExplosionFrameCount; i++) {
            int x = ExplosionFramesHorizontal ? i * ExplosionFrameSize : 0;
            int y = ExplosionFramesHorizontal ? 0 : i * ExplosionFrameSize;
            explosionFrames.Add(new TextureRegion(explosionSheet, x, y, ExplosionFrameSize, ExplosionFrameSize));
        }
        _explosionAnimation = new Animation(explosionFrames, TimeSpan.FromMilliseconds(150));
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        _time += (float)gameTime.ElapsedGameTime.TotalSeconds;
        Window.Title = $"Match3 — Score: {_board.Score}";
        for (int i = _activeAnimations.Count - 1; i >= 0; i--) {
            var (sprite, pos, delay) = _activeAnimations[i];
            float deltaSec = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (delay > 0f) {
                delay -= deltaSec;
                _activeAnimations[i] = (sprite, pos, delay);
            } else {
                sprite.Update(gameTime);
                if (sprite.IsFinished) {
                    _activeAnimations.RemoveAt(i);
                }
            }
        }
        for (int i = _movingCells.Count - 1; i >= 0; i--) {
            _movingCells[i].tween.Update(gameTime);
            if (_movingCells[i].tween.IsFinished) {
                _movingCells.RemoveAt(i);
            }
        }
        if (_pendingCycle && _movingCells.Count == 0) {
            _board.CycleTick();
            SpawnAnimationsFromEvents();
            _pendingCycle = false;
            _state = GameState.Idle;
            }
        MouseState currentMouseState = Mouse.GetState();
        bool clicked = currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
         switch (_state) {
            case GameState.Idle:
                if (clicked) {
                    var (row, col) = PixelToCell(currentMouseState.X, currentMouseState.Y);
                    if (row != -1) {
                        _selectedRow = row;
                        _selectedCol = col;
                        _state = GameState.Selected;
                    }
                } break;
            case GameState.Selected:
                if (clicked) {
                    var (row, col) = PixelToCell(currentMouseState.X, currentMouseState.Y);
                        if (row != -1) {
                            if (row == _selectedRow && col == _selectedCol) {
                                _state = GameState.Idle;
                            } else {
                                var cellA = _board.GetCell(_selectedRow, _selectedCol);
                                var cellB = _board.GetCell(row, col);
                                bool tryMove = _board.TryMakeMove(_selectedRow, _selectedCol, row, col);
                                if (tryMove) {
                                    _state = GameState.Resolving;
                                    StartSwapAnimation(_selectedRow, _selectedCol, row, col, cellA, cellB);
                                    _pendingCycle = true;
                                } else {
                                    _state = GameState.Idle;
                                    }
                            }
                        }
                } break;
            case GameState.Resolving:
            // TODO
            break;
         }
        _previousMouseState = currentMouseState;

        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        var (offsetX, offsetY) = GetBoardOffset();
        GraphicsDevice.Clear(Color.CornflowerBlue);
        bool hasSelection = (_state == GameState.Selected);
        float pulseScale = 1f + 0.1f * (float)Math.Cos(_time * 5);
        _spriteBatch.Begin();
        _backgroundBlur.Draw(
            _spriteBatch,
            GraphicsDevice.Viewport.Bounds,
            Color.White
        );

        var animatingCells = new HashSet<(int, int)>();
        foreach (var (r, c, _, _) in _movingCells) {
            animatingCells.Add((r, c));
        }

        for (int col=0; col < Board.Cols; col++) {
            for (int row=0; row < Board.Rows; row++) {
                if (animatingCells.Contains((row, col))) continue;

                Cell cell = _board.GetCell(row, col);
                string name = GetRegionName(cell.Color, cell.Bonus);
                TextureRegion region = _gemAtlas.GetRegion(name);
                float scale = (hasSelection && row == _selectedRow && col == _selectedCol) ? pulseScale : 1f;
                int scaledSize = (int)(CellSize * scale);
                int centerX = offsetX + col * CellSize + CellSize / 2;
                int centerY = offsetY + row * CellSize + CellSize / 2;
                Rectangle dest = new Rectangle(
                    centerX - scaledSize / 2,
                    centerY - scaledSize / 2,
                    CellSize - 4,
                    CellSize - 4
                    );
                Color tint = cell.Bonus != BonusType.None ? GemColorToTint(cell.Color) : Color.White;
                region.Draw(_spriteBatch, dest, tint);
            }
        }
        foreach (var (_, _, cell, tween) in _movingCells) {
            string name = GetRegionName(cell.Color, cell.Bonus);
            TextureRegion region = _gemAtlas.GetRegion(name);
            Vector2 pos = tween.Current;
            Rectangle dest = new Rectangle(
                (int)pos.X, (int)pos.Y,
                CellSize - 4, CellSize - 4
            );
            Color tint = cell.Bonus != BonusType.None ? GemColorToTint(cell.Color) : Color.White;
            region.Draw(_spriteBatch, dest, tint);
        }
        foreach (var (sprite, position, delay) in _activeAnimations) {
            if (delay <= 0f) {
                sprite.Draw(_spriteBatch, position);
            }
        }
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
