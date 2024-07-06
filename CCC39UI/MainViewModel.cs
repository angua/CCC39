using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CCC39Lib;
using CommonWPF;

namespace CCC39UI;

class MainViewModel : ViewModelBase
{
    // pixel size of a grid position on the map
    private int _gridPositionSize = 19;

    private Solver _solver = new();

    // tree of levels and files
    public ObservableCollection<ScenarioNode> LawnCollection { get; set; } = new();

    // lawns in one file
    public LawnSet CurrentLawnSet
    {
        get => GetValue<LawnSet>();
        set
        {
            SetValue(value);
            CurrentLawnIndex = 0;
            if (value != null && value.NumLawns > 0)
            {
                CurrentLawn = value.Lawns[CurrentLawnIndex];
            }
        }
    }


    // lawn in file
    public int CurrentLawnIndex
    {
        get => GetValue<int>();
        set => SetValue(value);
    }
    public Lawn CurrentLawn
    {
        get => GetValue<Lawn>();
        set
        {
            SetValue(value);
            if (CurrentLawnSet != null && value != null)
            {
                DrawLawn(value);
                Instructions = CurrentLawn.InstructionString;
            }
            StepCount = 0;
            Timing = 0;
        }
    }

    private BitmapGridDrawing? _lawnBitmap;
    private BitmapGridDrawing? _inputBitmap;
    public Image LawnImage
    {
        get => GetValue<Image>();
        set => SetValue(value);
    }

    public string LastStepValid
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public int StepCount
    {
        get => GetValue<int>();
        set => SetValue(value);
    }



    public int CurrentPathIndex
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    public PathStep? CurrentLastPathstep
    {
        get => GetValue<PathStep>();
        set
        {
            SetValue(value);
            if (value != null)
            {
                CurrentLawn.SetStepsfromLast(value);
                _solver.CreatePathfromSteps(CurrentLawn);

                LastStepValid = CurrentLawn.CorrectPathSteps.Last().IsValid.ToString();

                DrawLawn(CurrentLawn);
            }
        }
    }


    public int AllPathCount
    {
        get => GetValue<int>();
        set => SetValue(value);
    }


    public string Instructions
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public long Timing
    {
        get => GetValue<long>();
        set => SetValue(value);
    }

    public MainViewModel()
    {
        CurrentLawnIndex = 0;
        Timing = 0;
        ParseLawns();

        PreviousInput = new RelayCommand(CanPreviousInput, DoPreviousInput);
        NextInput = new RelayCommand(CanNextInput, DoNextInput);

        FindPath = new RelayCommand(CanFindPath, DoFindPath);

        FindPathNextStep = new RelayCommand(CanFindPathNextStep, DoFindPathNextStep);

        PreviousPath = new RelayCommand(CanPreviousPath, DoPreviousPath);
        NextPath = new RelayCommand(CanNextPath, DoNextPath);

        ClearPath = new RelayCommand(CanClearPath, DoClearPath);
    }

    public RelayCommand PreviousInput { get; }
    public bool CanPreviousInput()
    {
        return CurrentLawnSet != null && CurrentLawnIndex > 0;
    }
    public void DoPreviousInput()
    {
        CurrentLawn = CurrentLawnSet.Lawns[--CurrentLawnIndex];
    }

    public RelayCommand NextInput { get; }
    public bool CanNextInput()
    {
        return CurrentLawnSet != null && CurrentLawnIndex < CurrentLawnSet.Lawns.Count - 1;
    }
    public void DoNextInput()
    {
        CurrentLawn = CurrentLawnSet.Lawns[++CurrentLawnIndex];
    }


    public RelayCommand PreviousPath { get; }
    public bool CanPreviousPath()
    {
        return CurrentLawn != null && CurrentPathIndex > 0;
    }
    public void DoPreviousPath()
    {
        CurrentLastPathstep = CurrentLawn.AllLastSteps[--CurrentPathIndex];
    }

    public RelayCommand NextPath { get; }
    public bool CanNextPath()
    {
        return CurrentLawn != null && CurrentPathIndex < CurrentLawn.AllLastSteps.Count - 1;
    }
    public void DoNextPath()
    {
        CurrentLastPathstep = CurrentLawn.AllLastSteps[++CurrentPathIndex];
    }



    public RelayCommand FindPath { get; }
    public bool CanFindPath()
    {
        return CurrentLawnSet != null;
    }
    public void DoFindPath()
    {
        var useCycles = false;
        if (CurrentLawnSet.Level == 6 || CurrentLawnSet.Level == 7)
        {
            useCycles = true;
        }

        _solver.FindPath(CurrentLawn, useCycles);
        _solver.CreatePathfromSteps(CurrentLawn);
        LastStepValid = CurrentLawn.CorrectPathSteps.Last().IsValid.ToString();
        StepCount = CurrentLawn.PathStepsCount;
        Timing = _solver.Timing;

        DrawLawn(CurrentLawn);
        Instructions = CurrentLawn.InstructionString;

    }


