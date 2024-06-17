using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
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
    private int _gridPositionSize = 39;

    // tree of levels and files
    public ObservableCollection<ScenarioNode> LawnCollection { get; set; } = new();

    // lawns in one file
    public LawnSet CurrentLawnSet
    {
        get => GetValue<LawnSet>();
        set
        {
            SetValue(value);
            _currentLawnIndex = 0;
            if (value != null)
            {
                CurrentLawn = value.Lawns[_currentLawnIndex];
            }
        }
    }

    private BitmapGridDrawing? _lawnBitmap;
    private BitmapGridDrawing? _inputBitmap;
    public Image LawnImage
    {
        get => GetValue<Image>();
        set => SetValue(value);
    }

    // lawn in file
    private int _currentLawnIndex = 0;
    public Lawn CurrentLawn
    {
        get => GetValue<Lawn>();
        set
        {
            SetValue(value);
            if (CurrentLawnSet != null && value != null)
            {
                DrawLawn(value);
            }
        }
    }


    public string CurrentOutput
    {
        get => GetValue<string>();
        set => SetValue(value);
    }


    public MainViewModel()
    {
        ParseLawns();

        PreviousInput = new RelayCommand(CanPreviousInput, DoPreviousInput);
        NextInput = new RelayCommand(CanNextInput, DoNextInput);
    }

    public RelayCommand PreviousInput { get; }
    public bool CanPreviousInput()
    {
        return CurrentLawnSet != null && _currentLawnIndex > 0;
    }
    public void DoPreviousInput()
    {
        CurrentLawn = CurrentLawnSet.Lawns[--_currentLawnIndex];
    }

    public RelayCommand NextInput { get; }
    public bool CanNextInput()
    {
        return CurrentLawnSet != null && _currentLawnIndex < CurrentLawnSet.Lawns.Count - 1;
    }
    public void DoNextInput()
    {
        CurrentLawn = CurrentLawnSet.Lawns[++_currentLawnIndex];
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
            _lawnBitmap.FillGridCell((int)treePos.X, (int)treePos.Y, Color.FromRgb(0, 80, 20));
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

}
