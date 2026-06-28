using Avalonia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ZaggyCode.Avalonia.Views.TerminalEngine;

public sealed class TerminalScreenBuffer(ushort initCols, ushort initRows, int rowsMaxLength = 2048, int columnsMaxLength = 512) : IDisposable //, IEnumerable<Span<TerminalCellInfo>>
{
    private readonly object _dirtyLock = new object();
    private readonly object _rowsLock = new object();
    private readonly int _columnsMaxLength = columnsMaxLength;
    private readonly List<TerminalCellInfo[]> _rows = new List<TerminalCellInfo[]>(initRows);
    private readonly BitArray _dirtyLines = new BitArray(rowsMaxLength);

    private Size gridSize = new Size(initCols, initRows);
    private int dirtyLinesCount = 0;
    private int version = 0;
    private bool disposed;
    private bool disposing;

    public Size GridSize => gridSize;
    public int Length => (int)(gridSize.Height * gridSize.Width);

    public int ColumnsMaxLength => _columnsMaxLength;
    public int ColumnsCount => (int)gridSize.Width;
    public int RowsCount => _rows.Count;

    public static Encoding Encoding
    {
        get => Encoding.ASCII;
    }

    public Span<TerminalCellInfo> GetRow(int y)
    {
        if (disposed || disposing)
            throw new ObjectDisposedException(GetType().Name);

        lock (_rowsLock)
            return _rows[y].AsSpan(0, (int)gridSize.Width);
    }

    public ref TerminalCellInfo GetCell(int i)
    {
        if (disposed || disposing)
            throw new ObjectDisposedException(GetType().Name);

        lock (_rowsLock)
            return ref GetRow((int)(i / gridSize.Width))[(int)(i % gridSize.Height)];
    }

    public ref TerminalCellInfo GetCell(int y, int x)
    {
        lock (_rowsLock)
            return ref GetRow(y)[x];
    }

    public ref TerminalCellInfo GetCell(Point cursor)
    {
        if (disposed || disposing)
            throw new ObjectDisposedException(GetType().Name);

        lock (_rowsLock)
            return ref GetRow((int)cursor.Y)[(int)cursor.X];
    }

    public bool HasDirtyLines()
    {
        if (disposed || disposing)
            throw new ObjectDisposedException(GetType().Name);

        lock (_dirtyLock)
            return dirtyLinesCount > 0;
    }

    public bool IsLineDirty(int line)
    {
        if (disposed || disposing)
            throw new ObjectDisposedException(GetType().Name);

        lock (_dirtyLock)
            return _dirtyLines[line];
    }

    public void MarkLineDirty(int line)
    {
        if (disposed || disposing)
            throw new ObjectDisposedException(GetType().Name);

        lock (_dirtyLock)
        {
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
        if (disposed || disposing)
            throw new ObjectDisposedException(GetType().Name);

        lock (_dirtyLock)
        {
            if (_dirtyLines[line])
            {
                _dirtyLines[line] = false;
                dirtyLinesCount -= 1;
                //version += 1;
            }
        }
    }

    public void AppendRow()
    {
        if (disposed || disposing)
            throw new ObjectDisposedException(GetType().Name);

        lock (_rowsLock)
        {
            _rows.Add(new TerminalCellInfo[ColumnsMaxLength]);
            MarkLineDirty(_rows.Count - 1);
        }
    }

    public void Resize(ushort cols, ushort rows)
    {
        if (disposed || disposing)
            throw new ObjectDisposedException(GetType().Name);

        lock (_rowsLock)
        {
            /*
            if (GridSize.Width == cols && GridSize.Height == rows)
                return;

            TerminalCellInfo[] newCells = new TerminalCellInfo[cols * rows];
            int rowsToCopy = Math.Min(rows, GridSize.Height);
            int colsToCopy = Math.Min(cols, GridSize.Width);

            for (int y = 0; y < rowsToCopy; y++)
                Array.Copy(Cells, y * GridSize.Width, newCells, y * cols, colsToCopy);

            Cells = newCells;
            */

            if (cols > _columnsMaxLength)
                throw new IndexOutOfRangeException();

            gridSize = new Size(cols, rows);
            version += 1;
        }
    }

    public void Dispose()
    {
        if (disposed || disposing)
            return;

        disposing = true;
        version = 0;
        dirtyLinesCount = 0;
        _rows.Clear();

        GC.SuppressFinalize(this);
        disposed = true;
    }

    /*
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<Span<TerminalCellInfo>> GetEnumerator() => new TerminalScreenBufferEnumerator(this);

    private class TerminalScreenBufferEnumerator(TerminalScreenBuffer buffer) : IEnumerator<Span<TerminalCellInfo>>
    {
        private int version = buffer.version;
        private int position = -1;

        object IEnumerator.Current => throw new NotImplementedException();
        
        public Span<TerminalCellInfo> Current
        {
            get
            {
                if (version != buffer.version)
                    throw new InvalidOperationException();

                return buffer.Cells.AsSpan(position, buffer.GridSize.Width);
            }
        }

        public void Reset()
        {
            position = 0;
        }

        public bool MoveNext()
        {
            if (version != buffer.version)
                throw new InvalidOperationException();

            position += position == -1 ? 1 : buffer.GridSize.Width;
            return position >= buffer.Cells.Length;
        }

        public void Dispose()
        {

        }
    }
    */
}
