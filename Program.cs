GemType[,] testData = new GemType[,] {
    { GemType.Green, GemType.Empty, GemType.Green, GemType.Green, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Green, GemType.Green, GemType.Red },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
    { GemType.Red, GemType.Green, GemType.Red, GemType.Red, GemType.Green },
};

Board testBoard = new Board(testData);
testBoard.Print();

var matches = testBoard.FindAllMatches();
foreach (var (r,c) in matches) {
    Console.WriteLine($"{r},{c}");
}

testBoard.RemoveMatches(matches);
testBoard.Print();
