namespace Match3.Logic;
public class Board {
    public const int Rows = 8;
    public const int Cols = 8;
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
    }
    public Board(Cell[,] initBoard) {
        gameBoard = initBoard;
    }
    public void CycleTick() {
        while (true) {
            var matches = FindAllMatches();
            if (matches.Count == 0) {
                break;
            }
            var bombPositions = FindIntersections(matches);
            var bonusQueue = new Queue<(int row, int col, BonusType bonus)>();
            RemoveMatches(matches, bombPositions, bonusQueue);
            while (bonusQueue.Count > 0) {
                var (row, col, bonus) = bonusQueue.Dequeue();
                TriggerBonus(row, col, bonus, bonusQueue);
            }
            ApplyGravity();
            SpawnNewGem();
        }
    }
    public bool TryMakeMove(int startRow, int startCol, int endRow, int endCol) {
        bool areNeighbor = (startRow == endRow && Math.Abs(startCol - endCol) == 1) || (startCol == endCol && Math.Abs(startRow - endRow) == 1);
        if (!areNeighbor) return false; 
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
        Queue<(int row, int col, BonusType bonus)> bonusQueue
         ) {
        foreach (var match in matches) {
            foreach (var (row, col) in match.Cells) {
                BonusType currBonus = gameBoard[row, col].Bonus;
                if (currBonus != BonusType.None) {
                    bonusQueue.Enqueue((row, col, currBonus));
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
                    gameBoard[row, col] = new Cell(match.Color, BonusType.Bomb);
                } else if (bonus != BonusType.None && (row, col) == bonusPos) {
                    gameBoard[row, col] = new Cell(match.Color, bonus);
                } else {
                    gameBoard[row, col] = new Cell(GemColor.None);
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
    private void TriggerBonus(int row, int col, BonusType bonus, Queue<(int row, int col, BonusType bonus)> bonusQueue) {
        switch(bonus) {
            case BonusType.LineH:
                for (int c=0; c < Cols; c++) {
                    if (c != col) {
                        BonusType existingBonus = gameBoard[row, c].Bonus;
                        if (existingBonus != BonusType.None) {
                            bonusQueue.Enqueue((row, c, existingBonus));
                        }
                    }
                    gameBoard[row,c] = new Cell(GemColor.None);
                } break;
            case BonusType.LineV:
                for (int r=0; r < Rows; r++){
                    if (r != row) {
                        BonusType existingBonus = gameBoard[r, col].Bonus;
                        if (existingBonus != BonusType.None) {
                            bonusQueue.Enqueue((r, col, existingBonus));
                        }
                    }
                    gameBoard[r, col] = new Cell(GemColor.None);
                } break;
            case BonusType.Bomb:
                for (int r = row - 1; r <= row + 1; r++) {
                    for (int c = col - 1; c <= col + 1; c++) {
                        if (r >= 0 && r < Rows && c >= 0 && c < Cols) {
                            if (r != row || c != col) {
                                BonusType existing = gameBoard[r,c].Bonus;
                                if (existing != BonusType.None) {
                                    bonusQueue.Enqueue((r, c, existing));
                                }
                            }
                            gameBoard[r,c] = new Cell(GemColor.None);
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