    public RelayCommand FindPathNextStep { get; }
    public bool CanFindPathNextStep()
    {
        return true;
    }
    public void DoFindPathNextStep()
    {
        var useCycles = false;
        if (CurrentLawnSet.Level == 6 || CurrentLawnSet.Level == 7)
        {
            useCycles = true;
        }

        _solver.FindPathNextStep(CurrentLawn, useCycles);
        _solver.CreateAllPaths(CurrentLawn);

        AllPathCount = CurrentLawn.AllLastSteps.Count;

        CurrentPathIndex = 0;
        CurrentLastPathstep = CurrentLawn.AllLastSteps[CurrentPathIndex];

        StepCount = CurrentLawn.PathStepsCount;
        Timing = _solver.Timing;

        _solver.CreatePathfromSteps(CurrentLawn);
        LastStepValid = CurrentLawn.CorrectPathSteps.Last().IsValid.ToString();
        StepCount = CurrentLawn.PathStepsCount;

        DrawLawn(CurrentLawn);
    }

    public RelayCommand ClearPath { get; }
    public bool CanClearPath()
    {
        return true;
    }
    public void DoClearPath()
    {
        CurrentLawn.ClearPath();
        StepCount = 0;
        Timing = 0;
        AllPathCount = 0;
        DrawLawn(CurrentLawn);
    }



    private void ParseLawns()
    {
        var inputPath = $"../../../../Files";
        var inputFiles = Directory.GetFiles(inputPath, "*.in");

        var regex = new Regex(@"level(\d)_(\d)");

        foreach (var file in inputFiles)
        {
            if (!file.Contains("example"))
            {
                // level2_5.in
                var fileName = Path.GetFileNameWithoutExtension(file);
                var match = regex.Match(fileName);
                var level = match.Groups[1].Value;
                var fileNumber = match.Groups[2].Value;

                var levelName = $"Level {level}";
                var lawnSetName = $"Lawn Set {fileNumber}";

                var lawnSet = new LawnSet(int.Parse(level), int.Parse(fileNumber));

                var levelNode = LawnCollection.FirstOrDefault(n => n.Name == levelName);
                if (levelNode == null)
                {
                    levelNode = new ScenarioNode()
                    {
                        Name = levelName
                    };
                    LawnCollection.Add(levelNode);
                }

                levelNode.Children.Add(new ScenarioNode()
                {
                    Name = lawnSetName,
                    Scenario = lawnSet
                });

            }
        }
    }



    private void DrawLawn(Lawn lawn)
    {
        _lawnBitmap = new BitmapGridDrawing(lawn.Width, lawn.Height, _gridPositionSize, 1);
        _lawnBitmap.BackgroundColor = Color.FromRgb(0, 180, 0);
        _lawnBitmap.GridLineColor = Color.FromRgb(100, 255, 100);

        _lawnBitmap.DrawBackGround();

        foreach (var treePos in lawn.TreePositions)
        {
            _lawnBitmap.FillGridCell((int)treePos.X, (int)treePos.Y, Color.FromRgb(0, 60, 15));
        }

        // fill pathing rectangles with different colors
        for (int s = 0; s < lawn.CorrectPathSteps.Count; s++)
        {
            var step = lawn.CorrectPathSteps[s];
            var rectangleColor = GetRectangleColor(s);

            foreach (var pos in step.Path)
            {
                _lawnBitmap.FillGridCell((int)pos.X, (int)pos.Y, rectangleColor);
            }

        }


        if (lawn.Path.Count > 0)
        {
            var startPos = lawn.Path.First();
            _lawnBitmap.DrawXInGridcell((int)startPos.X, (int)startPos.Y, _gridPositionSize, Color.FromRgb(255, 0, 0));
            _lawnBitmap.DrawConnectedLines(lawn.Path, Color.FromRgb(255, 255, 0));

            var endPos = lawn.Path.Last();
            _lawnBitmap.DrawXInGridcell((int)endPos.X, (int)endPos.Y, _gridPositionSize / 2, Color.FromRgb(255, 255, 0));
        }


        LawnImage = new Image();

        LawnImage.Stretch = Stretch.None;
        LawnImage.Margin = new Thickness(0);

        LawnImage.Source = _lawnBitmap.Picture;
        RaisePropertyChanged(nameof(LawnImage));
    }

    private Color GetRectangleColor(int s)
    {
        var div = s % 5;

        return div switch
        {
            0 => Color.FromRgb(0, 12, 170),
            1 => Color.FromRgb(45, 19, 241),
            2 => Color.FromRgb(0, 210, 255),
            3 => Color.FromRgb(0, 240, 220),
            4 => Color.FromRgb(0, 133, 119),
        };


    }
}
