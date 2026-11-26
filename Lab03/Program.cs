using System.Text;

Console.OutputEncoding = Encoding.UTF8;
const int number = 19, letter = 3;
Console.WriteLine("Вкажіть каталог або натисніть Enter для поточного:");
string input = Console.ReadLine() ?? "";
string path = input.Length == 0 ? "." : input;
Console.WriteLine(Directory.Exists(path) ? $"Файли C# (кількість, байти): {Scan(path)}" : "Каталог не існує.");

var random = new Random(number);
var values = new int[16];
for (int i = 0; i < values.Length; i++)
    values[i] = random.Next(-20, 21);
Console.WriteLine(string.Join(", ", values));
var recursive = Solve(values, 0, values.Length - 1, letter % 10);
int iterative = CountLoop(values, letter % 10);
Console.WriteLine($"Підрахунок: {recursive.Count}; цикл: {iterative}; збіг: {recursive.Count == iterative}; глибина: {recursive.Depth}, log₂(16) = 4.");

int coins = letter % 10 + 8;
Console.WriteLine($"Розмін {coins} грн у форматі (1 грн, 2 грн, 5 грн):");
Console.WriteLine($"Способів: {Change([1, 2, 5], new int[3], 0, coins, Console.Out)}");
Solve([7, -2, 15, 4], 0, 3, letter % 10, trace: Console.Out);

Console.WriteLine($"Рівні елементи [4,4,4,4]: {Solve([4, 4, 4, 4], 0, 3, 3).Count}.");
Console.WriteLine($"Немає розміну: {Change([2, 5], new int[2], 0, 1, TextWriter.Null)}.");

static (int Count, long Bytes) Scan(string path)
{
    try
    {
        int count = 0;
        long bytes = 0;
        foreach (string file in Directory.EnumerateFiles(path))
        {
            if (!Path.GetExtension(file).Equals(".cs", StringComparison.OrdinalIgnoreCase))
                continue;
            try
            {
                bytes += new FileInfo(file).Length;
                count++;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Пропущено файл: {file}");
            }
        }

        foreach (string directory in Directory.EnumerateDirectories(path))
        {
            if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                continue;
            var child = Scan(directory);
            count += child.Count;
            bytes += child.Bytes;
        }
        return (count, bytes);
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine($"Пропущено каталог: {path}");
        return (0, 0);
    }
}

static (int Count, int Depth) Solve(int[] data, int lo, int hi, int threshold, int depth = 0, TextWriter? trace = null)
{
    string indent = new(' ', depth * 2);
    if (lo > hi)
        return (0, depth);
    trace?.WriteLine($"{indent}enter Solve({lo}..{hi})");
    int count, deepest;
    if (lo == hi)
    {
        count = data[lo] > threshold ? 1 : 0;
        deepest = depth;
    }
    else
    {
        int mid = lo + (hi - lo) / 2;
        var left = Solve(data, lo, mid, threshold, depth + 1, trace);
        var right = Solve(data, mid + 1, hi, threshold, depth + 1, trace);
        count = left.Count + right.Count;
        deepest = Math.Max(left.Depth, right.Depth);
    }
    trace?.WriteLine($"{indent}exit  Solve({lo}..{hi}) -> {count}");
    return (count, deepest);
}

static int CountLoop(int[] data, int threshold)
{
    int count = 0;
    foreach (int value in data)
        if (value > threshold)
            count++;
    return count;
}

static int Change(int[] values, int[] counts, int index, int remaining, TextWriter output)
{
    if (remaining < 0)
        return 0;
    if (index == values.Length)
    {
        if (remaining != 0)
            return 0;
        output.WriteLine(string.Join(", ", counts));
        return 1;
    }

    int solutions = 0;
    for (int count = 0; count <= remaining / values[index]; count++)
    {
        counts[index] = count;
        solutions += Change(values, counts, index + 1, remaining - count * values[index], output);
        counts[index] = 0;
    }
    return solutions;
}
