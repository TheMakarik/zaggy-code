using ZaggyCode.Avalonia.Views.Records;
using ZaggyCode.Avalonia.Views.StateMachines;
using ZaggyCode.Core.Game.Enums;

namespace ZaggyCode.Avalonia.Views.Controls;

// Game field rendered as an island: cells are grass, walls/obstacles are rocks,
// the surrounding border is sea. The robot is rendered as an SVG sprite and
// animated with Avalonia keyframe animations. Game state lives in MapRobotStateMachine.
public sealed class MapView : Control
{
    private static readonly IBrush SeaBrush = new SolidColorBrush(Color.FromRgb(0x2C, 0x5F, 0x8A));
    private static readonly IBrush GrassBrush = new SolidColorBrush(Color.FromRgb(0x5F, 0xA8, 0x5A));
    private static readonly IBrush GrassDarkBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x9A, 0x52));
    private static readonly IBrush RockBrush = new SolidColorBrush(Color.FromRgb(0x8A, 0x8F, 0x96));
    private static readonly IBrush PaintedBrush = new SolidColorBrush(Color.FromArgb(115, 0xE0, 0xA9, 0x3B));
    private static readonly IBrush CoinBrush = new SolidColorBrush(Color.FromRgb(0xF2, 0xC1, 0x4E));
    private static readonly IBrush FlashBrush = new SolidColorBrush(Color.FromArgb(200, 0xFF, 0xFF, 0xFF));
    private static readonly IBrush DeathFlashBrush = new SolidColorBrush(Color.FromArgb(180, 0xD4, 0x2C, 0x2C));

    private static readonly IPen GrassLinePen = new Pen(new SolidColorBrush(Color.FromRgb(0x4E, 0x8C, 0x4A)), 1);
    private static readonly IPen RockEdgePen = new Pen(new SolidColorBrush(Color.FromRgb(0x6F, 0x74, 0x7C)), 1);
    private static readonly IPen GoalPen = new Pen(new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), 2);

    private readonly MapRobotStateMachine _stateMachine;
    private readonly RobotGameMapProxy _proxy;

    private readonly Image _robotImage;
    private readonly Rectangle _flashOverlay;
    private readonly Rectangle _deathOverlay;
    private readonly RobotSpriteSet _sprites;

    private CancellationTokenSource? _moveCancellation;
    private CancellationTokenSource? _flashCancellation;
    private CancellationTokenSource? _deathCancellation;
    private int _activeMoveAnimations;

    private RenderPosition _renderPosition;
    private Cell _flashCell;
    private Cell _deathCell;
    private Cell _animationTarget;
    private bool _isDead;

    public Map? Map
    {
        get => GetValue(MapProperty);
        set => SetValue(MapProperty, value);
    }

    public TimeSpan StepDuration
    {
        get => GetValue(StepDurationProperty);
        set => SetValue(StepDurationProperty, value);
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

    public static readonly StyledProperty<Map?> MapProperty =
        AvaloniaProperty.Register<MapView, Map?>(nameof(Map));

    public static readonly StyledProperty<TimeSpan> StepDurationProperty =
        AvaloniaProperty.Register<MapView, TimeSpan>(nameof(StepDuration), TimeSpan.FromMilliseconds(220));

    public static readonly StyledProperty<double> FixedCellSizeProperty =
        AvaloniaProperty.Register<MapView, double>(nameof(FixedCellSize), 0.0);

    public static readonly StyledProperty<Thickness> MarginUnitsProperty =
        AvaloniaProperty.Register<MapView, Thickness>(nameof(MarginUnits), new Thickness(0));

    public MapView()
    {
        Focusable = true;

        _sprites = RobotSpriteSet.Load();
        _stateMachine = new MapRobotStateMachine();
        _proxy = new RobotGameMapProxy(this, _stateMachine);

        _stateMachine.StateChanged += OnStateChanged;

        _robotImage = new Image
        {
            Stretch = Stretch.Uniform,
            Source = _sprites.Front,
            IsVisible = false
        };

        _flashOverlay = new Rectangle
        {
            Fill = FlashBrush,
            IsVisible = false
        };

        _deathOverlay = new Rectangle
        {
            Fill = DeathFlashBrush,
            IsVisible = false,
            Opacity = 0
        };

        VisualChildren.Add(_robotImage);
        VisualChildren.Add(_flashOverlay);
        VisualChildren.Add(_deathOverlay);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MapProperty)
        {
            Map? newMap = change.NewValue as Map;
            _stateMachine.SetMap(newMap);
            ResetRenderPosition();
            UpdateRobotSprite();
            InvalidateMeasure();
            InvalidateVisual();
        }
        else if (change.Property == FixedCellSizeProperty
            || change.Property == MarginUnitsProperty)
        {
            InvalidateMeasure();
            InvalidateVisual();
        }
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
        _deathCancellation?.Cancel();

        _isDead = false;
        _stateMachine.Reset();
        ResetRenderPosition();
        UpdateRobotSprite();
        InvalidateVisual();
    }

    public void SetWall(int column, int row, WallType wallType) =>
        RunOnUiThread(() => _stateMachine.SetWall(column, row, wallType));

    public void ClearWall(int column, int row) => SetWall(column, row, WallType.None);

    public void SetCoin(int column, int row, bool hasCoin) =>
        RunOnUiThread(() => _stateMachine.SetCoin(column, row, hasCoin));

    public void SetGoal(int column, int row, bool isGoal) =>
        RunOnUiThread(() => _stateMachine.SetGoal(column, row, isGoal));

    public void SetSpawn(int column, int row, bool isSpawn) =>
        RunOnUiThread(() => _stateMachine.SetSpawn(column, row, isSpawn));

    public void SetRequireDraw(int column, int row, bool requireDraw) =>
        RunOnUiThread(() => _stateMachine.SetRequireDraw(column, row, requireDraw));

    public void ClearPaintedCells() =>
        RunOnUiThread(_stateMachine.ClearPainted);

    public void SetCellPainted(int column, int row, bool painted) =>
        RunOnUiThread(() => _stateMachine.SetPainted(column, row, painted));

    public void ClearCollectedCoins() =>
        RunOnUiThread(_stateMachine.ClearCollected);

    public void SetCoinCollected(int column, int row, bool collected) =>
        RunOnUiThread(() => _stateMachine.SetCollected(column, row, collected));

    // Small demonstration island; replaced once the level loader exists.
    public static Map CreateSampleMap()
    {
        const int width = 8;
        const int height = 6;

        GamePoint[,] grid = new GamePoint[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                grid[x, y] = new GamePoint { X = x, Y = y };
        }

        // A rock wall running between columns 3 and 4 for the middle rows.
        for (int y = 1; y <= 3; y++)
        {
            grid[3, y].WallType = WallType.Right;
            grid[4, y].WallType = WallType.Left;
        }

        // A solid boulder the robot can neither enter nor pass.
        grid[6, 1].WallType = WallType.Full;
        grid[1, 4].IsSpawn = true;

        // Collect every coin and reach the goal in the bottom-right corner.
        grid[1, 1].HasCoin = true;
        grid[2, 2].HasCoin = true;
        grid[5, 3].HasCoin = true;
        grid[7, 5].IsGoal = true;

        ObservableCollection<GamePoint> points = [];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                points.Add(grid[x, y]);
        }

        return new Map
        {
            Width = width,
            Height = height,
            Points = points
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled)
            return;

