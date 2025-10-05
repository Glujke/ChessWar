using ChessWar.Domain.Services.AI.Math;

namespace ChessWar.Tests.Unit.AI.Math;

/// <summary>
/// Тесты для операций с матрицами
/// </summary>
public class MatrixOperationsTests
{
    [Fact]
    public void MatrixVectorMultiply_WithValidMatrixAndVector_ShouldCalculateCorrectly()
    {
        var matrix = new double[,]
        {
            { 1.0, 2.0, 3.0 },
            { 4.0, 5.0, 6.0 }
        };
        var vector = new double[] { 1.0, 2.0, 3.0 };

        var result = MatrixOperations.MatrixVectorMultiply(matrix, vector);

        Assert.Equal(2, result.Length);
        Assert.Equal(14.0, result[0], 10);
        Assert.Equal(32.0, result[1], 10);
    }

    [Fact]
    public void MatrixVectorMultiply_WithMismatchedDimensions_ShouldThrowArgumentException()
    {
        var matrix = new double[,]
        {
            { 1.0, 2.0 },
            { 3.0, 4.0 }
        };
        var vector = new double[] { 1.0, 2.0, 3.0 };

        Assert.Throws<ArgumentException>(() => MatrixOperations.MatrixVectorMultiply(matrix, vector));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void CreateIdentityMatrix_WithVariousSizes_ShouldCreateCorrectMatrix(int size)
    {
        var result = MatrixOperations.CreateIdentityMatrix(size);

        Assert.Equal(size, result.GetLength(0));
        Assert.Equal(size, result.GetLength(1));

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (i == j)
                    Assert.Equal(1.0, result[i, j]);
                else
                    Assert.Equal(0.0, result[i, j]);
            }
        }
    }

    [Fact]
    public void NormalizeRows_WithValidMatrix_ShouldNormalizeEachRow()
    {
        var matrix = new double[,]
        {
            { 1.0, 2.0, 3.0 },
            { 4.0, 5.0, 6.0 },
            { 0.0, 0.0, 0.0 }
        };

        var result = MatrixOperations.NormalizeRows(matrix);

        Assert.Equal(3, result.GetLength(0));
        Assert.Equal(3, result.GetLength(1));

        Assert.Equal(1.0 / 6.0, result[0, 0], 10);
        Assert.Equal(2.0 / 6.0, result[0, 1], 10);
        Assert.Equal(3.0 / 6.0, result[0, 2], 10);

        Assert.Equal(4.0 / 15.0, result[1, 0], 10);
        Assert.Equal(5.0 / 15.0, result[1, 1], 10);
        Assert.Equal(6.0 / 15.0, result[1, 2], 10);

        Assert.Equal(1.0 / 3.0, result[2, 0], 10);
        Assert.Equal(1.0 / 3.0, result[2, 1], 10);
        Assert.Equal(1.0 / 3.0, result[2, 2], 10);
    }

    [Fact]
    public void NormalizeRows_WithZeroRows_ShouldHandleGracefully()
    {
        var matrix = new double[0, 0];

        var result = MatrixOperations.NormalizeRows(matrix);

        Assert.Equal(0, result.GetLength(0));
        Assert.Equal(0, result.GetLength(1));
    }

    [Fact]
    public void FindMax_WithValidMatrix_ShouldFindMaximumValue()
    {
        var matrix = new double[,]
        {
            { 1.0, 5.0, 3.0 },
            { 2.0, 1.0, 4.0 },
            { 3.0, 2.0, 1.0 }
        };

        var result = MatrixOperations.FindMax(matrix);

        Assert.Equal(5.0, result);
    }

    [Fact]
    public void FindMax_WithNegativeValues_ShouldFindMaximumValue()
    {
        var matrix = new double[,]
        {
            { -5.0, -1.0, -3.0 },
            { -2.0, -4.0, -1.0 }
        };

        var result = MatrixOperations.FindMax(matrix);

        Assert.Equal(-1.0, result);
    }

    [Fact]
    public void FindMin_WithValidMatrix_ShouldFindMinimumValue()
    {
        var matrix = new double[,]
        {
            { 5.0, 1.0, 3.0 },
            { 2.0, 4.0, 1.0 },
            { 3.0, 2.0, 5.0 }
        };

        var result = MatrixOperations.FindMin(matrix);

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void FindMin_WithNegativeValues_ShouldFindMinimumValue()
    {
        var matrix = new double[,]
        {
            { -1.0, -5.0, -3.0 },
            { -2.0, -1.0, -4.0 }
        };

        var result = MatrixOperations.FindMin(matrix);

        Assert.Equal(-5.0, result);
    }

    [Fact]
    public void FindMin_WithEmptyMatrix_ShouldReturnMaxValue()
    {
        var matrix = new double[0, 0];

        var result = MatrixOperations.FindMin(matrix);

        Assert.Equal(double.MaxValue, result);
    }

    [Fact]
    public void FindMax_WithEmptyMatrix_ShouldReturnMinValue()
    {
        var matrix = new double[0, 0];

        var result = MatrixOperations.FindMax(matrix);

        Assert.Equal(double.MinValue, result);
    }
}
