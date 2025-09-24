using ChessWar.Domain.Services.AI.Math;

namespace ChessWar.Tests.Unit.AI.Math;

/// <summary>
/// Тесты для математических утилит вероятностей
/// </summary>
public class ProbabilityMathTests
{
    [Fact]
    public void Softmax_WithEmptyArray_ShouldReturnEmptyArray()
    {
        // Arrange
        var values = new double[0];
        var temperature = 1.0;

        // Act
        var result = ProbabilityMath.Softmax(values, temperature);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Softmax_WithSingleValue_ShouldReturnOne()
    {
        // Arrange
        var values = new double[] { 5.0 };
        var temperature = 1.0;

        // Act
        var result = ProbabilityMath.Softmax(values, temperature);

        // Assert
        Assert.Single(result);
        Assert.Equal(1.0, result[0], 10);
    }

    [Fact]
    public void Softmax_WithMultipleValues_ShouldSumToOne()
    {
        // Arrange
        var values = new double[] { 1.0, 2.0, 3.0 };
        var temperature = 1.0;

        // Act
        var result = ProbabilityMath.Softmax(values, temperature);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal(1.0, result.Sum(), 10);
    }

    [Fact]
    public void Softmax_WithHighTemperature_ShouldBeMoreUniform()
    {
        // Arrange
        var values = new double[] { 1.0, 2.0, 3.0 };
        var highTemperature = 10.0;
        var lowTemperature = 0.1;

        // Act
        var highTempResult = ProbabilityMath.Softmax(values, highTemperature);
        var lowTempResult = ProbabilityMath.Softmax(values, lowTemperature);

        // Assert
        // При высокой температуре распределение более равномерное
        var highTempVariance = CalculateVariance(highTempResult);
        var lowTempVariance = CalculateVariance(lowTempResult);
        
        Assert.True(highTempVariance < lowTempVariance);
    }

    [Fact]
    public void SelectByProbability_WithValidProbabilities_ShouldSelectCorrectly()
    {
        // Arrange
        var probabilities = new double[] { 0.1, 0.3, 0.6 };
        var random = new Random(42); // Фиксированный seed для воспроизводимости

        // Act
        var selectedIndex = ProbabilityMath.SelectByProbability(probabilities, random);

        // Assert
        Assert.True(selectedIndex >= 0);
        Assert.True(selectedIndex < probabilities.Length);
    }

    [Fact]
    public void SelectByProbability_WithZeroProbabilities_ShouldNotThrow()
    {
        // Arrange
        var probabilities = new double[] { 0.0, 0.0, 0.0 };
        var random = new Random(42);

        // Act & Assert
        var selectedIndex = ProbabilityMath.SelectByProbability(probabilities, random);
        Assert.True(selectedIndex >= 0);
        Assert.True(selectedIndex < probabilities.Length);
    }

    [Theory]
    [InlineData(5.0, 0.0, 10.0, 5.0)]
    [InlineData(-5.0, 0.0, 10.0, 0.0)]
    [InlineData(15.0, 0.0, 10.0, 10.0)]
    [InlineData(5.0, 2.0, 8.0, 5.0)]
    public void Clamp_WithVariousValues_ShouldClampCorrectly(double value, double min, double max, double expected)
    {
        // Act
        var result = ProbabilityMath.Clamp(value, min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_WithEmptyArray_ShouldReturnEmptyArray()
    {
        // Arrange
        var values = new double[0];

        // Act
        var result = ProbabilityMath.Normalize(values);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Normalize_WithSingleValue_ShouldReturnHalf()
    {
        // Arrange
        var values = new double[] { 5.0 };

        // Act
        var result = ProbabilityMath.Normalize(values);

        // Assert
        Assert.Single(result);
        Assert.Equal(0.5, result[0], 10);
    }

    [Fact]
    public void Normalize_WithMultipleValues_ShouldNormalizeToZeroOne()
    {
        // Arrange
        var values = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // Act
        var result = ProbabilityMath.Normalize(values);

        // Assert
        Assert.Equal(5, result.Length);
        Assert.Equal(0.0, result[0], 10); // Минимальное значение
        Assert.Equal(1.0, result[4], 10); // Максимальное значение
    }

    [Theory]
    [InlineData(0.0, 0.0, 1.0, 1.0, 1.4142135623730951)]
    [InlineData(0.0, 0.0, 3.0, 4.0, 5.0)]
    [InlineData(1.0, 1.0, 1.0, 1.0, 0.0)]
    public void EuclideanDistance_WithVariousPoints_ShouldCalculateCorrectly(
        double x1, double y1, double x2, double y2, double expected)
    {
        // Act
        var result = ProbabilityMath.EuclideanDistance(x1, y1, x2, y2);

        // Assert
        Assert.Equal(expected, result, 10);
    }

    [Theory]
    [InlineData(0.0, 0.0, 1.0, 1.0, 1.0)]
    [InlineData(0.0, 0.0, 3.0, 4.0, 4.0)]
    [InlineData(1.0, 1.0, 1.0, 1.0, 0.0)]
    [InlineData(0.0, 0.0, 2.0, 1.0, 2.0)]
    public void ChebyshevDistance_WithVariousPoints_ShouldCalculateCorrectly(
        double x1, double y1, double x2, double y2, double expected)
    {
        // Act
        var result = ProbabilityMath.ChebyshevDistance(x1, y1, x2, y2);

        // Assert
        Assert.Equal(expected, result, 10);
    }

    private double CalculateVariance(double[] values)
    {
        if (values.Length == 0) return 0.0;
        
        var mean = values.Average();
        var variance = values.Select(v => System.Math.Pow(v - mean, 2)).Average();
        return variance;
    }
}
