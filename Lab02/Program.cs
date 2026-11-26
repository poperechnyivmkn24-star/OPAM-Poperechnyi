using System.Text;

Console.OutputEncoding = Encoding.UTF8;
const int number = 19, letter = 3;
var random = new Random(number);
var data = new int[18];
for (int i = 0; i < data.Length; i++)
    data[i] = random.Next(-3, 4);
Show(data, letter % 4 + 2);
var matrix = new int[6, 3];
for (int row = 0; row < matrix.GetLength(0); row++)
{
    for (int col = 0; col < matrix.GetLength(1); col++)
    {
        matrix[row, col] = random.Next(-3, 4);
        Console.Write($"{matrix[row, col],4}");
    }
    Console.WriteLine();
}
var (sums, maxima, bestRow) = AnalyzeMatrix(matrix);
Console.WriteLine($"Суми: {Format(sums)}. Максимуми стовпців: {Format(maxima)}. Найкращий індекс рядка: {bestRow}.");
Show([], 5);
Show([5], 5);
Show([2, 2, 2], 5);
Show([2, 1, 1, 1], 5);

static (long Sum, double? Average, int MinIndex, int MaxIndex, int Zeros) Describe(int[] data)
{
    if (data.Length == 0)
        return (0, null, -1, -1, 0);

    long sum = 0;
    int minIndex = 0, maxIndex = 0, zeros = 0;
    for (int i = 0; i < data.Length; i++)
    {
        int value = data[i];
        sum += value;
        if (value < data[minIndex]) minIndex = i;
        if (value > data[maxIndex]) maxIndex = i;
        if (value == 0) zeros++;
    }
    return (sum, (double)sum / data.Length, minIndex, maxIndex, zeros);
}

static int[] Filter(int[] data, int divisor)
{
    int count = 0;
    foreach (int value in data)
        if (value % divisor == 0) count++;

    var result = new int[count];
    int index = 0;
    foreach (int value in data)
        if (value % divisor == 0) result[index++] = value;
    return result;
}

static (int Value, int Length, int Start)? LongestRun(int[] data)
{
    if (data.Length == 0)
        return null;
    int start = 0, bestStart = 0, bestLength = 1;
    for (int i = 1; i < data.Length; i++)
    {
        if (data[i] != data[i - 1]) start = i;
        int length = i - start + 1;
        if (length > bestLength)
        {
            bestStart = start;
            bestLength = length;
        }
    }
    return (data[bestStart], bestLength, bestStart);
}

static void SwapHalves(int[] data)
{
    int half = data.Length / 2;
    int right = data.Length - half;
    for (int i = 0; i < half; i++)
        (data[i], data[right + i]) = (data[right + i], data[i]);
}

static (long[] Sums, int[] Maxima, int BestRow) AnalyzeMatrix(int[,] data)
{
    var sums = new long[data.GetLength(0)];
    var maxima = new int[data.GetLength(1)];
    int best = sums.Length == 0 ? -1 : 0;
    for (int row = 0; row < sums.Length; row++)
    {
        for (int col = 0; col < maxima.Length; col++)
        {
            int value = data[row, col];
            sums[row] += value;
            if (row == 0 || value > maxima[col]) maxima[col] = value;
        }
        if (sums[row] > sums[best]) best = row;
    }
    return (sums, maxima, best);
}

static string Format<T>(T[] values) => $"[{string.Join(", ", values)}]";

static void Show(int[] data, int divisor)
{
    Console.WriteLine($"Вхід: {Format(data)}");
    var s = Describe(data);
    Console.WriteLine(s.Average is null ? "Агрегація: немає даних" :
        $"Сума {s.Sum}, середнє {s.Average:F2}, мінімум {data[s.MinIndex]} [{s.MinIndex}], максимум {data[s.MaxIndex]} [{s.MaxIndex}], нулів {s.Zeros}");
    Console.WriteLine($"Кратні {divisor}: {Format(Filter(data, divisor))}");
    Console.WriteLine($"Найдовша серія (значення, довжина, початок): {LongestRun(data)?.ToString() ?? "немає даних"}");
    SwapHalves(data);
    Console.WriteLine($"Після обміну половин: {Format(data)}");
}
