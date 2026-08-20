namespace ZaggyCode.Avalonia.Views.Controls;

internal readonly record struct CellOffset(int ColumnDelta, int RowDelta);

internal readonly record struct MapSize(int Columns, int Rows);

internal readonly record struct CellWalls(bool Top, bool Bottom, bool Left, bool Right);

internal readonly record struct PixelOffset(double X, double Y);

internal readonly record struct RenderPosition(double Column, double Row);

internal readonly record struct MapLayout(PixelOffset Offset, double CellSize);

internal readonly record struct Cell(int Column, int Row)
{
    public static Cell operator +(Cell cell, CellOffset offset) =>
        new(cell.Column + offset.ColumnDelta, cell.Row + offset.RowDelta);
}