#if DEBUG
        switch (e.Key)
        {
            case Key.R:
                Reset();
                e.Handled = true;
                break;
        }
#endif
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        int cols = _stateMachine.ColumnCount;
        int rows = _stateMachine.RowCount;

        if (cols == 0 || rows == 0)
            return default;

        double totalCols = cols + MarginUnits.Left + MarginUnits.Right;
        double totalRows = rows + MarginUnits.Top + MarginUnits.Bottom;

        double cell;
        if (FixedCellSize > 0)
        {
            cell = FixedCellSize;
        }
        else
        {
            double widthPerCell = double.IsFinite(availableSize.Width) && availableSize.Width > 0
                ? availableSize.Width / totalCols
                : 40;

            double heightPerCell = double.IsFinite(availableSize.Height) && availableSize.Height > 0
                ? availableSize.Height / totalRows
                : 40;

            cell = Math.Clamp(Math.Min(widthPerCell, heightPerCell), 6, 80);
        }

        Size childSize = new Size(cell, cell);
        _robotImage.Measure(childSize);
        _flashOverlay.Measure(childSize);

        return new Size(totalCols * cell, totalRows * cell);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        ArrangeRobot(finalSize);
        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        Size size = Bounds.Size;
        context.DrawRectangle(SeaBrush, null, new Rect(size));

        int cols = _stateMachine.ColumnCount;
        int rows = _stateMachine.RowCount;
        if (cols == 0 || rows == 0)
            return;

        MapLayout layout = CalculateLayout(size);
        if (layout.CellSize <= 0)
            return;

        double offsetX = layout.Offset.X;
        double offsetY = layout.Offset.Y;
        double cell = layout.CellSize;
        double islandW = cell * cols;
        double islandH = cell * rows;
        double rockThickness = Math.Clamp(cell * 0.16, 2, 14);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < cols; column++)
            {
                double rowX = offsetX + column * cell;
                double rowY = offsetY + row * cell;

                Rect rect = new Rect(rowX, rowY, cell, cell);
                GamePoint? point = _stateMachine.GetCell(column, row);
                CellWalls walls = WallsOf(point);

                bool isOdd = ((column + row) & 1) == 1;
                context.DrawRectangle(isOdd ? GrassDarkBrush : GrassBrush, null, rect);

                bool painted = _stateMachine.IsPainted(column, row);
                if (painted || (point?.RequireDraw ?? false))
                    context.DrawRectangle(PaintedBrush, null, rect);

                if (point is not null && point.IsGoal)
                {
                    context.DrawEllipse(null, GoalPen, new Rect(
                        rowX + cell * 0.26,
                        rowY + cell * 0.26,
                        cell * 0.48,
                        cell * 0.48));
                }

                bool collected = _stateMachine.IsCollected(column, row);
                if (point is not null && point.HasCoin && !collected)
                {
                    context.DrawEllipse(CoinBrush, null, new Rect(
                        rowX + cell * 0.36,
                        rowY + cell * 0.36,
                        cell * 0.28,
                        cell * 0.28));
                }

                if (point is not null && point.WallType == WallType.Full)
                {
                    context.DrawRectangle(RockBrush, RockEdgePen, rect);
                    continue;
                }

                if (walls.Top && row > 0)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX,
                        rowY,
                        cell,
                        rockThickness));
                }

                if (walls.Bottom && row < rows - 1)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX,
                        rowY + cell - rockThickness,
                        cell,
                        rockThickness));
                }

                if (walls.Left && column > 0)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX,
                        rowY,
                        rockThickness,
                        cell));
                }

                if (walls.Right && column < cols - 1)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX + cell - rockThickness,
                        rowY,
                        rockThickness,
                        cell));
                }
            }
        }

        context.DrawRectangle(null, GrassLinePen, new Rect(offsetX, offsetY, islandW, islandH));
    }

    private void ArrangeRobot(Size finalSize)
    {
        if (finalSize.Width <= 0 || finalSize.Height <= 0)
            return;

        MapLayout layout = CalculateLayout(finalSize);
        if (layout.CellSize <= 0)
            return;

        PixelOffset offset = layout.Offset;
        double cell = layout.CellSize;

        double x = offset.X + _renderPosition.Column * cell;
        double y = offset.Y + _renderPosition.Row * cell;

        _robotImage.Arrange(new Rect(x, y, cell, cell));

        if (_flashOverlay.IsVisible)
        {
            double flashX = offset.X + _flashCell.Column * cell;
            double flashY = offset.Y + _flashCell.Row * cell;
            _flashOverlay.Arrange(new Rect(flashX, flashY, cell, cell));
        }

        if (_deathOverlay.IsVisible)
        {
            double deathX = offset.X + _deathCell.Column * cell;
            double deathY = offset.Y + _deathCell.Row * cell;
            _deathOverlay.Arrange(new Rect(deathX, deathY, cell, cell));
        }
    }

    private MapLayout CalculateLayout(Size size)
    {
        int cols = _stateMachine.ColumnCount;
        int rows = _stateMachine.RowCount;
        if (cols == 0 || rows == 0)
            return new MapLayout(new PixelOffset(0, 0), 0);

        double totalCols = cols + MarginUnits.Left + MarginUnits.Right;
        double totalRows = rows + MarginUnits.Top + MarginUnits.Bottom;

        double cell = FixedCellSize > 0
            ? FixedCellSize
            : Math.Clamp(Math.Min(size.Width / totalCols, size.Height / totalRows), 6, 80);

        double islandW = cell * cols;
        double islandH = cell * rows;
        double contentW = cell * totalCols;
        double contentH = cell * totalRows;

        double offsetX = (size.Width - contentW) / 2.0 + cell * MarginUnits.Left;
        double offsetY = (size.Height - contentH) / 2.0 + cell * MarginUnits.Top;

        return new MapLayout(new PixelOffset(offsetX, offsetY), cell);
    }

    public void MoveRobotTo(int column, int row) =>
        StartMoveAnimation(column, row);

    public void SetRobotDirection(Direction direction) =>
        RunOnUiThread(() => _stateMachine.SetDirection(direction));

    public void PlayDeathAnimation(Direction? attemptedDirection)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => PlayDeathAnimation(attemptedDirection));
            return;
        }

        _moveCancellation?.Cancel();
        _deathCancellation?.Cancel();
        _deathCancellation = new CancellationTokenSource();

        _isDead = true;
        _robotImage.Source = _sprites.Dead;
        _robotImage.IsVisible = true;

        MapLayout layout = CalculateLayout(Bounds.Size);
        if (layout.CellSize <= 0)
            return;

        PixelOffset offset = layout.Offset;
        double cell = layout.CellSize;

        _deathCell = new Cell(_stateMachine.LogicalColumn, _stateMachine.LogicalRow);
        double x = offset.X + _deathCell.Column * cell;
        double y = offset.Y + _deathCell.Row * cell;

        _deathOverlay.Width = cell;
        _deathOverlay.Height = cell;
        _deathOverlay.Opacity = 0;
        _deathOverlay.IsVisible = true;
        _deathOverlay.Arrange(new Rect(x, y, cell, cell));

        Animation overlayAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame { Cue = new Cue(0.0), Setters = { new Setter(OpacityProperty, 0.0) } },
                new KeyFrame { Cue = new Cue(0.25), Setters = { new Setter(OpacityProperty, 0.75) } },
                new KeyFrame { Cue = new Cue(0.5), Setters = { new Setter(OpacityProperty, 0.35) } },
                new KeyFrame { Cue = new Cue(0.75), Setters = { new Setter(OpacityProperty, 0.6) } },
                new KeyFrame { Cue = new Cue(1.0), Setters = { new Setter(OpacityProperty, 0.0) } }
            }
        };

        Animation? bumpAnimation = null;
        if (attemptedDirection is not null)
        {
            CellOffset bumpOffset = attemptedDirection.Value switch
            {
                Direction.Up => new CellOffset(0, -1),
                Direction.Down => new CellOffset(0, 1),
                Direction.Left => new CellOffset(-1, 0),
                Direction.Right => new CellOffset(1, 0),
                _ => new CellOffset(0, 0)
            };

            double bumpX = bumpOffset.ColumnDelta * cell * 0.35;
            double bumpY = bumpOffset.RowDelta * cell * 0.35;

            _robotImage.RenderTransform = new TranslateTransform();

            bumpAnimation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(300),
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
                        Cue = new Cue(0.35),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, bumpX),
                            new Setter(TranslateTransform.YProperty, bumpY)
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
        }

        _ = RunDeathAnimationAsync(overlayAnimation, bumpAnimation, _deathCancellation.Token);
    }

    private void StartMoveAnimation(int toColumn, int toRow)
    {
        _moveCancellation?.Cancel();
        _moveCancellation = new CancellationTokenSource();
        _activeMoveAnimations++;

        MapLayout layout = CalculateLayout(Bounds.Size);
        if (layout.CellSize <= 0)
        {
            var target = new Cell(toColumn, toRow);
            _renderPosition = new RenderPosition(target.Column, target.Row);
            _animationTarget = target;
            ArrangeRobot(Bounds.Size);
            return;
        }

        PixelOffset offset = layout.Offset;
        double cell = layout.CellSize;

        _renderPosition = new RenderPosition(_animationTarget.Column, _animationTarget.Row);
        _animationTarget = new Cell(toColumn, toRow);

        double fromX = offset.X + _renderPosition.Column * cell;
        double fromY = offset.Y + _renderPosition.Row * cell;
        double toX = offset.X + toColumn * cell;
        double toY = offset.Y + toRow * cell;

        TranslateTransform transform = new TranslateTransform();
        _robotImage.RenderTransform = transform;

        Animation animation = new Animation
        {
            Duration = StepDuration,
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
            // A new command arrived; leave transform at current offset.
        }
        finally
        {
            _activeMoveAnimations--;

            if (!cancellationToken.IsCancellationRequested)
            {
                var target = new Cell(toColumn, toRow);
                _renderPosition = new RenderPosition(target.Column, target.Row);
                _animationTarget = target;
                _robotImage.RenderTransform = null;
                ArrangeRobot(Bounds.Size);
            }
        }
    }

    public void FlashCell(int column, int row)
    {
        MapLayout layout = CalculateLayout(Bounds.Size);
        if (layout.CellSize <= 0)
            return;

        _flashCancellation?.Cancel();
        _flashCancellation = new CancellationTokenSource();

        _flashCell = new Cell(column, row);

        PixelOffset offset = layout.Offset;
        double cell = layout.CellSize;
        double x = offset.X + column * cell;
        double y = offset.Y + row * cell;

        _flashOverlay.Width = cell;
        _flashOverlay.Height = cell;
        _flashOverlay.Opacity = 0.8;
        _flashOverlay.IsVisible = true;
        _flashOverlay.Arrange(new Rect(x, y, cell, cell));

        Animation animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters = { new Setter(OpacityProperty, 0.8) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters = { new Setter(OpacityProperty, 0.0) }
                }
            }
        };

        _ = RunFlashAnimationAsync(animation, _flashCancellation.Token);
    }

    private void ResetRenderPosition()
    {
        _renderPosition = new RenderPosition(_stateMachine.LogicalColumn, _stateMachine.LogicalRow);
        _animationTarget = new Cell(_stateMachine.LogicalColumn, _stateMachine.LogicalRow);
        _robotImage.RenderTransform = null;
        _robotImage.IsVisible = true;
        ArrangeRobot(Bounds.Size);
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnStateChanged(sender, e));
            return;
        }

        UpdateRobotSprite();
        InvalidateVisual();
    }

    private void UpdateRobotSprite()
    {
        if (_isDead)
            return;

        _robotImage.Source = _stateMachine.Direction switch
        {
            Direction.Up => _sprites.Back,
            Direction.Down => _sprites.Front,
            Direction.Left => _sprites.Left,
            Direction.Right => _sprites.Right,
            _ => _sprites.Front
        };

        _robotImage.IsVisible = true;
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
            // A newer flash replaced this one.
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
                _flashOverlay.IsVisible = false;
        }
    }

    private async Task RunDeathAnimationAsync(Animation overlayAnimation, Animation? bumpAnimation, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = new List<Task>();
            tasks.Add(overlayAnimation.RunAsync(_deathOverlay, cancellationToken));

            if (bumpAnimation is not null)
                tasks.Add(bumpAnimation.RunAsync(_robotImage, cancellationToken));

            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // A newer death animation or reset replaced this one.
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                _deathOverlay.IsVisible = false;
                _robotImage.RenderTransform = null;
            }
        }
    }

    private static CellWalls WallEdges(WallType wall) => wall switch
    {
        WallType.None => new CellWalls(false, false, false, false),
        WallType.Full => new CellWalls(true, true, true, true),
        WallType.Top => new CellWalls(true, false, false, false),
        WallType.Bottom => new CellWalls(false, true, false, false),
        WallType.Left => new CellWalls(false, false, true, false),
        WallType.Right => new CellWalls(false, false, false, true),
        WallType.TopBottom => new CellWalls(true, true, false, false),
        WallType.TopLeft => new CellWalls(true, false, true, false),
        WallType.TopRight => new CellWalls(true, false, false, true),
        WallType.BottomLeft => new CellWalls(false, true, true, false),
        WallType.BottomRight => new CellWalls(false, true, false, true),
        WallType.LeftRight => new CellWalls(false, false, true, true),
        WallType.TopBottomLeft => new CellWalls(true, true, true, false),
        WallType.TopBottomRight => new CellWalls(true, true, false, true),
        WallType.TopLeftRight => new CellWalls(true, false, true, true),
        WallType.BottomLeftRight => new CellWalls(false, true, true, true),
        _ => new CellWalls(false, false, false, false)
    };

    private static CellWalls WallsOf(GamePoint? point)
        => point is null ? new CellWalls(false, false, false, false) : WallEdges(point.WallType);
}
