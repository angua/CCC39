using CCC39Lib;

var level = 4;

WriteOutputs(level);
    
void WriteOutputs(int level)
{
    var solver = new Solver();

    for (var inputFileNumber = 1; inputFileNumber <= 5; inputFileNumber++)
    {
        var inputfilename = $"../../../../Files/level{level}_{inputFileNumber}.in";
        var lines = File.ReadAllLines(inputfilename).ToList();

        var outputfilename = $"../../../../Files/level{level}_{inputFileNumber}.out";
        using var outputWriter = new StreamWriter(outputfilename);

        var output = solver.Solve(level, lines);

        outputWriter.Write(output);
    }
}
