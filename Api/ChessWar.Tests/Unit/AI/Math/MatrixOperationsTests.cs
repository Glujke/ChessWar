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
        // Arrange
        var matrix = new double[,]
        {
            { 1.0, 2.0, 3.0 },
            { 4.0, 5.0, 6.0 }
        };
        var vector = new double[] { 1.0, 2.0, 3.0 };

        // Act
        var result = MatrixOperations.MatrixVectorMultiply(matrix, vector);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(14.0, result[0], 10); // 1*1 + 2*2 + 3*3 = 14
        Assert.Equal(32.0, result[1], 10); // 4*1 + 5*2 + 6*3 = 32
    }

    [Fact]
    public void MatrixVectorMultiply_WithMismatchedDimensions_ShouldThrowArgumentException()
    {
        // Arrange
        var matrix = new double[,]
        {
            { 1.0, 2.0 },
            { 3.0, 4.0 }
        };
        var vector = new double[] { 1.0, 2.0, 3.0 }; // Неправильный размер

        // Act & Assert
        Assert.Throws<ArgumentException>(() => MatrixOperations.MatrixVectorMultiply(matrix, vector));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void CreateIdentityMatrix_WithVariousSizes_ShouldCreateCorrectMatrix(int size)
    {
        // Act
        var result = MatrixOperations.CreateIdentityMatrix(size);

        // Assert
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
        // Arrange
        var matrix = new double[,]
        {
            { 1.0, 2.0, 3.0 },
            { 4.0, 5.0, 6.0 },
            { 0.0, 0.0, 0.0 }
        };

        // Act
        var result = MatrixOperations.NormalizeRows(matrix);

        // Assert
        Assert.Equal(3, result.GetLength(0));
        Assert.Equal(3, result.GetLength(1));
        
        // Первая строка: [1,2,3] -> [1/6, 2/6, 3/6]
        Assert.Equal(1.0/6.0, result[0, 0], 10);
        Assert.Equal(2.0/6.0, result[0, 1], 10);
        Assert.Equal(3.0/6.0, result[0, 2], 10);
        
        // Вторая строка: [4,5,6] -> [4/15, 5/15, 6/15]
        Assert.Equal(4.0/15.0, result[1, 0], 10);
        Assert.Equal(5.0/15.0, result[1, 1], 10);
        Assert.Equal(6.0/15.0, result[1, 2], 10);
        
        // Третья строка: [0,0,0] -> [1/3, 1/3, 1/3] (равномерное распределение)
        Assert.Equal(1.0/3.0, result[2, 0], 10);
        Assert.Equal(1.0/3.0, result[2, 1], 10);
        Assert.Equal(1.0/3.0, result[2, 2], 10);
    }

    [Fact]
    public void NormalizeRows_WithZeroRows_ShouldHandleGracefully()
    {
        // Arrange
        var matrix = new double[0, 0];

        // Act
        var result = MatrixOperations.NormalizeRows(matrix);

        // Assert
        Assert.Equal(0, result.GetLength(0));
        Assert.Equal(0, result.GetLength(1));
    }

    [Fact]
    public void FindMax_WithValidMatrix_ShouldFindMaximumValue()
    {
        // Arrange
        var matrix = new double[,]
        {
            { 1.0, 5.0, 3.0 },
            { 2.0, 1.0, 4.0 },
            { 3.0, 2.0, 1.0 }
        };

        // Act
        var result = MatrixOperations.FindMax(matrix);

        // Assert
        Assert.Equal(5.0, result);
    }

    [Fact]
    public void FindMax_WithNegativeValues_ShouldFindMaximumValue()
    {
        // Arrange
        var matrix = new double[,]
        {
            { -5.0, -1.0, -3.0 },
            { -2.0, -4.0, -1.0 }
        };

        // Act
        var result = MatrixOperations.FindMax(matrix);

        // Assert
        Assert.Equal(-1.0, result);
    }

    [Fact]
    public void FindMin_WithValidMatrix_ShouldFindMinimumValue()
    {
        // Arrange
        var matrix = new double[,]
        {
            { 5.0, 1.0, 3.0 },
            { 2.0, 4.0, 1.0 },
            { 3.0, 2.0, 5.0 }
        };

        // Act
        var result = MatrixOperations.FindMin(matrix);

        // Assert
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void FindMin_WithNegativeValues_ShouldFindMinimumValue()
    {
        // Arrange
        var matrix = new double[,]
        {
            { -1.0, -5.0, -3.0 },
            { -2.0, -1.0, -4.0 }
        };

        // Act
        var result = MatrixOperations.FindMin(matrix);

        // Assert
        Assert.Equal(-5.0, result);
    }

    [Fact]
    public void FindMin_WithEmptyMatrix_ShouldReturnMaxValue()
    {
        // Arrange
        var matrix = new double[0, 0];

        // Act
        var result = MatrixOperations.FindMin(matrix);

        // Assert
        Assert.Equal(double.MaxValue, result);
    }

    [Fact]
    public void FindMax_WithEmptyMatrix_ShouldReturnMinValue()
    {
        // Arrange
        var matrix = new double[0, 0];

        // Act
        var result = MatrixOperations.FindMax(matrix);

        // Assert
        Assert.Equal(double.MinValue, result);
    }
}
