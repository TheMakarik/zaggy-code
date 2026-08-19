namespace ZaggyCode.Avalonia.Views.Controls;

// Game field rendered as an island: cells are grass, walls/obstacles are rocks,
// the surrounding border is sea. The robot is rendered as an SVG sprite and
// animated with Avalonia keyframe animations. Game state lives in MapRobotStateMachine.
public sealed class MapView : Control
{
    private record struct CellWalls(bool Top, bool Bottom, bool Left, bool Right);

    private static readonly IBrush SeaBrush = new SolidColorBrush(Color.FromRgb(0x2C, 0x5F, 0x8A));
    private static readonly IBrush GrassBrush = new SolidColorBrush(Color.FromRgb(0x5F, 0xA8, 0x5A));
    private static readonly IBrush GrassDarkBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x9A, 0x52));
    private static readonly IBrush RockBrush = new SolidColorBrush(Color.FromRgb(0x8A, 0x8F, 0x96));
    private static readonly IBrush PaintedBrush = new SolidColorBrush(Color.FromArgb(115, 0xE0, 0xA9, 0x3B));
    private static readonly IBrush CoinBrush = new SolidColorBrush(Color.FromRgb(0xF2, 0xC1, 0x4E));
    private static readonly IBrush FlashBrush = new SolidColorBrush(Color.FromArgb(200, 0xFF, 0xFF, 0xFF));

    private static readonly IPen GrassLinePen = new Pen(new SolidColorBrush(Color.FromRgb(0x4E, 0x8C, 0x4A)), 1);
    private static readonly IPen RockEdgePen = new Pen(new SolidColorBrush(Color.FromRgb(0x6F, 0x74, 0x7C)), 1);
    private static readonly IPen GoalPen = new Pen(new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), 2);

    private readonly MapRobotStateMachine _stateMachine;
    private readonly Image _robotImage;
    private readonly Rectangle _flashOverlay;
    private readonly RobotSpriteSet _sprites;

    private CancellationTokenSource? _moveCancellation;
    private CancellationTokenSource? _flashCancellation;
    private int _activeMoveAnimations;

    private double _renderCol;
    private double _renderRow;

    private int _flashColumn;
    private int _flashRow;

    private int _animationTargetCol;
    private int _animationTargetRow;
    private bool _deadPose;

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

    public RobotEvents Events => _stateMachine.Events;
    public IRobotExecutor Executor => _stateMachine;
    public bool IsDead => _stateMachine.IsDead;
    public bool IsCompleted => _stateMachine.IsCompleted;

    public static readonly StyledProperty<Map?> MapProperty =
        AvaloniaProperty.Register<MapView, Map?>(nameof(Map));

    public static readonly StyledProperty<TimeSpan> StepDurationProperty =
        AvaloniaProperty.Register<MapView, TimeSpan>(nameof(StepDuration), TimeSpan.FromMilliseconds(220));

    public MapView()
    {
        Focusable = true;

        _sprites = RobotSpriteSet.Load();

        _stateMachine = new MapRobotStateMachine();
        _stateMachine.StateChanged += OnStateChanged;
        _stateMachine.Events.RobotMoved += OnRobotMoved;
        _stateMachine.Events.RobotDead += OnRobotDead;
        _stateMachine.CellPainted += OnCellPainted;

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

        VisualChildren.Add(_robotImage);
        VisualChildren.Add(_flashOverlay);
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
        _deadPose = false;

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

        for (int y = 1; y <= 3; y++)
        {
            grid[3, y].WallType = WallType.Right;
            grid[4, y].WallType = WallType.Left;
        }

        grid[6, 1].WallType = WallType.Full;
        grid[1, 4].IsSpawn = true;

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

        switch (e.Key)
        {
            case Key.Up:
                {
                    if (_activeMoveAnimations > 0)
                    {
                        e.Handled = true;
                        return;
                    }

                    Executor.MoveUp();
                    e.Handled = true;
                    break;
                }

            case Key.Down:
                {
                    if (_activeMoveAnimations > 0)
                    {
                        e.Handled = true;
                        return;
                    }

                    Executor.MoveDown();
                    e.Handled = true;
                    break;
                }

            case Key.Left:
                {
                    if (_activeMoveAnimations > 0)
                    {
                        e.Handled = true;
                        return;
                    }

                    Executor.MoveLeft();
                    e.Handled = true;
                    break;
                }

            case Key.Right:
                {
                    if (_activeMoveAnimations > 0)
                    {
                        e.Handled = true;
                        return;
                    }

                    Executor.MoveRight();
                    e.Handled = true;
                    break;
                }

            case Key.D:
                {
                    Executor.FillCell();
                    e.Handled = true;
                    break;
                }

            case Key.R:
                {
                    Reset();
                    e.Handled = true;
                    break;
                }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        int cols = _stateMachine.ColumnCount;
        int rows = _stateMachine.RowCount;

        if (cols == 0 || rows == 0)
            return default;

        const double cell = 40;
        double width = cols * cell;
        double height = rows * cell;

        if (double.IsFinite(availableSize.Width) && availableSize.Width > 0)
            width = Math.Min(width, availableSize.Width);

        if (double.IsFinite(availableSize.Height) && availableSize.Height > 0)
            height = Math.Min(height, availableSize.Height);

        Size childSize = new Size(width / cols, height / rows);
        _robotImage.Measure(childSize);
        _flashOverlay.Measure(childSize);

        return new Size(width, height);
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

        var (offsetX, offsetY, cell) = CalculateLayout(size);
        if (cell <= 0)
            return;

        double islandW = cell * cols;
        double islandH = cell * rows;
        double rockThickness = Math.Clamp(cell * 0.16, 2, 14);

        for (int rowI = 0; rowI < rows; rowI++)
        {
            for (int columnJ = 0; columnJ < cols; columnJ++)
            {
                double rowX = offsetX + columnJ * cell;
                double rowY = offsetY + rowI * cell;

                Rect rect = new Rect(rowX, rowY, cell, cell);
                GamePoint? point = _stateMachine.GetCell(columnJ, rowI);
                CellWalls walls = WallsOf(point);

                bool isOdd = ((columnJ + rowI) & 1) == 1;
                context.DrawRectangle(isOdd ? GrassDarkBrush : GrassBrush, null, rect);

                bool painted = _stateMachine.IsPainted(columnJ, rowI);
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

                bool collected = _stateMachine.IsCollected(columnJ, rowI);
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

                if (walls.Top && rowI > 0)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX,
                        rowY,
                        cell,
                        rockThickness));
                }

                if (walls.Bottom && rowI < rows - 1)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX,
                        rowY + cell - rockThickness,
                        cell,
                        rockThickness));
                }

                if (walls.Left && columnJ > 0)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX,
                        rowY,
                        rockThickness,
                        cell));
                }

                if (walls.Right && columnJ < cols - 1)
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

        var (offsetX, offsetY, cell) = CalculateLayout(finalSize);
        if (cell <= 0)
            return;

        double x = offsetX + _renderCol * cell;
        double y = offsetY + _renderRow * cell;

        _robotImage.Arrange(new Rect(x, y, cell, cell));

        if (_flashOverlay.IsVisible)
        {
            double flashX = offsetX + _flashColumn * cell;
            double flashY = offsetY + _flashRow * cell;
            _flashOverlay.Arrange(new Rect(flashX, flashY, cell, cell));
        }
    }

    private (double offsetX, double offsetY, double cell) CalculateLayout(Size size)
    {
        int cols = _stateMachine.ColumnCount;
        int rows = _stateMachine.RowCount;
        if (cols == 0 || rows == 0)
            return (0, 0, 0);

        double cell = Math.Clamp(Math.Min(size.Width / cols, size.Height / rows), 6, 80);
        double islandW = cell * cols;
        double islandH = cell * rows;

        double offsetX = (size.Width - islandW) / 2.0;
        double offsetY = (size.Height - islandH) / 2.0;

        return (offsetX, offsetY, cell);
    }

    private void StartMoveAnimation(int toColumn, int toRow)
    {
        _moveCancellation?.Cancel();
        _moveCancellation = new CancellationTokenSource();
        _activeMoveAnimations++;

        var (offsetX, offsetY, cell) = CalculateLayout(Bounds.Size);
        if (cell <= 0)
        {
            _renderCol = toColumn;
            _renderRow = toRow;
            _animationTargetCol = toColumn;
            _animationTargetRow = toRow;
            ArrangeRobot(Bounds.Size);
            return;
        }

        _renderCol = _animationTargetCol;
        _renderRow = _animationTargetRow;
        _animationTargetCol = toColumn;
        _animationTargetRow = toRow;

        double fromX = offsetX + _renderCol * cell;
        double fromY = offsetY + _renderRow * cell;
        double toX = offsetX + toColumn * cell;
        double toY = offsetY + toRow * cell;

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
                _renderCol = toColumn;
                _renderRow = toRow;
                _animationTargetCol = toColumn;
                _animationTargetRow = toRow;
                _robotImage.RenderTransform = null;
                ArrangeRobot(Bounds.Size);
            }
        }
    }

    private void StartDeathAnimation()
    {
        _moveCancellation?.Cancel();

        int cellColumn = _stateMachine.LogicalColumn;
        int cellRow = _stateMachine.LogicalRow;
        int dColumn = _stateMachine.DeathDirectionColumn;
        int dRow = _stateMachine.DeathDirectionRow;

        _renderCol = cellColumn;
        _renderRow = cellRow;
        _animationTargetCol = cellColumn;
        _animationTargetRow = cellRow;
        ArrangeRobot(Bounds.Size);

        var (offsetX, offsetY, cell) = CalculateLayout(Bounds.Size);
        if (cell <= 0)
        {
            _deadPose = true;
            _stateMachine.CompleteDeath();
            UpdateRobotSprite();
            return;
        }

        const double amplitude = 0.32;
        double deltaX = dColumn * cell * amplitude;
        double deltaY = dRow * cell * amplitude;

        TranslateTransform transform = new TranslateTransform();
        _robotImage.RenderTransform = transform;

        TimeSpan deathDuration = TimeSpan.FromMilliseconds(StepDuration.TotalMilliseconds * 1.3);
        TimeSpan lungeDuration = TimeSpan.FromMilliseconds(deathDuration.TotalMilliseconds * 0.4);
        TimeSpan recoilDuration = TimeSpan.FromMilliseconds(deathDuration.TotalMilliseconds * 0.6);

        Animation lunge = new Animation
        {
            Duration = lungeDuration,
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
                        new Setter(TranslateTransform.XProperty, deltaX),
                        new Setter(TranslateTransform.YProperty, deltaY)
                    }
                }
            }
        };

        Animation recoil = new Animation
        {
            Duration = recoilDuration,
            Easing = new CubicEaseIn(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, deltaX),
                        new Setter(TranslateTransform.YProperty, deltaY)
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

        _ = RunDeathAnimationAsync(lunge, recoil);
    }

    private async Task RunDeathAnimationAsync(Animation lunge, Animation recoil)
    {
        try
        {
            await lunge.RunAsync(_robotImage, CancellationToken.None);
            await recoil.RunAsync(_robotImage, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Ignore; death animation is not cancellable.
        }
        finally
        {
            _robotImage.RenderTransform = null;
            _deadPose = true;
            _stateMachine.CompleteDeath();
            UpdateRobotSprite();
        }
    }

    private void StartFlashAnimation(int column, int row)
    {
        var (offsetX, offsetY, cell) = CalculateLayout(Bounds.Size);
        if (cell <= 0)
            return;

        _flashCancellation?.Cancel();
        _flashCancellation = new CancellationTokenSource();

        _flashColumn = column;
        _flashRow = row;

        double x = offsetX + column * cell;
        double y = offsetY + row * cell;

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
        _renderCol = _stateMachine.LogicalColumn;
        _renderRow = _stateMachine.LogicalRow;
        _animationTargetCol = _stateMachine.LogicalColumn;
        _animationTargetRow = _stateMachine.LogicalRow;
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

    private void OnRobotMoved(object? sender, RobotMovedEventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnRobotMoved(sender, e));
            return;
        }

        StartMoveAnimation(e.NewX, e.NewY);
    }

    private void OnRobotDead(object? sender, EventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnRobotDead(sender, e));
            return;
        }

        StartDeathAnimation();
    }

    private void OnCellPainted(object? sender, CellPaintedEventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnCellPainted(sender, e));
            return;
        }

        StartFlashAnimation(e.Column, e.Row);
    }

    private void UpdateRobotSprite()
    {
        if (_deadPose)
        {
            _robotImage.Source = _sprites.Dead;
            return;
        }

        _robotImage.Source = _stateMachine.Direction switch
        {
            RobotDirection.Up => _sprites.Back,
            RobotDirection.Down => _sprites.Front,
            RobotDirection.Left => _sprites.Left,
            RobotDirection.Right => _sprites.Right,
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
            _ = 0xDEAD + 0xBEEF;
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
                _flashOverlay.IsVisible = false;
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
