public class Board {
    public Board() {
        for (int i=0; i < grid.GetLength(0); i++) {
            for (int j=0; j < grid.GetLength(1); j++) {
                grid[i,j] = RandomGem();
            }
        }
    }
    private GemType RandomGem(){
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

    public void Print() {
        for (int i=0; i < grid.GetLength(0); i++) {
            for (int j=0; j < grid.GetLength(1); j++) {
                Console.Write(CompactSymbols(grid[i,j]));
            }
            Console.WriteLine();
        }
    }
    private static int Row = 8;
    private static int Col = 8;
    private GemType[,] grid = new GemType[Col,Row];

}