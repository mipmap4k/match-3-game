namespace Match3.Logic;
public class Board {
    public const int Rows = 8;
    public const int Cols = 8;
    public int Score { get; private set; } = 0;
    public List<(int row, int col, BonusType bonus, GemColor color)> LastTickEvents { get; } = new();
    public List<(int row, int col, BonusType bonus, GemColor color, Cell wasCell)> CreatedBonuses { get; } = new();
    public List<(int row, int col, Cell wasCell)> RemovedCells { get; } = new();
    private static int GemColorsCount => Enum.GetValues<GemColor>().Length - 1;
    private Cell[,] gameBoard = new Cell[Cols,Rows];
    private (int row, int col)? lastMoveEnd = null;

    public Board() {
        for (int row=0; row < gameBoard.GetLength(0); row++) {
            for (int col=0; col < gameBoard.GetLength(1); col++) {
                gameBoard[row,col] = RandomGem();
            }
        }
        CycleTick();
        Score = 0;
    }
    public Board(Cell[,] initBoard) {
        gameBoard = initBoard;
    }
    public void CycleTick() {
        while (true) {
            if (!TryRemoveStep()) break;
            ApplyGravityAndSpawn();
        }
    }
    public bool TryRemoveStep() {
        LastTickEvents.Clear();
        CreatedBonuses.Clear();
        RemovedCells.Clear();

        var matches = FindAllMatches();
        if (matches.Count == 0) return false;
        var bombPositions = FindIntersections(matches);
        var bonusQueue = new Queue<(int row, int col, BonusType bonus, GemColor color)>();
        RemoveMatches(matches, bombPositions, bonusQueue);
        while (bonusQueue.Count > 0) {
            var (row, col, bonus, color) = bonusQueue.Dequeue();
            TriggerBonus(row, col, bonus, color, bonusQueue);
        }
        return true;
    }
    public static bool AreNeighbors(int startRow, int startCol, int endRow, int endCol) {
        return (startRow == endRow && Math.Abs(startCol - endCol) == 1) || (startCol == endCol && Math.Abs(startRow - endRow) == 1);
    }
    public void ApplyGravityAndSpawn() {
        ApplyGravity();
        SpawnNewGem();
    }
    public bool TryMakeMove(int startRow, int startCol, int endRow, int endCol) {
        if (!AreNeighbors(startRow, startCol, endRow, endCol)) return false; 
        Swap(startRow, startCol, endRow, endCol);
        if (!HasMatches()) {
            Swap(startRow, startCol, endRow, endCol);
            return false;
        }
        lastMoveEnd = (endRow, endCol);
        return true;   
    }
    public Cell GetCell(int row, int col) {
        return gameBoard[row,col];
    }
    public bool HasMatches() {
        return FindAllMatches().Count > 0;
    }
    private List<Match> FindAllMatches() {
        List<Match> allMatches = [];
        allMatches.AddRange(FindHorizontal());
        allMatches.AddRange(FindVertical());
        return allMatches;
    }
    private void RemoveMatches(
        List<Match> matches,
        HashSet<(int, int)> bombPositions,
        Queue<(int row, int col, BonusType bonus, GemColor color)> bonusQueue
         ) {
        foreach (var match in matches) {
            foreach (var (row, col) in match.Cells) {
                BonusType currBonus = gameBoard[row, col].Bonus;
                if (currBonus != BonusType.None) {
                    bonusQueue.Enqueue((row, col, currBonus, gameBoard[row, col].Color));
                }
            }
        }
        foreach (var match in matches) {
            BonusType bonus = BonusType.None;
            if (match.Cells.Count == 4) {
                bonus = match.IsHorizontal ? BonusType.LineH : BonusType.LineV;
            } else if (match.Cells.Count >= 5) {
                bonus = BonusType.Bomb;
            }
            (int row, int col) bonusPos = (-1,-1);
            if (bonus != BonusType.None) {
                if (lastMoveEnd.HasValue && match.Cells.Contains(lastMoveEnd.Value)) {
                    bonusPos = lastMoveEnd.Value;
                } else {
                    bonusPos = match.Cells[match.Cells.Count / 2];
                }
            }
            foreach (var (row,col) in match.Cells) {
                if (bombPositions.Contains((row, col))) {
                    Cell wasCell = gameBoard[row, col];
                    if (gameBoard[row, col].Bonus != BonusType.Bomb || gameBoard[row, col].Color != match.Color) {
                        gameBoard[row, col] = new Cell(match.Color, BonusType.Bomb);
                        if (!CreatedBonusesContains(row, col)) {
                            CreatedBonuses.Add((row, col, BonusType.Bomb, match.Color, wasCell));
                        }
                    }
                } else if (bonus != BonusType.None && (row, col) == bonusPos) {
                    Cell wasCell = gameBoard[row, col];
                    gameBoard[row, col] = new Cell(match.Color, bonus);
                    if (!CreatedBonusesContains(row, col)) {
                        CreatedBonuses.Add((row, col, bonus, match.Color, wasCell));
                    }
                } else {
                    RemoveCell(row, col);
                }
            }
        }
    }
    private void ApplyGravity() {
        for (int col=0; col < gameBoard.GetLength(1); col++) {
            List<Cell> gems = [];
            for (int row=0; row < gameBoard.GetLength(0); row++) {
                if (gameBoard[row,col].Color != GemColor.None) {
                    gems.Add(gameBoard[row,col]);
                }
            }
            for (int row=0; row < gameBoard.GetLength(0); row++) {
                gameBoard[row,col] = new Cell(GemColor.None);
            }
            for (int row = gameBoard.GetLength(0)- 1; row >= 0 && gems.Count > 0; row--) {
                gameBoard[row,col] = gems[^1];
                gems.RemoveAt(gems.Count - 1);      
            }
        }
    }
    private void SpawnNewGem() {
        for (int row=0; row < gameBoard.GetLength(0); row++) {
            for (int col=0; col < gameBoard.GetLength(1); col++) {
                if (gameBoard[row,col].Color == GemColor.None) {
                    gameBoard[row,col] = RandomGem();
                }
            }
        }
    }
    private bool CreatedBonusesContains(int row, int col) {
        foreach (var (r, c, _, _, _) in CreatedBonuses) {
            if (r == row && c == col) return true;
        }
        return false;
    }

