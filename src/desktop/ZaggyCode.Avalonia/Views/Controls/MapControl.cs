using Avalonia.VisualTree;
using ZaggyCode.Avalonia.Views.Records;
using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Avalonia.Views.Controls;

// Игровое поле только рисует данные карты и анимации. Логическое состояние робота
// живёт в исполнителе, сюда оно приходит через IRobotGameMapProxy.
public sealed class MapControl : Control
{
    private static readonly IBrush FlashBrush = new SolidColorBrush(Color.FromArgb(200, 0xFF, 0xFF, 0xFF));

    private const double MinCellSize = 6;
    private const double MaxCellSize = 80;
    private const double FallbackCellSize = 40;
    private const double DefaultMoveStepSeconds = 0.22;
    private const double DefaultSadSeconds = 0.4;
    private const double BumpFactor = 0.3;
    private const double FlashPeakOpacity = 0.8;
    private const double DefaultDrawnOpacity = 0.5;
    private const double RobotScale = 0.7;
    private const int BumpDurationMilliseconds = 200;
    private const int FlashDurationMilliseconds = 200;

    private readonly Image _robotImage = new() { Stretch = Stretch.Uniform, IsVisible = false };
    private readonly Rectangle _flashOverlay = new() { Fill = FlashBrush, IsVisible = false };
    private readonly Dictionary<(int X, int Y), GamePoint> _points = [];
    private readonly HashSet<(int X, int Y)> _drawnPoints = [];
    private readonly HashSet<(int X, int Y)> _hiddenCoins = [];
    private readonly Dictionary<PointControl, Image> _coinImages = [];

    private PointControl[,] _cells = new PointControl[0, 0];
    private RobotMap? _map;
    private RobotDirection _robotDirection = RobotDirection.Down;

    private IImage? _front;
    private IImage? _back;
    private IImage? _left;
    private IImage? _right;
    private IImage? _frontLaggy;
    private IImage? _backLaggy;
    private IImage? _leftLaggy;
    private IImage? _rightLaggy;
    private IImage? _sad;

    private CancellationTokenSource? _moveCancellation;
    private CancellationTokenSource? _flashCancellation;
    private bool _isDead;
    private bool _isLaggy;

    private RenderPosition _renderPosition;
    private Cell _flashCell;
    private Cell _animationTarget;

    public RobotMap? Map
    {
        get => GetValue(MapProperty);
        set => SetValue(MapProperty, value);
    }

    public string? CoinPath
    {
        get => GetValue(CoinPathProperty);
        set => SetValue(CoinPathProperty, value);
    }

    public string? FrontPath
    {
        get => GetValue(FrontPathProperty);
        set => SetValue(FrontPathProperty, value);
    }

    public string? BackPath
    {
        get => GetValue(BackPathProperty);
        set => SetValue(BackPathProperty, value);
    }

    public string? LeftPath
    {
        get => GetValue(LeftPathProperty);
        set => SetValue(LeftPathProperty, value);
    }

    public string? RightPath
    {
        get => GetValue(RightPathProperty);
        set => SetValue(RightPathProperty, value);
    }

    public string? FrontLaggyPath
    {
        get => GetValue(FrontLaggyPathProperty);
        set => SetValue(FrontLaggyPathProperty, value);
    }

    public string? BackLaggyPath
    {
        get => GetValue(BackLaggyPathProperty);
        set => SetValue(BackLaggyPathProperty, value);
    }

    public string? LeftLaggyPath
    {
        get => GetValue(LeftLaggyPathProperty);
        set => SetValue(LeftLaggyPathProperty, value);
    }

    public string? RightLaggyPath
    {
        get => GetValue(RightLaggyPathProperty);
        set => SetValue(RightLaggyPathProperty, value);
    }

    public string? SadPath
    {
        get => GetValue(SadPathProperty);
        set => SetValue(SadPathProperty, value);
    }

    public double MoveStepSeconds
    {
        get => GetValue(MoveStepSecondsProperty);
        set => SetValue(MoveStepSecondsProperty, value);
    }

    public double SadSeconds
    {
        get => GetValue(SadSecondsProperty);
        set => SetValue(SadSecondsProperty, value);
    }

