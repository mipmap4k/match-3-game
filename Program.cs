GemType[,] testData = new GemType[,] {
    { GemType.Green, GemType.Red, GemType.Green, GemType.Green, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Green, GemType.Green, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
};

Board testBoard = new Board(testData);
testBoard.Print();