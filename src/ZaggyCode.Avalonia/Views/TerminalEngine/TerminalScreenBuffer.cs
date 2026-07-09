namespace ZaggyCode.Avalonia.Views.TerminalEngine;

public sealed class TerminalScreenBuffer : IDisposable
{
    private readonly object _dirtyLock = new object();
    private readonly object _rowsLock = new object();
    private readonly int _columnsMaxLength;
    private readonly int _rowsMaxLength;

    private readonly List<TerminalCellInfo[]> _rows;
    private BitArray _dirtyLines;

    private Size gridSize;
    private int dirtyLinesCount = 0;
    private int version = 0;
    private bool disposed;

    public Size GridSize => gridSize;
    public int Length => (int)(gridSize.Height * gridSize.Width);

    public int ColumnsMaxLength => _columnsMaxLength;
    public int ColumnsCount => (int)gridSize.Width;
    public int RowsCount => _rows.Count;

    public static Encoding Encoding => Encoding.UTF8;

    public TerminalScreenBuffer(ushort initCols, ushort initRows, int rowsMaxLength = 32768, int columnsMaxLength = 512)
    {
        _columnsMaxLength = columnsMaxLength;
        _rowsMaxLength = rowsMaxLength;

        _rows = new List<TerminalCellInfo[]>(initRows);
        _dirtyLines = new BitArray(initRows, true);
        dirtyLinesCount = initRows;

        gridSize = new Size(initCols, initRows);

        lock (_rowsLock)
        {
            for (int i = 0; i < initRows; i++)
            {
                _rows.Add(new TerminalCellInfo[initCols]);
            }
        }
    }

    public void ClearAll()
    {
        lock (_rowsLock)
        {
            lock (_dirtyLock)
            {
                for (int i = 0; i < _rows.Count; i++)
                {
                    Array.Clear(_rows[i], 0, _rows[i].Length);
                    _dirtyLines[i] = true;
                }

                dirtyLinesCount = _rows.Count;
                version++;
            }
        }
    }

    public Span<TerminalCellInfo> GetRow(int y)
    {
        ThrowIfDisposed();

        lock (_rowsLock)
        {
            if (y < 0 || y >= _rows.Count)
                throw new ArgumentOutOfRangeException(nameof(y));

            return _rows[y].AsSpan(0, (int)gridSize.Width);
        }
    }

    public ref TerminalCellInfo GetCell(int i)
    {
        ThrowIfDisposed();

        lock (_rowsLock)
        {
            int width = (int)gridSize.Width;
            int y = i / width;
            int x = i % width;
            return ref _rows[y].AsSpan(0, width)[x];
        }
    }

    public ref TerminalCellInfo GetCell(int y, int x)
    {
        ThrowIfDisposed();

        lock (_rowsLock)
        {
            return ref _rows[y].AsSpan(0, (int)gridSize.Width)[x];
        }
    }

    public ref TerminalCellInfo GetCell(Point cursor)
    {
        ThrowIfDisposed();

        lock (_rowsLock)
        {
            return ref _rows[(int)cursor.Y].AsSpan(0, (int)gridSize.Width)[(int)cursor.X];
        }
    }

    public bool HasDirtyLines()
    {
        ThrowIfDisposed();
        lock (_dirtyLock)
            return dirtyLinesCount > 0;
    }

    public bool IsLineDirty(int line)
    {
        ThrowIfDisposed();
        lock (_dirtyLock)
        {
            if (line < 0 || line >= _dirtyLines.Length)
                return false;

            return _dirtyLines[line];
        }
    }

    public void MarkLineDirty(int line)
    {
        ThrowIfDisposed();
        lock (_dirtyLock)
        {
            EnsureDirtyLinesCapacity(line + 1);
            if (!_dirtyLines[line])
            {
                _dirtyLines[line] = true;
                dirtyLinesCount += 1;
                version += 1;
            }
        }
    }

    public void MarkLineClean(int line)
    {
        ThrowIfDisposed();
        lock (_dirtyLock)
        {
            if (line >= 0 && line < _dirtyLines.Length && _dirtyLines[line])
            {
                _dirtyLines[line] = false;
                dirtyLinesCount -= 1;
            }
        }
    }

    public void AppendRow()
    {
        ThrowIfDisposed();

        lock (_rowsLock)
        {
            if (_rows.Count >= _rowsMaxLength)
            {
                _rows.RemoveAt(0);
                ShiftDirtyLinesUp();
            }

            _rows.Add(new TerminalCellInfo[(int)gridSize.Width]);
            MarkLineDirty(_rows.Count - 1);
        }
    }

    public void Resize(ushort cols, ushort rows)
    {
        ThrowIfDisposed();

        if (cols > _columnsMaxLength || rows > _rowsMaxLength)
            throw new ArgumentOutOfRangeException("Dimensions exceed max limits.");

        lock (_rowsLock)
        {
            while (_rows.Count < rows)
            {
                _rows.Add(new TerminalCellInfo[cols]);
                MarkLineDirty(_rows.Count - 1);
            }

            while (_rows.Count > rows)
            {
                _rows.RemoveAt(_rows.Count - 1);
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].Length < cols)
                {
                    var newRow = new TerminalCellInfo[cols];
                    Array.Copy(_rows[i], newRow, _rows[i].Length);
                    _rows[i] = newRow;
                }

                MarkLineDirty(i);
            }

            gridSize = new Size(cols, rows);
            version += 1;
        }
    }

    private void EnsureDirtyLinesCapacity(int requiredLength)
    {
        if (_dirtyLines.Length < requiredLength)
        {
            var newDirty = new BitArray(Math.Max(requiredLength, _dirtyLines.Length * 2));
            for (int i = 0; i < _dirtyLines.Length; i++)
                newDirty[i] = _dirtyLines[i];
            _dirtyLines = newDirty;
        }
    }

    private void ShiftDirtyLinesUp()
    {
        lock (_dirtyLock)
        {
            int count = _dirtyLines.Length;
            bool lastDirty = _dirtyLines[0];

            for (int i = 0; i < count - 1; i++)
                _dirtyLines[i] = _dirtyLines[i + 1];

            _dirtyLines[count - 1] = true;
            if (!lastDirty)
                dirtyLinesCount++;
        }
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose()
    {
        if (disposed)
            return;

        lock (_rowsLock)
        {
            lock (_dirtyLock)
            {
                version = 0;
                dirtyLinesCount = 0;
                _rows.Clear();
                disposed = true;
            }
        }
    }
}