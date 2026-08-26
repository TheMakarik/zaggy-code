namespace ZaggyCode.Avalonia.Views.Records;

public readonly record struct Cell(int Column, int Row)
{
    public static Cell operator +(Cell cell, CellOffset offset) =>
        new(cell.Column + offset.ColumnDelta, cell.Row + offset.RowDelta);
}
