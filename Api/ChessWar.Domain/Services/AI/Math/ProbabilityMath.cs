namespace ChessWar.Domain.Services.AI.Math;

/// <summary>
/// Математические утилиты для работы с вероятностями
/// </summary>
public static class ProbabilityMath
{
    /// <summary>
    /// Softmax функция для нормализации вероятностей
    /// </summary>
    /// <param name="values">Значения для нормализации</param>
    /// <param name="temperature">Температура (τ) - чем меньше, тем более "острые" вероятности</param>
    /// <returns>Нормализованные вероятности</returns>
    public static double[] Softmax(double[] values, double temperature = 1.0)
    {
        if (values.Length == 0) return Array.Empty<double>();

        var maxValue = values.Max();
        var expValues = values.Select(v => System.Math.Exp((v - maxValue) / temperature)).ToArray();
        var sum = expValues.Sum();

        return expValues.Select(v => v / sum).ToArray();
    }

    /// <summary>
    /// Выбор элемента по вероятностям (roulette wheel selection)
    /// </summary>
    /// <param name="probabilities">Массив вероятностей</param>
    /// <param name="random">Генератор случайных чисел</param>
    /// <returns>Индекс выбранного элемента</returns>
    public static int SelectByProbability(double[] probabilities, Random random)
    {
        var randomValue = random.NextDouble();
        var cumulativeProbability = 0.0;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue <= cumulativeProbability)
                return i;
        }

        return probabilities.Length - 1;
    }

    /// <summary>
    /// Ограничить значение в диапазоне [min, max]
    /// </summary>
    public static double Clamp(double value, double min, double max)
    {
        return System.Math.Max(min, System.Math.Min(max, value));
    }

    /// <summary>
    /// Нормализовать массив значений в диапазон [0, 1]
    /// </summary>
    public static double[] Normalize(double[] values)
    {
        if (values.Length == 0) return Array.Empty<double>();

        var min = values.Min();
        var max = values.Max();
        var range = max - min;

        if (range == 0) return values.Select(_ => 0.5).ToArray();

        return values.Select(v => (v - min) / range).ToArray();
    }

    /// <summary>
    /// Рассчитать евклидово расстояние между двумя точками
    /// </summary>
    public static double EuclideanDistance(double x1, double y1, double x2, double y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return System.Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Рассчитать расстояние Чебышёва между двумя точками
    /// </summary>
    public static double ChebyshevDistance(double x1, double y1, double x2, double y2)
    {
        return System.Math.Max(System.Math.Abs(x2 - x1), System.Math.Abs(y2 - y1));
    }
}
