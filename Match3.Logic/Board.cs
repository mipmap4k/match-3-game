namespace Match3.Logic;
public class Board {
    public const int Rows = 5;
    public const int Cols = 5;
    private static int GemColorsCount => Enum.GetValues<GemColor>().Length - 1;
    private Cell[,] gameBoard = new Cell[Cols,Rows];

    public Board() {
        for (int i=0; i < gameBoard.GetLength(0); i++) {
            for (int j=0; j < gameBoard.GetLength(1); j++) {
                gameBoard[i,j] = RandomGem();
            }
        }
        CycleTick();
    }
    public Board(Cell[,] initBoard) {
        gameBoard = initBoard;
    }
    public void CycleTick() {
        while (true) {
            HashSet<(int,int)> matches = FindAllMatches();
            if (matches.Count == 0) {
                break;
            }
            RemoveMatches(matches);
            ApplyGravity();
            SpawnNewGem();
        }
    }
    public bool TryMakeMove(int startRow, int startCol, int endRow, int endCol) {
        bool areNeighbor = ((startRow == endRow && Math.Abs(startCol - endCol) == 1) || (startCol == endCol && Math.Abs(startRow - endRow) == 1));
        if (!areNeighbor) return false; 
        Swap(startRow, startCol, endRow, endCol);
        if (!HasMatches()) {
            Swap(startRow, startCol, endRow, endCol);
            return false;
        }
        return true;   
    }
    public Cell GetCell(int row, int col) {
        return gameBoard[row,col];
    }
    public bool HasMatches() {
        return FindAllMatches().Count > 0;
    }
    private HashSet<(int, int)> FindAllMatches() {
        var allMatches = new HashSet<(int,int)>(FindHorizontal());
        allMatches.UnionWith(FindVertical());
        return allMatches;
    }
    private void RemoveMatches(HashSet<(int, int)> matches) {
        foreach (var (r,c) in matches) {
            gameBoard[r,c] = new Cell(GemColor.None);
        }
    }
    private void ApplyGravity() {
        for (int j=0; j < gameBoard.GetLength(1); j++) {
            List<Cell> gems = [];
            for (int i=0; i < gameBoard.GetLength(0); i++) {
                if (gameBoard[i,j].Color != GemColor.None) {
                    gems.Add(gameBoard[i,j]);
                }
            }
            for (int i=0; i < gameBoard.GetLength(0); i++) {
                gameBoard[i,j] = new Cell(GemColor.None);
            }
            for (int i = gameBoard.GetLength(0)- 1; i >= 0 && gems.Count > 0; i--) {
                gameBoard[i,j] = gems[^1];
                gems.RemoveAt(gems.Count - 1);      
            }
        }
    }
    private void SpawnNewGem() {
        for (int i=0; i < gameBoard.GetLength(0); i++) {
            for (int j=0; j < gameBoard.GetLength(1); j++) {
                if (gameBoard[i,j].Color == GemColor.None) {
                    gameBoard[i,j] = RandomGem();
                }
            }
        }
    }
    private List<(int, int)> FindHorizontal() {
        List<(int, int)> pos = [];
        for (int i=0; i < gameBoard.GetLength(0); i++) {
            int start = 0;
            GemColor currColor = gameBoard[i,0].Color;
            for (int j=0; j < gameBoard.GetLength(1); j++) {
                if (currColor != gameBoard[i,j].Color) {
                    int lenIn = j - start;
                    if (lenIn >= 3) {
                        for (int k=start; k < j; k++) {
                            pos.Add((i,k));
                        }
                    }
                    start = j;
                    currColor = gameBoard[i,j].Color;
                }
            }
            int lenOut = gameBoard.GetLength(1) - start;
            if (lenOut >= 3) {
                for (int k=start; k < gameBoard.GetLength(1); k++) {
                    pos.Add((i,k));
                }
            }
        }
        return pos;
    }

    private List<(int, int)> FindVertical() {
        List<(int,int)> pos = [];
        for (int j=0; j < gameBoard.GetLength(1); j++) {
            int start = 0;
            GemColor currColor = gameBoard[0,j].Color;
            for (int i=0; i < gameBoard.GetLength(0); i++) {
                if (currColor != gameBoard[i,j].Color) {
                    int lenIn = i - start;
                    if (lenIn >= 3) {
                        for (int k=start; k < i; k++) {
                            pos.Add((k,j));
                        }
                    }
                    start = i;
                    currColor = gameBoard[i,j].Color;
                }
            }
            int lenOut = gameBoard.GetLength(0) - start;
            if (lenOut >= 3) {
                for (int k=start; k < gameBoard.GetLength(0); k++) {
                    pos.Add((k,j));
                }
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