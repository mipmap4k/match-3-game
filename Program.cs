GemType[,] testData = new GemType[,] {
    { GemType.Green, GemType.Red, GemType.Green, GemType.Green, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Green, GemType.Green, GemType.Red },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
};

Board testBoard = new Board(testData);
testBoard.Print();

var matchesHorizontal = testBoard.FindHorizontal();
foreach (var (r,c) in matchesHorizontal) {
    Console.WriteLine($"{r},{c}");
}

var matchesVertical = testBoard.FindVertical();
foreach (var (col,row) in matchesVertical) {
    Console.WriteLine($"{col},{row}");
}