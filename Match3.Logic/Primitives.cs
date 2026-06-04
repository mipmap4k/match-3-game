namespace Match3.Logic;
public record Match(List<(int row, int col)> Cells, bool IsHorizontal, GemColor Color);
public readonly record struct Cell(GemColor Color = GemColor.None, BonusType Bonus = BonusType.None);
public enum GemColor {Green, Yellow, Blue, Red, None};
public enum BonusType {None, LineH, LineV, Bomb};