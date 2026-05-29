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
    int r1 = int.Parse(cords[0]);
    int c1 = int.Parse(cords[1]);
    int r2 = int.Parse(cords[2]);
    int c2 = int.Parse(cords[3]);
    if (testBoard.Swap(r1, c1, r2, c2)) {
        testBoard.Update();
    } else {
      Console.WriteLine("НЕПРАВИЛЬНЫЕ корды");  
    }
}
// testBoard.Print();
// Console.WriteLine();
// testBoard.Swap(0,2,0,3);
// testBoard.Print();
// Console.WriteLine();
// testBoard.Update();
// testBoard.Print();