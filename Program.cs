GemType[,] testData = new GemType[,] {
    { GemType.Green, GemType.Empty, GemType.Green, GemType.Green, GemType.Green },
    { GemType.Yellow, GemType.Green, GemType.Green, GemType.Green, GemType.Red },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
};

Board testBoard = new Board(testData);
testBoard.Print();
Console.WriteLine();
testBoard.Update();
testBoard.Print();