    public double FixedCellSize
    {
        get => GetValue(FixedCellSizeProperty);
        set => SetValue(FixedCellSizeProperty, value);
    }

    public Thickness MarginUnits
    {
        get => GetValue(MarginUnitsProperty);
        set => SetValue(MarginUnitsProperty, value);
    }

    public IBrush? SeaBrush
    {
        get => GetValue(SeaBrushProperty);
        set => SetValue(SeaBrushProperty, value);
    }

    public IBrush? PaintedBrush
    {
        get => GetValue(PaintedBrushProperty);
        set => SetValue(PaintedBrushProperty, value);
    }

    public static readonly StyledProperty<RobotMap?> MapProperty =
        AvaloniaProperty.Register<MapControl, RobotMap?>(nameof(Map));

    public static readonly StyledProperty<string?> CoinPathProperty =
        AvaloniaProperty.Register<MapControl, string?>(nameof(CoinPath));

    public static readonly StyledProperty<string?> FrontPathProperty =
        AvaloniaProperty.Register<MapControl, string?>(nameof(FrontPath));

    public static readonly StyledProperty<string?> BackPathProperty =
        AvaloniaProperty.Register<MapControl, string?>(nameof(BackPath));

    public static readonly StyledProperty<string?> LeftPathProperty =
        AvaloniaProperty.Register<MapControl, string?>(nameof(LeftPath));

    public static readonly StyledProperty<string?> RightPathProperty =
        AvaloniaProperty.Register<MapControl, string?>(nameof(RightPath));

    public static readonly StyledProperty<string?> FrontLaggyPathProperty =
        AvaloniaProperty.Register<MapControl, string?>(nameof(FrontLaggyPath));

    public static readonly StyledProperty<string?> BackLaggyPathProperty =
        AvaloniaProperty.Register<MapControl, string?>(nameof(BackLaggyPath));

    public static readonly StyledProperty<string?> LeftLaggyPathProperty =
        AvaloniaProperty.Register<MapControl, string?>(nameof(LeftLaggyPath));

    public static readonly StyledProperty<string?> RightLaggyPathProperty =
        AvaloniaProperty.Register<MapControl, string?>(nameof(RightLaggyPath));

    public static readonly StyledProperty<string?> SadPathProperty =
        AvaloniaProperty.Register<MapControl, string?>(nameof(SadPath));

    public static readonly StyledProperty<double> MoveStepSecondsProperty =
        AvaloniaProperty.Register<MapControl, double>(nameof(MoveStepSeconds), DefaultMoveStepSeconds);

    public static readonly StyledProperty<double> SadSecondsProperty =
        AvaloniaProperty.Register<MapControl, double>(nameof(SadSeconds), DefaultSadSeconds);

    public static readonly StyledProperty<double> FixedCellSizeProperty =
        AvaloniaProperty.Register<MapControl, double>(nameof(FixedCellSize), 0.0);

    public static readonly StyledProperty<Thickness> MarginUnitsProperty =
        AvaloniaProperty.Register<MapControl, Thickness>(nameof(MarginUnits), new Thickness(0));

    public static readonly StyledProperty<IBrush?> SeaBrushProperty =
        AvaloniaProperty.Register<MapControl, IBrush?>(nameof(SeaBrush));

    public static readonly StyledProperty<IBrush?> PaintedBrushProperty =
        AvaloniaProperty.Register<MapControl, IBrush?>(nameof(PaintedBrush));

