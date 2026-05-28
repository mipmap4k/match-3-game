GemType[,] testData = new GemType[,] {
    { GemType.Green, GemType.Red, GemType.Green, GemType.Green, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Green, GemType.Green, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
};

Board testBoard = new Board(testData);
testBoard.Print();
var matches = testBoard.FindHorizontal();
foreach (var (r,c) in matches) {
    Console.WriteLine($"{r},{c}");
}