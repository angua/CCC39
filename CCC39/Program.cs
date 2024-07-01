﻿using CCC39Lib;

var level = 5;

//WriteOutputs(level);

DoPerformanceTest();

void DoPerformanceTest()
{
    var solver = new Solver();
    var level = 4;
    var inputFileNumber = 1;

    var inputfilename = $"../../../../Files/level{level}_{inputFileNumber}.in";
    var lines = File.ReadAllLines(inputfilename).ToList();

    var timing = solver.PerformanceTest(lines);

    foreach (var time in timing)
    {
        Console.WriteLine(time);
    }
    Console.WriteLine($"Total time: {timing.Sum()}");

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
