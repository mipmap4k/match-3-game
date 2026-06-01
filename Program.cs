GemType[,] testData = new GemType[,] {
    { GemType.Green, GemType.Green, GemType.Empty, GemType.Green, GemType.Green },
    { GemType.Yellow, GemType.Green, GemType.Green, GemType.Green, GemType.Red },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
};

Board testBoard = new Board(testData);
while (true) {
    testBoard.Print();
    Console.WriteLine();
    string? input = Console.ReadLine();
    if (input == "q" || input == null) {
        break;
    }
    string[] cords = input.Split(' ');
    int startRow = int.Parse(cords[0]);
    int startCol = int.Parse(cords[1]);
    int endRow = int.Parse(cords[2]);
    int endCol = int.Parse(cords[3]);
    if (testBoard.TryMakeMove(startRow, startCol, endRow, endCol)) {
        testBoard.CycleTik();
        } else {
            Console.WriteLine("ZOMBI virus");
        }
    }