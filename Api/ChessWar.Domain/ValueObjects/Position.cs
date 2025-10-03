namespace ChessWar.Domain.ValueObjects;

public record Position(int X, int Y)
{
    public int DistanceTo(Position other) =>
        Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

    public bool IsAdjacent(Position other) => DistanceTo(other) == 1;

    public bool IsOnSameDiagonal(Position other) =>
        Math.Abs(X - other.X) == Math.Abs(Y - other.Y);

    public bool IsOnSameRow(Position other) => Y == other.Y;

    public bool IsOnSameColumn(Position other) => X == other.X;
}
