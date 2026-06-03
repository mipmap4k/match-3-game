namespace Match3.Logic;
public readonly record struct Cell(GemColor Color = GemColor.None, BonusType Bonus = BonusType.None);
public enum GemColor {Green, Yellow, Blue, Red, None};
public enum BonusType {None, LineH, LineV, Bomb};