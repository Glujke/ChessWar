namespace ChessWar.Domain.Services.AI.Math;

/// <summary>
/// Операции с матрицами для марковских цепей
/// </summary>
public static class MatrixOperations
{
    /// <summary>
    /// Умножение матрицы на вектор
    /// </summary>
    public static double[] MatrixVectorMultiply(double[,] matrix, double[] vector)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        
        if (cols != vector.Length)
            throw new ArgumentException("Matrix columns must match vector length");
        
        var result = new double[rows];
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Создать единичную матрицу
    /// </summary>
    public static double[,] CreateIdentityMatrix(int size)
    {
        var matrix = new double[size, size];
        
        for (int i = 0; i < size; i++)
        {
            matrix[i, i] = 1.0;
        }
        
        return matrix;
    }
    
    /// <summary>
    /// Нормализовать строки матрицы (сумма каждой строки = 1)
    /// </summary>
    public static double[,] NormalizeRows(double[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var normalized = new double[rows, cols];
        
        for (int i = 0; i < rows; i++)
        {
            var rowSum = 0.0;
            for (int j = 0; j < cols; j++)
            {
                rowSum += matrix[i, j];
            }
            
            if (rowSum > 0)
            {
                for (int j = 0; j < cols; j++)
                {
                    normalized[i, j] = matrix[i, j] / rowSum;
                }
            }
            else
            {
                for (int j = 0; j < cols; j++)
                {
                    normalized[i, j] = 1.0 / cols;
                }
            }
        }
        
        return normalized;
    }
    
    /// <summary>
    /// Найти максимальный элемент в матрице
    /// </summary>
    public static double FindMax(double[,] matrix)
    {
        var max = double.MinValue;
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (matrix[i, j] > max)
                    max = matrix[i, j];
            }
        }
        
        return max;
    }
    
    /// <summary>
    /// Найти минимальный элемент в матрице
    /// </summary>
    public static double FindMin(double[,] matrix)
    {
        var min = double.MaxValue;
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (matrix[i, j] < min)
                    min = matrix[i, j];
            }
        }
        
        return min;
    }
}
