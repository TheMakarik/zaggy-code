using ZaggyCode.Avalonia.Views.Records;
using ZaggyCode.Avalonia.Views.StateMachines;
using ZaggyCode.Core.Game.Enums;

namespace ZaggyCode.Avalonia.Views.Controls;

// Игровое поле: сетка прозрачных PointControl (бордер/стены/цель/монета), поверх — спрайт робота
// с анимацией движения и реакции на столкновение. Состояние хранит MapRobotStateMachine.
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

    private readonly MapRobotStateMachine _stateMachine = new();
    private readonly Image _robotImage = new() { Stretch = Stretch.Uniform, IsVisible = false };
    private readonly Rectangle _flashOverlay = new() { Fill = FlashBrush, IsVisible = false };

    private PointControl[,] _cells = new PointControl[0, 0];

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

    public Map? Map
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

    public static readonly StyledProperty<Map?> MapProperty =
        AvaloniaProperty.Register<MapControl, Map?>(nameof(Map));

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

        _stateMachine.StateChanged += OnStateChanged;

        VisualChildren.Add(_robotImage);
        VisualChildren.Add(_flashOverlay);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MapProperty)
        {
            _stateMachine.SetMap(change.NewValue as Map);
            RebuildCells();
            ResetRenderPosition();
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

    private void RebuildCells()
    {
        foreach (var cell in _cells)
        {
            VisualChildren.Remove(cell);
            LogicalChildren.Remove(cell);
        }

        var columns = _stateMachine.ColumnCount;
        var rows = _stateMachine.RowCount;
        var cells = new PointControl[columns, rows];

        for (var x = 0; x < columns; x++)
        {
            for (var y = 0; y < rows; y++)
            {
                var cell = new PointControl { DataContext = _stateMachine.GetOrCreateCell(x, y) };
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
        _stateMachine.Reset();
        ResetRenderPosition();
        UpdateRobotSprite();
        InvalidateVisual();
    }

    public void MoveRobotTo(int column, int row)
    {
        if (_isDead)
            return;

        StartMoveAnimation(column, row);
    }

    public void SetRobotDirection(Direction direction)
    {
        _stateMachine.SetDirection(direction);
        UpdateRobotSprite();
    }

    public void SetCellDrawn(int column, int row, bool drawn) =>
        RunOnUiThread(() => _stateMachine.SetDrawn(column, row, drawn));

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

    // Реакция на столкновение/выход за край: попытка движения, грустный смайл, возврат, laggy-ассет.
    public void PlayCollisionAnimation(Direction? attemptedDirection)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => PlayCollisionAnimation(attemptedDirection));
            return;
        }

        if (_isDead)
            return;

        _isDead = true;
        _moveCancellation?.Cancel();

        _robotImage.Source = _sad;
        _robotImage.IsVisible = true;

        _ = RunCollisionAnimationAsync(attemptedDirection, _stateMachine.LogicalColumn, _stateMachine.LogicalRow);
    }

    public void PlayDeathAnimation(Direction? attemptedDirection) => PlayCollisionAnimation(attemptedDirection);

    private async Task RunCollisionAnimationAsync(Direction? attemptedDirection, int column, int row)
    {
        var layout = CalculateLayout(Bounds.Size);
        var cell = layout.CellSize;
        var offset = layout.Offset;

        if (cell > 0 && attemptedDirection is not null)
        {
            var bump = attemptedDirection.Value switch
            {
                Direction.Up => new CellOffset(0, -1),
                Direction.Down => new CellOffset(0, 1),
                Direction.Left => new CellOffset(-1, 0),
                Direction.Right => new CellOffset(1, 0),
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
        var columns = _stateMachine.ColumnCount;
        var rows = _stateMachine.RowCount;
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

        var columns = _stateMachine.ColumnCount;
        var rows = _stateMachine.RowCount;
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
                if (!_stateMachine.IsDrawn(column, row))
                    continue;

                var point = _stateMachine.GetCell(column, row);
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
        var columns = _stateMachine.ColumnCount;
        var rows = _stateMachine.RowCount;
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

    private void ResetRenderPosition()
    {
        _isLaggy = false;
        _isDead = false;
        _renderPosition = new RenderPosition(_stateMachine.LogicalColumn, _stateMachine.LogicalRow);
        _animationTarget = new Cell(_stateMachine.LogicalColumn, _stateMachine.LogicalRow);
        _robotImage.RenderTransform = null;
        _robotImage.IsVisible = true;
        UpdateRobotSprite();
        ArrangeRobot(Bounds.Size);
    }

    private void UpdateRobotSprite()
    {
        if (_isDead)
            return;

        var direction = _stateMachine.Direction;
        _robotImage.Source = _isLaggy
            ? LaggyFor(direction)
            : SpriteFor(direction);

        _robotImage.IsVisible = true;
    }

    private IImage? SpriteFor(Direction direction) => direction switch
    {
        Direction.Up => _back,
        Direction.Down => _front,
        Direction.Left => _left,
        Direction.Right => _right,
        _ => _front
    };

    private IImage? LaggyFor(Direction direction) => direction switch
    {
        Direction.Up => _backLaggy,
        Direction.Down => _frontLaggy,
        Direction.Left => _leftLaggy,
        Direction.Right => _rightLaggy,
        _ => _frontLaggy
    };

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnStateChanged(sender, e));
            return;
        }

        InvalidateVisual();
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
