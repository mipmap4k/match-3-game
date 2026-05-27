public class Board {
    const int Rows = 8;
    const int Cols = 8;
    readonly GemType[,] grid = new GemType[Rows, Cols];
    private static string GetSymbol(GemType gem) {
        return gem switch
        {
          GemType.Blue => "B",
          GemType.Green => "G",
          GemType.Red => "R",
          GemType.Yellow => "Y",
          _ => ".!."
        };
    }
    public Board() {
         for (int i=0; i < grid.GetLength(0); i++) {
            for (int j=0; j < grid.GetLength(1); j++) {
                grid[i,j] = (GemType)Random.Shared.Next(Enum.GetValues<GemType>().Length);
            }
         }
    }
    public void Print() {
            for (int i=0; i < grid.GetLength(0); i++) {
            for (int j=0; j < grid.GetLength(1); j++) {
                Console.Write(GetSymbol(grid[i,j]) + " ");
            }
            Console.WriteLine();
         }
    }
}
