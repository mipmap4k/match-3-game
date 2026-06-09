using Match3.Logic;
using Match3.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace Match3.Desktop.Scenes;

public class GameScene : Scene
{
    private Board _board;
    private MouseState _previousMouseState;
    private int _selectedRow;
    private int _selectedCol;
    private float _time = 0f;
    private TextureAtlas _gemAtlas = null!;
    private TextureRegion _backgroundBlur = null!;
    private Animation _explosionAnimation = null!;
    private List<(AnimatedSprite sprite, Vector2 position, float delay)> _activeAnimations = new();
    private List<(int row, int col, Cell cell, PositionTween tween, bool needReverse)> _movingCells = new();
    private List<(int row, int col, Cell wasCell, FloatTween fade)> _fadingCells = new();
    private List<(int row, int col, Cell wasCell, Cell newCell, FloatTween progress)> _appearingBonuses = new();
    private List<(Cell spriteCell, PositionTween tween)> _flyingDestroyers = new();
    private bool _pendingCycle = false;
    private const float FadeDuration = 0.25f;
    private const float AppearDuration = 0.35f;
    private const float LineWaveDelay = 0.07f;
    private GameState _state = GameState.Idle;
    private const int SpriteSize = 100;
    private const int CellSize = 64;
    private const int ExplosionFrameSize = 100;
    private const int ExplosionFrameCount = 4;
    private const bool ExplosionFramesHorizontal = true;
    private const float SwapDuration = 0.25f;
    private const float FallPixelsPerSecond = 600f;

    private enum GameState {
        Idle,
        Selected,
        SwapAnimating,
        RemoveAnimating,
        FallAnimating
    };

