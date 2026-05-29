public class Board {
    private const int Row = 5;
    private const int Col = 5;
    private GemType[,] grid = new GemType[Col,Row];

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
    return (GemType)Random.Shared.Next(Enum.GetValues<GemType>().Length - 1);
    }
    private string CompactSymbols(GemType gem){ 
        return gem switch{
            GemType.Blue => "B",
            GemType.Green => "G",
            GemType.Red => "R",
            GemType.Yellow => "Y",
            GemType.Empty => ".",
            _ => ".!."
        };
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
    public void SpawnNewGem() {
        for (int i=0; i < grid.GetLength(0); i++) {
            for (int j=0; j < grid.GetLength(1); j++) {
                if (grid[i,j] == GemType.Empty) {
                    grid[i,j] = RandomGem();
                }
            }
        }
    }
    public void ApplyGravity() {
        for (int j=0; j < grid.GetLength(1); j++) {
            List<GemType> gems = [];
            for (int i=0; i < grid.GetLength(0); i++) {
                if (grid[i,j] != GemType.Empty) {
                    gems.Add(grid[i,j]);
                }
            }
            for (int i=0; i < grid.GetLength(0); i++) {
                grid[i,j] = GemType.Empty;
            }
            for (int i = grid.GetLength(0)- 1; i >= 0 && gems.Count > 0; i--) {
                grid[i,j] = gems[^1];
                gems.RemoveAt(gems.Count - 1);      
            }
        }
    }

    public void RemoveMatches(HashSet<(int, int)> matches) {
        foreach (var (r,c) in matches) {
            grid[r,c] = GemType.Empty;
        }
    }
    public HashSet<(int, int)> FindAllMatches() {
        var allMatches = new HashSet<(int,int)>(FindHorizontal());
        allMatches.UnionWith(FindVertical());
        return allMatches;
    } 
    private List<(int, int)> FindHorizontal() {
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

    private List<(int, int)> FindVertical() {
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
}