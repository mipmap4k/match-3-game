public class Board {
    private const int Rows = 5;
    private const int Cols = 5;
    private static int GemColorsCount => Enum.GetValues<GemType>().Length - 1;
    private GemType[,] gameBoard = new GemType[Cols,Rows];

    public Board() {
        for (int i=0; i < gameBoard.GetLength(0); i++) {
            for (int j=0; j < gameBoard.GetLength(1); j++) {
                gameBoard[i,j] = RandomGem();
            }
        }
    }
    public Board(GemType[,] initBoard) {
        gameBoard = initBoard;
    }
    public void Update() {
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
    public void Print() {
        for (int i=0; i < gameBoard.GetLength(0); i++) {
            for (int j=0; j < gameBoard.GetLength(1); j++) {
                Console.Write(CompactSymbols(gameBoard[i,j]) + " ");
            }
            Console.WriteLine();
        }
    }
    public bool HasMatches() {
        return FindAllMatches().Count > 0;
    }
    public bool Swap(int startRow, int startCol, int endRow, int endCol) {
        bool areNeighbor = ((startRow == endRow && Math.Abs(startCol - endCol) == 1) || (startCol == endCol && Math.Abs(startRow - endRow) == 1));
        if (!areNeighbor) return false; 
        var temp = gameBoard[startRow, startCol];
        gameBoard[startRow, startCol] = gameBoard[endRow, endCol];
        gameBoard[endRow, endCol] = temp;
        return true;   
    }
    private HashSet<(int, int)> FindAllMatches() {
        var allMatches = new HashSet<(int,int)>(FindHorizontal());
        allMatches.UnionWith(FindVertical());
        return allMatches;
    }
    private void RemoveMatches(HashSet<(int, int)> matches) {
        foreach (var (r,c) in matches) {
            gameBoard[r,c] = GemType.Empty;
        }
    }
    private void ApplyGravity() {
        for (int j=0; j < gameBoard.GetLength(1); j++) {
            List<GemType> gems = [];
            for (int i=0; i < gameBoard.GetLength(0); i++) {
                if (gameBoard[i,j] != GemType.Empty) {
                    gems.Add(gameBoard[i,j]);
                }
            }
            for (int i=0; i < gameBoard.GetLength(0); i++) {
                gameBoard[i,j] = GemType.Empty;
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
                if (gameBoard[i,j] == GemType.Empty) {
                    gameBoard[i,j] = RandomGem();
                }
            }
        }
    }
    private List<(int, int)> FindHorizontal() {
        List<(int, int)> pos = [];
        for (int i=0; i < gameBoard.GetLength(0); i++) {
            int start = 0;
            GemType currColor = gameBoard[i,0];
            for (int j=0; j < gameBoard.GetLength(1); j++) {
                if (currColor != gameBoard[i,j]) {
                    int lenIn = j - start;
                    if (lenIn >= 3) {
                        for (int k=start; k < j; k++) {
                            pos.Add((i,k));
                        }
                    }
                    start = j;
                    currColor = gameBoard[i,j];
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
            GemType currColor = gameBoard[0,j];
            for (int i=0; i < gameBoard.GetLength(0); i++) {
                if (currColor != gameBoard[i,j]) {
                    int lenIn = i - start;
                    if (lenIn >= 3) {
                        for (int k=start; k < i; k++) {
                            pos.Add((k,j));
                        }
                    }
                    start = i;
                    currColor = gameBoard[i,j];
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
    private static GemType RandomGem(){
    return (GemType)Random.Shared.Next(GemColorsCount);
    }
    private static string CompactSymbols(GemType gem){ 
        return gem switch{
            GemType.Blue => "B",
            GemType.Green => "G",
            GemType.Red => "R",
            GemType.Yellow => "Y",
            GemType.Empty => ".",
            _ => ".!."
        };
    }
}