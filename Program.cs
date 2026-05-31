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
    if (testBoard.Swap(startRow, startCol, endRow, endCol)) {
        if (testBoard.HasMatches()) {
            testBoard.Update();
        } else {
            testBoard.Print();
            Console.WriteLine("ZOMBI virus");
             testBoard.Swap(startRow, startCol, endRow, endCol);
        }
    }
    testBoard.Update();
}
// testBoard.Print();
// Console.WriteLine();
// testBoard.Swap(0,2,0,3);
// testBoard.Print();
// Console.WriteLine();
// testBoard.Update();
// testBoard.Print();