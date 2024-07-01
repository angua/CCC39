using CCC39Lib;
using CCC39UI;

var level = 5;

//WriteOutputs(level);

DoPerformanceTest();

void DoPerformanceTest()
{
    var solver = new Solver();

    Console.WriteLine("Lawn set 1");

    var level = 4;
    var inputFileNumber = 1;

    var inputfilename = $"../../../../Files/level{level}_{inputFileNumber}.in";
    var lines = File.ReadAllLines(inputfilename).ToList();

    var timing = solver.PerformanceTest(lines);

    foreach (var time in timing)
    {
        Console.WriteLine(time);
    }
    Console.WriteLine($"Total:");
    Console.WriteLine(timing.Sum());

    inputFileNumber = 5;

    Console.WriteLine($"slow lawn");
    inputfilename = $"../../../../Files/level{level}_{inputFileNumber}.in";
    lines = File.ReadAllLines(inputfilename).ToList();

    var set = new LawnSet(level, lines);
    var slowLawn = set.Lawns[1];

    var slowTiming = solver.PerformanceTest(slowLawn);
    Console.WriteLine(slowTiming);



}

void WriteOutputs(int level)
{
    var solver = new Solver();

    for (var inputFileNumber = 1; inputFileNumber <= 5; inputFileNumber++)
    {
        Console.WriteLine($"File number {inputFileNumber}");
        var inputfilename = $"../../../../Files/level{level}_{inputFileNumber}.in";
        var lines = File.ReadAllLines(inputfilename).ToList();

        var outputfilename = $"../../../../Files/level{level}_{inputFileNumber}.out";
        using var outputWriter = new StreamWriter(outputfilename);

        var output = solver.Solve(level, lines);

        outputWriter.Write(output);
    }
}