    public MapControl()
    {
        Focusable = true;

        VisualChildren.Add(_robotImage);
        VisualChildren.Add(_flashOverlay);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MapProperty)
        {
            SetMap(change.NewValue as RobotMap);
            RebuildCells();
            Reset();
            InvalidateMeasure();
            InvalidateVisual();
        }
        else if (change.Property == FrontPathProperty
            || change.Property == BackPathProperty
            || change.Property == LeftPathProperty
            || change.Property == RightPathProperty
            || change.Property == FrontLaggyPathProperty
            || change.Property == BackLaggyPathProperty
            || change.Property == LeftLaggyPathProperty
            || change.Property == RightLaggyPathProperty
            || change.Property == SadPathProperty)
        {
            ReloadSprites();
        }
        else if (change.Property == FixedCellSizeProperty
            || change.Property == MarginUnitsProperty)
        {
            InvalidateMeasure();
            InvalidateVisual();
        }
        else if (change.Property == SeaBrushProperty || change.Property == PaintedBrushProperty)
        {
            InvalidateVisual();
        }
    }

    private void SetMap(RobotMap? map)
    {
        _map = map;
        _points.Clear();

        if (map is null)
            return;

        foreach (var point in map.Points)
            _points[(point.X, point.Y)] = point;
    }

    private void RebuildCells()
    {
        foreach (var cell in _cells)
        {
            VisualChildren.Remove(cell);
            LogicalChildren.Remove(cell);
        }

        _coinImages.Clear();

        var columns = _map?.Width ?? 0;
        var rows = _map?.Height ?? 0;
        var cells = new PointControl[columns, rows];

        for (var x = 0; x < columns; x++)
        {
            for (var y = 0; y < rows; y++)
            {
                _points.TryGetValue((x, y), out var point);
                var cell = new PointControl { DataContext = point };
                MapAssetPaths.SetCoinPath(cell, CoinPath);
                cells[x, y] = cell;
                VisualChildren.Add(cell);
                LogicalChildren.Add(cell);
            }
        }

        _cells = cells;
    }

    private void ReloadSprites()
    {
        _front = LoadImage(FrontPath);
        _back = LoadImage(BackPath);
        _left = LoadImage(LeftPath);
        _right = LoadImage(RightPath);
        _frontLaggy = LoadImage(FrontLaggyPath);
        _backLaggy = LoadImage(BackLaggyPath);
        _leftLaggy = LoadImage(LeftLaggyPath);
        _rightLaggy = LoadImage(RightLaggyPath);
        _sad = LoadImage(SadPath);

        UpdateRobotSprite();
    }

    private static IImage? LoadImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        return Path.GetExtension(path).Equals(".svg", StringComparison.OrdinalIgnoreCase)
            ? new SvgImage { Source = SvgSource.Load(path) }
            : new Bitmap(path);
    }

    public void Reset()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(Reset);
            return;
        }

        _moveCancellation?.Cancel();
        _flashCancellation?.Cancel();

        _isDead = false;
        _isLaggy = false;
        _drawnPoints.Clear();
        _hiddenCoins.Clear();
        _robotDirection = RobotDirection.Down;

        var spawn = _points.Values.FirstOrDefault(point => point.IsSpawn);
        var column = spawn?.X ?? 0;
        var row = spawn?.Y ?? 0;

        _renderPosition = new RenderPosition(column, row);
        _animationTarget = new Cell(column, row);
        _robotImage.RenderTransform = null;
        _robotImage.IsVisible = true;

        RefreshCoinVisibility();
        UpdateRobotSprite();
        ArrangeRobot(Bounds.Size);
        InvalidateVisual();
    }

    public void SetRobotPosition(int column, int row, RobotDirection direction)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => SetRobotPosition(column, row, direction));
            return;
        }

        _robotDirection = direction;

        if (_isDead)
            return;

        StartMoveAnimation(column, row);
    }

    public void SetCellDrawn(int column, int row, bool drawn)
    {
        RunOnUiThread(() =>
        {
            if (drawn)
                _drawnPoints.Add((column, row));
            else
                _drawnPoints.Remove((column, row));
        });
    }

    public void SetCellCollected(int column, int row, bool collected)
    {
        RunOnUiThread(() =>
        {
            if (collected)
            {
                _hiddenCoins.Add((column, row));
                UpdateCoinVisibility(column, row);
            }
            else
            {
                _hiddenCoins.Remove((column, row));
                UpdateCoinVisibility(column, row);
            }
        });
    }

    public void FlashCell(int column, int row)
    {
        var layout = CalculateLayout(Bounds.Size);
        if (layout.CellSize <= 0)
            return;

        _flashCancellation?.Cancel();
        _flashCancellation = new CancellationTokenSource();

        _flashCell = new Cell(column, row);

        var offset = layout.Offset;
        var cell = layout.CellSize;
        var x = offset.X + column * cell;
        var y = offset.Y + row * cell;

        _flashOverlay.Width = cell;
        _flashOverlay.Height = cell;
        _flashOverlay.Opacity = FlashPeakOpacity;
        _flashOverlay.IsVisible = true;
        _flashOverlay.Arrange(new Rect(x, y, cell, cell));

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(FlashDurationMilliseconds),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame { Cue = new Cue(0.0), Setters = { new Setter(OpacityProperty, FlashPeakOpacity) } },
                new KeyFrame { Cue = new Cue(1.0), Setters = { new Setter(OpacityProperty, 0.0) } }
            }
        };

        _ = RunFlashAnimationAsync(animation, _flashCancellation.Token);
    }

    public void PlayCollisionAnimation(RobotDirection? attemptedDirection, int column, int row)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => PlayCollisionAnimation(attemptedDirection, column, row));
            return;
        }

        if (_isDead)
            return;

        _isDead = true;
        _moveCancellation?.Cancel();

        _robotImage.Source = _sad;
        _robotImage.IsVisible = true;

        _ = RunCollisionAnimationAsync(attemptedDirection, column, row);
    }

    private async Task RunCollisionAnimationAsync(RobotDirection? attemptedDirection, int column, int row)
    {
        var layout = CalculateLayout(Bounds.Size);
        var cell = layout.CellSize;
        var offset = layout.Offset;

        if (cell > 0 && attemptedDirection is not null)
        {
            var bump = attemptedDirection.Value switch
            {
                RobotDirection.Up => new CellOffset(0, -1),
                RobotDirection.Down => new CellOffset(0, 1),
                RobotDirection.Left => new CellOffset(-1, 0),
                RobotDirection.Right => new CellOffset(1, 0),
                _ => new CellOffset(0, 0)
            };

            var transform = new TranslateTransform();
            _robotImage.RenderTransform = transform;

            var bumpAnimation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(BumpDurationMilliseconds),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0.0),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 0.0),
                            new Setter(TranslateTransform.YProperty, 0.0)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(0.5),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, bump.ColumnDelta * cell * BumpFactor),
                            new Setter(TranslateTransform.YProperty, bump.RowDelta * cell * BumpFactor)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1.0),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 0.0),
                            new Setter(TranslateTransform.YProperty, 0.0)
                        }
                    }
                }
            };

            try
            {
                await bumpAnimation.RunAsync(_robotImage);
            }
            catch (OperationCanceledException)
            {
            }

            _robotImage.RenderTransform = null;
        }

        await Task.Delay(TimeSpan.FromSeconds(Math.Max(0, SadSeconds)));

        _isDead = false;
        _isLaggy = true;
        _renderPosition = new RenderPosition(column, row);
        _animationTarget = new Cell(column, row);
        ArrangeRobot(Bounds.Size);
        UpdateRobotSprite();
    }

    private void StartMoveAnimation(int toColumn, int toRow)
    {
        _moveCancellation?.Cancel();
        _moveCancellation = new CancellationTokenSource();

        var layout = CalculateLayout(Bounds.Size);
        if (layout.CellSize <= 0)
        {
            var target = new Cell(toColumn, toRow);
            _renderPosition = new RenderPosition(target.Column, target.Row);
            _animationTarget = target;
            ArrangeRobot(Bounds.Size);
            return;
        }

        _isLaggy = false;
        var offset = layout.Offset;
        var cell = layout.CellSize;

        _renderPosition = new RenderPosition(_animationTarget.Column, _animationTarget.Row);
        _animationTarget = new Cell(toColumn, toRow);
        // Несколько действий в одной строке приходят пачкой: ставим картинку
        // в начало нового перехода, иначе она прыгнет из прежней позиции.
        ArrangeRobot(Bounds.Size);

        var fromX = offset.X + _renderPosition.Column * cell;
        var fromY = offset.Y + _renderPosition.Row * cell;
        var toX = offset.X + toColumn * cell;
        var toY = offset.Y + toRow * cell;

        var transform = new TranslateTransform();
        _robotImage.RenderTransform = transform;

        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(Math.Max(0, MoveStepSeconds)),
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, 0.0),
                        new Setter(TranslateTransform.YProperty, 0.0)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, toX - fromX),
                        new Setter(TranslateTransform.YProperty, toY - fromY)
                    }
                }
            }
        };

        _ = RunMoveAnimationAsync(animation, toColumn, toRow, _moveCancellation.Token);
    }

    private async Task RunMoveAnimationAsync(Animation animation, int toColumn, int toRow, CancellationToken cancellationToken)
    {
        try
        {
            await animation.RunAsync(_robotImage, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var target = new Cell(toColumn, toRow);
                _renderPosition = new RenderPosition(target.Column, target.Row);
                _animationTarget = target;
                _robotImage.RenderTransform = null;
                ArrangeRobot(Bounds.Size);
                UpdateRobotSprite();
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var columns = _map?.Width ?? 0;
        var rows = _map?.Height ?? 0;
        if (columns == 0 || rows == 0)
            return default;

        var totalColumns = columns + MarginUnits.Left + MarginUnits.Right;
        var totalRows = rows + MarginUnits.Top + MarginUnits.Bottom;

        double cell;
        if (FixedCellSize > 0)
        {
            cell = FixedCellSize;
        }
        else
        {
            var widthPerCell = double.IsFinite(availableSize.Width) && availableSize.Width > 0
                ? availableSize.Width / totalColumns
                : FallbackCellSize;

            var heightPerCell = double.IsFinite(availableSize.Height) && availableSize.Height > 0
                ? availableSize.Height / totalRows
                : FallbackCellSize;

            cell = Math.Clamp(Math.Min(widthPerCell, heightPerCell), MinCellSize, MaxCellSize);
        }

        var childSize = new Size(cell, cell);
        _robotImage.Measure(childSize);
        _flashOverlay.Measure(childSize);

        foreach (var cellControl in _cells)
            cellControl.Measure(childSize);

        return new Size(totalColumns * cell, totalRows * cell);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        ArrangeCells(finalSize);
        ArrangeRobot(finalSize);
        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        var size = Bounds.Size;
        context.DrawRectangle(SeaBrush ?? Brush("MapBackgroundBrush"), null, new Rect(size));

        var columns = _map?.Width ?? 0;
        var rows = _map?.Height ?? 0;
        if (columns == 0 || rows == 0)
            return;

        var layout = CalculateLayout(size);
        if (layout.CellSize <= 0)
            return;

        var offset = layout.Offset;
        var cell = layout.CellSize;
        var defaultBrush = PaintedBrush ?? Brush("MapPaintedCellBrush");

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                if (!_drawnPoints.Contains((column, row)))
                    continue;

                _points.TryGetValue((column, row), out var point);
                var brush = BrushFromHex(point?.CustomCoreDrewPointHex) ?? defaultBrush;
                var opacity = point?.RequireDrawCustomOpacity ?? DefaultDrawnOpacity;
                var rect = new Rect(offset.X + column * cell, offset.Y + row * cell, cell, cell);

                using (context.PushOpacity(opacity))
                    context.DrawRectangle(brush, null, rect);
            }
        }
    }

    private static IBrush? BrushFromHex(string? hex)
        => string.IsNullOrWhiteSpace(hex) ? null : global::Avalonia.Media.Brush.Parse(hex);

    private static IBrush Brush(string key)
    {
        if (Application.Current is null)
            return Brushes.Transparent;

        return Application.Current.Resources.TryGetResource(key, null, out var resource) && resource is IBrush brush
            ? brush
            : Brushes.Transparent;
    }

    private void ArrangeCells(Size finalSize)
    {
        if (finalSize.Width <= 0 || finalSize.Height <= 0)
            return;

        var layout = CalculateLayout(finalSize);
        if (layout.CellSize <= 0)
            return;

        var offset = layout.Offset;
        var cell = layout.CellSize;

        for (var x = 0; x < _cells.GetLength(0); x++)
        {
            for (var y = 0; y < _cells.GetLength(1); y++)
                _cells[x, y].Arrange(new Rect(offset.X + x * cell, offset.Y + y * cell, cell, cell));
        }
    }

    private void ArrangeRobot(Size finalSize)
    {
        if (finalSize.Width <= 0 || finalSize.Height <= 0)
            return;

        var layout = CalculateLayout(finalSize);
        if (layout.CellSize <= 0)
            return;

        var offset = layout.Offset;
        var cell = layout.CellSize;

        var robotCell = cell * RobotScale;
        var robotInset = (cell - robotCell) / 2;

        _robotImage.Arrange(new Rect(
            offset.X + _renderPosition.Column * cell + robotInset,
            offset.Y + _renderPosition.Row * cell + robotInset,
            robotCell,
            robotCell));

        if (_flashOverlay.IsVisible)
        {
            _flashOverlay.Arrange(new Rect(
                offset.X + _flashCell.Column * cell,
                offset.Y + _flashCell.Row * cell,
                cell,
                cell));
        }
    }

    private MapLayout CalculateLayout(Size size)
    {
        var columns = _map?.Width ?? 0;
        var rows = _map?.Height ?? 0;
        if (columns == 0 || rows == 0)
            return new MapLayout(new PixelOffset(0, 0), 0);

        var totalColumns = columns + MarginUnits.Left + MarginUnits.Right;
        var totalRows = rows + MarginUnits.Top + MarginUnits.Bottom;

        var cell = FixedCellSize > 0
            ? FixedCellSize
            : Math.Clamp(Math.Min(size.Width / totalColumns, size.Height / totalRows), MinCellSize, MaxCellSize);

        var contentWidth = cell * totalColumns;
        var contentHeight = cell * totalRows;

        var offsetX = (size.Width - contentWidth) / 2.0 + cell * MarginUnits.Left;
        var offsetY = (size.Height - contentHeight) / 2.0 + cell * MarginUnits.Top;

        return new MapLayout(new PixelOffset(offsetX, offsetY), cell);
    }

    private void UpdateRobotSprite()
    {
        if (_isDead)
            return;

        _robotImage.Source = _isLaggy
            ? LaggyFor(_robotDirection)
            : SpriteFor(_robotDirection);

        _robotImage.IsVisible = true;
    }

    private IImage? SpriteFor(RobotDirection robotDirection) => robotDirection switch
    {
        RobotDirection.Up => _back,
        RobotDirection.Down => _front,
        RobotDirection.Left => _left,
        RobotDirection.Right => _right,
        _ => _front
    };

    private IImage? LaggyFor(RobotDirection robotDirection) => robotDirection switch
    {
        RobotDirection.Up => _backLaggy,
        RobotDirection.Down => _frontLaggy,
        RobotDirection.Left => _leftLaggy,
        RobotDirection.Right => _rightLaggy,
        _ => _frontLaggy
    };

    private void UpdateCoinVisibility(int column, int row)
    {
        if (column < 0 || row < 0 || column >= _cells.GetLength(0) || row >= _cells.GetLength(1))
            return;

        var cell = _cells[column, row];
        _points.TryGetValue((column, row), out var point);
        var coinImage = GetCoinImage(cell);

        if (coinImage is not null)
            coinImage.IsVisible = point?.HasCoin == true && !_hiddenCoins.Contains((column, row));
    }

    // В PointControl единственный Image — это монета, отдельного имени у него нет.
    private Image? GetCoinImage(PointControl cell)
    {
        if (_coinImages.TryGetValue(cell, out var image))
            return image;

        image = cell.GetVisualDescendants().OfType<Image>().FirstOrDefault();
        if (image is not null)
            _coinImages[cell] = image;

        return image;
    }

    private void RefreshCoinVisibility()
    {
        for (var column = 0; column < _cells.GetLength(0); column++)
        {
            for (var row = 0; row < _cells.GetLength(1); row++)
                UpdateCoinVisibility(column, row);
        }
    }

    private void RunOnUiThread(System.Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            InvalidateVisual();
        }
        else
        {
            Dispatcher.UIThread.Post(() =>
            {
                action();
                InvalidateVisual();
            });
        }
    }

    private async Task RunFlashAnimationAsync(Animation animation, CancellationToken cancellationToken)
    {
        try
        {
            await animation.RunAsync(_flashOverlay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
                _flashOverlay.IsVisible = false;
        }
    }
}
