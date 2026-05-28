public class Board {
    public Board() {
        for (int i=0; i < grid.GetLength(0); i++) {
            for (int j=0; j < grid.GetLength(1); j++) {
                grid[i,j] = RandomGem();
            }
        }
    }
    public Board(GemType[,] initGrid) {
        grid = initGrid;
    }
    private static GemType RandomGem(){
    return (GemType)Random.Shared.Next(Enum.GetValues<GemType>().Length);
    }
    private string CompactSymbols(GemType gem){ 
        return gem switch{
            GemType.Blue => "B",
            GemType.Green => "G",
            GemType.Red => "R",
            GemType.Yellow => "Y",
            _ => ".!."
        };
    }
       public HashSet<(int, int)> FindAllMatches() {
        var all = new HashSet<(int,int)>(FindHorizontal());
        all.UnionWith(FindVertical());
        return all;
    }
    public List<(int, int)> FindHorizontal() {
        List<(int, int)> pos = [];
        for (int i=0; i < grid.GetLength(0); i++) {
            int start = 0;
            GemType currColor = grid[i,0];
            for (int j=0; j < grid.GetLength(1); j++) {
                if (currColor != grid[i,j]) {
                    int lenIn = j - start;
                    if (lenIn >= 3) {
                        for (int k=start; k < j; k++) {
                            pos.Add((i,k));
                        }
                    }
                    start = j;
                    currColor = grid[i,j];
                }
            }
            int lenOut = grid.GetLength(1) - start;
            if (lenOut >= 3) {
                for (int k=start; k < grid.GetLength(1); k++) {
                    pos.Add((i,k));
                }
            }
        }
        return pos;
    }

    public List<(int, int)> FindVertical() {
        List<(int,int)> pos = [];
        for (int j=0; j < grid.GetLength(1); j++) {
            int start = 0;
            GemType currColor = grid[0,j];
            for (int i=0; i < grid.GetLength(0); i++) {
                if (currColor != grid[i,j]) {
                    int lenIn = i - start;
                    if (lenIn >= 3) {
                        for (int k=start; k < i; k++) {
                            pos.Add((k,j));
                        }
                    }
                    start = i;
                    currColor = grid[i,j];
                }
            }
            int lenOut = grid.GetLength(0) - start;
            if (lenOut >= 3) {
                for (int k=start; k < grid.GetLength(0); k++) {
                    pos.Add((k,j));
                }
            }
        }
        return pos;
    }
    public void Print() {
        for (int i=0; i < grid.GetLength(0); i++) {
            for (int j=0; j < grid.GetLength(1); j++) {
                Console.Write(CompactSymbols(grid[i,j]) + " ");
            }
            Console.WriteLine();
        }
    }
    private const int Row = 5;
    private const int Col = 5;
    private GemType[,] grid = new GemType[Col,Row];

}