public class Board {
    const int Rows = 8;
    const int Cols = 8;
    readonly GemType[,] grid = new GemType[Rows, Cols];

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
                Console.Write(grid[i,j] + " ");
            }
            Console.WriteLine();
         }
    }
}