    public GameScene(Game1 game) : base(game) {
        _board = new Board();
    }

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
        int offsetX = (Game.GraphicsDevice.Viewport.Width - boardWidth) / 2;
        int offsetY = (Game.GraphicsDevice.Viewport.Height - boardHeight) / 2;
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
        foreach (var (row, col, bonus, color) in _board.LastTickEvents) {
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
                        float delay = Math.Abs(c - col) * LineWaveDelay;
                        AddExplosion(row, c, delay);
                    }
                    AddFlyingDestroyer(row, col, BonusType.LineH, color);
                    break;
                case BonusType.LineV:
                    for (int r = 0; r < Board.Rows; r++) {
                        float delay = Math.Abs(r - row) * LineWaveDelay;
                        AddExplosion(r, col, delay);
                    }
                    AddFlyingDestroyer(row, col, BonusType.LineV, color);
                    break;
            }
        }
        _board.LastTickEvents.Clear();
    }
    private void AddFlyingDestroyer(int row, int col, BonusType type, GemColor color) {
        var (offsetX, offsetY) = GetBoardOffset();
        Vector2 startPos = CellToPixel(row, col, offsetX, offsetY);
        Cell spriteCell = new Cell(color, type);
        if (type == BonusType.LineH) {
            Vector2 leftEnd = CellToPixel(row, -1, offsetX, offsetY);
            float leftDuration = col * LineWaveDelay;
            if (leftDuration > 0f) {
                _flyingDestroyers.Add((spriteCell, new PositionTween(startPos, leftEnd, leftDuration)));
            }
            Vector2 rightEnd = CellToPixel(row, Board.Cols, offsetX, offsetY);
            float rightDuration = (Board.Cols - 1 - col) * LineWaveDelay;
            if (rightDuration > 0f) {
                _flyingDestroyers.Add((spriteCell, new PositionTween(startPos, rightEnd, rightDuration)));
            }
        } else if (type == BonusType.LineV) {
            Vector2 topEnd = CellToPixel(-1, col, offsetX, offsetY);
            float topDuration = row * LineWaveDelay;
            if (topDuration > 0f) {
                _flyingDestroyers.Add((spriteCell, new PositionTween(startPos, topEnd, topDuration)));
            }
            Vector2 botEnd = CellToPixel(Board.Rows, col, offsetX, offsetY);
            float botDuration = (Board.Rows - 1 - row) * LineWaveDelay;
            if (botDuration > 0f) {
                _flyingDestroyers.Add((spriteCell, new PositionTween(startPos, botEnd, botDuration)));
            }
        }
    }

    private void StartSwapAnimation(int rowA, int colA, int rowB, int colB, Cell cellA, Cell cellB, bool reverse = false) {
        var (offsetX, offsetY) = GetBoardOffset();
        Vector2 posA = CellToPixel(rowA, colA, offsetX, offsetY);
        Vector2 posB = CellToPixel(rowB, colB, offsetX, offsetY);

        _movingCells.Add((rowB, colB, cellA, new PositionTween(posA, posB, SwapDuration), reverse));
        _movingCells.Add((rowA, colA, cellB, new PositionTween(posB, posA, SwapDuration), reverse));
    }

    private Vector2 CellToPixel(int row, int col, int offsetX, int offsetY) {
        return new Vector2(offsetX + col * CellSize, offsetY + row * CellSize);
    }
    private void EnterRemoveAnimating() {
        bool changed = _board.TryRemoveStep();
        if (!changed) {
            _state = GameState.Idle;
            return;
        }
        StartFadeForRemovedCells();
        StartAppearForCreatedBonuses();
        SpawnAnimationsFromEvents();
        _state = GameState.RemoveAnimating;
    }
    private void StartAppearForCreatedBonuses() {
        foreach (var (row, col, bonus, color, wasCell) in _board.CreatedBonuses) {
            var newCell = new Cell(color, bonus);
            var progress = new FloatTween(0f, 1f, AppearDuration);
            _appearingBonuses.Add((row, col, wasCell, newCell, progress));
        }
        _board.CreatedBonuses.Clear();
    }
    private void StartFadeForRemovedCells() {
        foreach (var (row, col, wasCell) in _board.RemovedCells) {
            float delay = ComputeBonusDelay(row, col);
            var fade = new FloatTween(1f, 0f, FadeDuration, delay);
            _fadingCells.Add((row, col, wasCell, fade));
        }
        _board.RemovedCells.Clear();
    }
    private float ComputeBonusDelay(int row, int col) {
        float minDelay = float.PositiveInfinity;
        foreach (var (br, bc, bonus, _) in _board.LastTickEvents) {
            float? d = null;
            if (bonus == BonusType.Bomb && Math.Abs(row - br) <= 1 && Math.Abs(col - bc) <= 1) {
                d = 0f;
            } else if (bonus == BonusType.LineH && row == br) {
                d = Math.Abs(col - bc) * LineWaveDelay;
            } else if (bonus == BonusType.LineV && col == bc) {
                d = Math.Abs(row - br) * LineWaveDelay;
            }
            if (d.HasValue && d.Value < minDelay) minDelay = d.Value;
        }
        return float.IsInfinity(minDelay) ? 0f : minDelay;
    }

    private void EnterFallAnimating() {
        var snapshot = SnapshotBoard();
        _board.ApplyGravityAndSpawn();
        StartFallForChangedCells(snapshot);
        _state = GameState.FallAnimating;
    }
    private Cell[,] SnapshotBoard() {
        var snap = new Cell[Board.Rows, Board.Cols];
        for (int r = 0; r < Board.Rows; r++) {
            for (int c = 0; c < Board.Cols; c++) {
                snap[r, c] = _board.GetCell(r, c);
            }
        }
        return snap;
    }
    private void StartFallForChangedCells(Cell[,] before) {
        var (offsetX, offsetY) = GetBoardOffset();
        for (int col = 0; col < Board.Cols; col++) {
            var oldRows = new List<int>();
            for (int r = 0; r < Board.Rows; r++) {
                if (before[r, col].Color != GemColor.None) {
                    oldRows.Add(r);
                }
            }
            int existingCount = oldRows.Count;
            int newCount = Board.Rows - existingCount;
            for (int i = 0; i < existingCount; i++) {
                int fromRow = oldRows[i];
                int toRow = Board.Rows - existingCount + i;
                if (fromRow == toRow) continue;

                Cell cell = _board.GetCell(toRow, col);
                Vector2 startPos = CellToPixel(fromRow, col, offsetX, offsetY);
                Vector2 endPos = CellToPixel(toRow, col, offsetX, offsetY);
                float distance = (toRow - fromRow) * CellSize;
                float duration = distance / FallPixelsPerSecond;
                _movingCells.Add((toRow, col, cell, new PositionTween(startPos, endPos, duration), false));
            }
            for (int r = 0; r < newCount; r++) {
                Cell cell = _board.GetCell(r, col);
                if (cell.Color == GemColor.None) continue;
                Vector2 endPos = CellToPixel(r, col, offsetX, offsetY);
                Vector2 startPos = new Vector2(endPos.X, offsetY - (newCount - r) * CellSize);
                float distance = endPos.Y - startPos.Y;
                float duration = distance / FallPixelsPerSecond;
                _movingCells.Add((r, col, cell, new PositionTween(startPos, endPos, duration), false));
            }
        }
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

    public override void LoadContent()
    {
        Texture2D gemSheet = Game.Content.Load<Texture2D>("assets_candy");
        GrayscaleRegion(gemSheet, 0, 4 * SpriteSize, 5 * SpriteSize, 3 * SpriteSize);
        Texture2D background = Game.Content.Load<Texture2D>("background_blur");
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
        Texture2D explosionSheet = Game.Content.Load<Texture2D>("explotion");
        var explosionFrames = new List<TextureRegion>();
        for (int i = 0; i < ExplosionFrameCount; i++) {
            int x = ExplosionFramesHorizontal ? i * ExplosionFrameSize : 0;
            int y = ExplosionFramesHorizontal ? 0 : i * ExplosionFrameSize;
            explosionFrames.Add(new TextureRegion(explosionSheet, x, y, ExplosionFrameSize, ExplosionFrameSize));
        }
        _explosionAnimation = new Animation(explosionFrames, TimeSpan.FromMilliseconds(150));
        _previousMouseState = Mouse.GetState();
    }

    public override void Update(GameTime gameTime)
    {
        _time += (float)gameTime.ElapsedGameTime.TotalSeconds;
        Game.Window.Title = $"Match3 — Score: {_board.Score}";
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
            var (row, col, cell, tween, needReverse) = _movingCells[i];
            tween.Update(gameTime);
            if (tween.IsFinished) {
                if (needReverse) {
                    var backward = new PositionTween(tween.End, tween.Start, SwapDuration);
                    _movingCells[i] = (row, col, cell, backward, false);
                } else {
                    _movingCells.RemoveAt(i);
                }
            }
        }
        if (_state == GameState.SwapAnimating && _movingCells.Count == 0) {
            if (_pendingCycle) {
                _pendingCycle = false;
                EnterRemoveAnimating();
            } else {
                _state = GameState.Idle;
            }
        }
        for (int i = _fadingCells.Count - 1; i >= 0; i--) {
            _fadingCells[i].fade.Update(gameTime);
            if (_fadingCells[i].fade.IsFinished) {
                _fadingCells.RemoveAt(i);
            }
        }
        for (int i = _appearingBonuses.Count - 1; i >= 0; i--) {
            _appearingBonuses[i].progress.Update(gameTime);
            if (_appearingBonuses[i].progress.IsFinished) {
                _appearingBonuses.RemoveAt(i);
            }
        }
        for (int i = _flyingDestroyers.Count - 1; i >= 0; i--) {
            _flyingDestroyers[i].tween.Update(gameTime);
            if (_flyingDestroyers[i].tween.IsFinished) {
                _flyingDestroyers.RemoveAt(i);
            }
        }
        if (_state == GameState.RemoveAnimating
                && _movingCells.Count == 0
                && _activeAnimations.Count == 0
                && _fadingCells.Count == 0
                && _appearingBonuses.Count == 0
                && _flyingDestroyers.Count == 0) {
            EnterFallAnimating();
        }
        if (_state == GameState.FallAnimating && _movingCells.Count == 0) {
            EnterRemoveAnimating();
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
                                if (!Board.AreNeighbors(_selectedRow, _selectedCol, row, col)) {
                                    _state = GameState.Idle;
                                } else {
                                    var cellA = _board.GetCell(_selectedRow, _selectedCol);
                                    var cellB = _board.GetCell(row, col);
                                    bool tryMove = _board.TryMakeMove(_selectedRow, _selectedCol, row, col);
                                    if (tryMove) {
                                        _state = GameState.SwapAnimating;
                                        StartSwapAnimation(_selectedRow, _selectedCol, row, col, cellA, cellB);
                                        _pendingCycle = true;
                                    } else {
                                        _state = GameState.SwapAnimating;
                                        StartSwapAnimation(_selectedRow, _selectedCol, row, col, cellA, cellB, reverse: true);
                                    }
                                }
                            }
                        }
                } break;
            case GameState.SwapAnimating:
            case GameState.RemoveAnimating:
            case GameState.FallAnimating:
                break;
         }
        _previousMouseState = currentMouseState;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var (offsetX, offsetY) = GetBoardOffset();
        bool hasSelection = (_state == GameState.Selected);
        float pulseScale = 1f + 0.1f * (float)Math.Cos(_time * 5);
        _backgroundBlur.Draw(
            spriteBatch,
            Game.GraphicsDevice.Viewport.Bounds,
            Color.White
        );

        var animatingCells = new HashSet<(int, int)>();
        foreach (var (r, c, _, _, _) in _movingCells) {
            animatingCells.Add((r, c));
        }
        foreach (var (r, c, _, _, _) in _appearingBonuses) {
            animatingCells.Add((r, c));
        }

        for (int col=0; col < Board.Cols; col++) {
            for (int row=0; row < Board.Rows; row++) {
                if (animatingCells.Contains((row, col))) continue;

                Cell cell = _board.GetCell(row, col);
                if (cell.Color == GemColor.None) continue;
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
                region.Draw(spriteBatch, dest, tint);
            }
        }
        foreach (var (_, _, cell, tween, _) in _movingCells) {
            string name = GetRegionName(cell.Color, cell.Bonus);
            TextureRegion region = _gemAtlas.GetRegion(name);
            Vector2 pos = tween.Current;
            Rectangle dest = new Rectangle(
                (int)pos.X, (int)pos.Y,
                CellSize - 4, CellSize - 4
            );
            Color tint = cell.Bonus != BonusType.None ? GemColorToTint(cell.Color) : Color.White;
            region.Draw(spriteBatch, dest, tint);
        }
        foreach (var (row, col, wasCell, newCell, progress) in _appearingBonuses) {
            float t = progress.Current;
            Cell shownCell;
            float scale, alpha;
            if (t < 0.5f) {
                shownCell = wasCell;
                float lt = t / 0.5f;
                scale = 1f - lt;
                alpha = 1f - lt;
            } else {
                shownCell = newCell;
                float lt = (t - 0.5f) / 0.5f;
                scale = lt;
                alpha = lt;
            }
            if (scale <= 0f || alpha <= 0f) continue;
            string name = GetRegionName(shownCell.Color, shownCell.Bonus);
            TextureRegion region = _gemAtlas.GetRegion(name);
            int scaledSize = (int)(CellSize * scale);
            int centerX = offsetX + col * CellSize + CellSize / 2;
            int centerY = offsetY + row * CellSize + CellSize / 2;
            Rectangle dest = new Rectangle(
                centerX - scaledSize / 2,
                centerY - scaledSize / 2,
                scaledSize - 4, scaledSize - 4
            );
            Color baseTint = shownCell.Bonus != BonusType.None ? GemColorToTint(shownCell.Color) : Color.White;
            Color tint = new Color(baseTint.R, baseTint.G, baseTint.B, (byte)(alpha * 255));
            region.Draw(spriteBatch, dest, tint);
        }
        foreach (var (row, col, wasCell, fade) in _fadingCells) {
            float alpha = fade.Current;
            if (alpha <= 0f) continue;
            float scale = alpha;
            string name = GetRegionName(wasCell.Color, wasCell.Bonus);
            TextureRegion region = _gemAtlas.GetRegion(name);
            int scaledSize = (int)(CellSize * scale);
            int centerX = offsetX + col * CellSize + CellSize / 2;
            int centerY = offsetY + row * CellSize + CellSize / 2;
            Rectangle dest = new Rectangle(
                centerX - scaledSize / 2,
                centerY - scaledSize / 2,
                scaledSize - 4, scaledSize - 4
            );
            Color baseTint = wasCell.Bonus != BonusType.None ? GemColorToTint(wasCell.Color) : Color.White;
            Color tint = new Color(baseTint.R, baseTint.G, baseTint.B, (byte)(alpha * 255));
            region.Draw(spriteBatch, dest, tint);
        }
        foreach (var (sprite, position, delay) in _activeAnimations) {
            if (delay <= 0f) {
                sprite.Draw(spriteBatch, position);
            }
        }
        foreach (var (spriteCell, tween) in _flyingDestroyers) {
            Vector2 pos = tween.Current;
            string name = GetRegionName(spriteCell.Color, spriteCell.Bonus);
            TextureRegion region = _gemAtlas.GetRegion(name);
            Rectangle dest = new Rectangle(
                (int)pos.X, (int)pos.Y,
                CellSize - 4, CellSize - 4
            );
            Color tint = GemColorToTint(spriteCell.Color);
            region.Draw(spriteBatch, dest, tint);
        }
    }
}