    private void RemoveCell(int row, int col) {
        Cell wasCell = gameBoard[row, col];
        if (wasCell.Color != GemColor.None) {
            Score += 10;
            RemovedCells.Add((row, col, wasCell));
        }
        gameBoard[row, col] = new Cell(GemColor.None);
    }
    private void TriggerBonus(int row, int col, BonusType bonus, GemColor bonusColor, Queue<(int row, int col, BonusType bonus, GemColor color)> bonusQueue) {
        LastTickEvents.Add((row, col, bonus, bonusColor));
        switch(bonus) {
            case BonusType.LineH:
                for (int c=0; c < Cols; c++) {
                    if (c != col) {
                        BonusType existingBonus = gameBoard[row, c].Bonus;
                        if (existingBonus != BonusType.None) {
                            bonusQueue.Enqueue((row, c, existingBonus, gameBoard[row, c].Color));
                        }
                    }
                    RemoveCell(row, c);
                } break;
            case BonusType.LineV:
                for (int r=0; r < Rows; r++){
                    if (r != row) {
                        BonusType existingBonus = gameBoard[r, col].Bonus;
                        if (existingBonus != BonusType.None) {
                            bonusQueue.Enqueue((r, col, existingBonus, gameBoard[r, col].Color));
                        }
                    }
                    RemoveCell(r, col);
                } break;
            case BonusType.Bomb:
                for (int r = row - 1; r <= row + 1; r++) {
                    for (int c = col - 1; c <= col + 1; c++) {
                        if (r >= 0 && r < Rows && c >= 0 && c < Cols) {
                            if (r != row || c != col) {
                                BonusType existing = gameBoard[r,c].Bonus;
                                if (existing != BonusType.None) {
                                    bonusQueue.Enqueue((r, c, existing, gameBoard[r, c].Color));
                                }
                            }
                            RemoveCell(r, c);
                        }
                    }
                } break;
        }
    }
    private HashSet<(int, int)> FindIntersections(List<Match> matches) {
    var bombPositions = new HashSet<(int, int)>();
    
    foreach (var h in matches) {
        if (!h.IsHorizontal) continue;
        
        foreach (var v in matches) {
            if (v.IsHorizontal) continue;
            
            foreach (var cell in h.Cells) {
                if (v.Cells.Contains(cell)) {
                    bombPositions.Add(cell);
                }
            }
        }
    }
    
    return bombPositions;
}
    private List<Match> FindHorizontal() {
        List<Match> pos = [];
        for (int row=0; row < gameBoard.GetLength(0); row++) {
            int start = 0;
            GemColor currColor = gameBoard[row,0].Color;
            for (int col=0; col < gameBoard.GetLength(1); col++) {
                if (currColor != gameBoard[row,col].Color) {
                    int lenIn = col - start;
                    if (lenIn >= 3) {
                        List<(int, int)> groupIn = [];
                        for (int subCol=start; subCol < col; subCol++) {
                            groupIn.Add((row,subCol));
                        }
                        pos.Add(new Match(Cells: groupIn, IsHorizontal: true, Color: currColor));
                    }
                    start = col;
                    currColor = gameBoard[row,col].Color;
                }
            }
            int lenOut = gameBoard.GetLength(1) - start;
            if (lenOut >= 3) {
                List<(int, int)> groupOut = []; 
                for (int subCol=start; subCol < gameBoard.GetLength(1); subCol++) {
                    groupOut.Add((row,subCol));
                }
                pos.Add(new Match(Cells: groupOut, IsHorizontal: true, Color: currColor));
            }
        }
        return pos;
    }

    private List<Match> FindVertical() {
        List<Match> pos = [];
        for (int col=0; col < gameBoard.GetLength(1); col++) {
            int start = 0;
            GemColor currColor = gameBoard[0,col].Color;
            for (int row=0; row < gameBoard.GetLength(0); row++) {
                if (currColor != gameBoard[row,col].Color) {
                    int lenIn = row - start;
                    if (lenIn >= 3) {
                        List<(int, int)> groupIn = [];
                        for (int subCol=start; subCol < row; subCol++) {
                            groupIn.Add((subCol,col));
                        }
                        pos.Add(new Match(Cells: groupIn, IsHorizontal: false, Color: currColor));
                    }
                    start = row;
                    currColor = gameBoard[row,col].Color;
                }
            }
            int lenOut = gameBoard.GetLength(0) - start;
            if (lenOut >= 3) {
                List<(int, int)> groupOut = [];
                for (int subCol=start; subCol < gameBoard.GetLength(0); subCol++) {
                    groupOut.Add((subCol,col));
                }
                pos.Add(new Match(Cells: groupOut, IsHorizontal: false, Color: currColor));
            }
        }
        return pos;
    }
    private void Swap(int startRow, int startCol, int endRow, int endCol) {
        var temp = gameBoard[startRow, startCol];
        gameBoard[startRow, startCol] = gameBoard[endRow, endCol];
        gameBoard[endRow, endCol] = temp;
    }
    private static Cell RandomGem(){
        var color = (GemColor)Random.Shared.Next(GemColorsCount);
        return new Cell(color);
    }
}