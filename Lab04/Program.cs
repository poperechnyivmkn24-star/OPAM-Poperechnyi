using System.Diagnostics;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
const int n = 19, k = 3;
var random = new Random(n);
var values = new List<int>(20);
for (int i = 0; i < 20; i++)
    values.Add(random.Next(-9, 10));
Show(values, k % 4 + 2);

var left = Increasing(10 + k % 5, n);
var right = Increasing(6 + k % 4, n + 100);
ShowMerge(left, right);
ShowMerge([], right);

Console.WriteLine("Крайові випадки:");
Show([], 5);
Show([5], 5);
Show([1, 2], 5);
Show([5, 10], 5);

double add = Measure(m => AddItems(m, false));
double reserved = Measure(m => AddItems(m, true));
double insert = Measure(InsertItems);
double contains = MeasureContains();
Console.WriteLine($"Add: {add:F4} мс; Add із місткістю: {reserved:F4} мс; Insert: {insert:F4} мс; Contains: {contains:F4} мс; Insert/Add: {insert / add:F2}.");

static List<int> SelectMultiples(List<int> data, int divisor)
{
    var result = new List<int>();
    foreach (int value in data)
        if (value % divisor == 0)
            result.Add(value);
    return result;
}

static double Average(List<int> data)
{
    if (data.Count == 0)
        return 0;
    long sum = 0;
    foreach (int value in data)
        sum += value;
    return (double)sum / data.Count;
}

static void RemoveForwardWrong(List<int> data, double threshold)
{
    for (int i = 0; i < data.Count; i++)
        if (data[i] < threshold)
            data.RemoveAt(i);
}

static void RemoveBackward(List<int> data, double threshold)
{
    for (int i = data.Count - 1; i >= 0; i--)
        if (data[i] < threshold)
            data.RemoveAt(i);
}

static List<int> KeepAtLeast(List<int> data, double threshold)
{
    var result = new List<int>();
    foreach (int value in data)
        if (value >= threshold)
            result.Add(value);
    return result;
}

static bool Same(List<int> first, List<int> second)
{
    if (first.Count != second.Count)
        return false;
    for (int i = 0; i < first.Count; i++)
        if (first[i] != second[i])
            return false;
    return true;
}

static List<int> Increasing(int length, int seed)
{
    var result = new List<int>(length);
    var random = new Random(seed);
    int value = 0;
    for (int i = 0; i < length; i++)
    {
        value += random.Next(1, 10);
        result.Add(value);
    }
    return result;
}

static List<int> Merge(List<int> first, List<int> second)
{
    var result = new List<int>(first.Count + second.Count);
    int i = 0, j = 0;
    while (i < first.Count && j < second.Count)
        result.Add(first[i] <= second[j] ? first[i++] : second[j++]);
    while (i < first.Count)
        result.Add(first[i++]);
    while (j < second.Count)
        result.Add(second[j++]);
    return result;
}

static bool Ordered(List<int> data)
{
    for (int i = 1; i < data.Count; i++)
        if (data[i] < data[i - 1])
            return false;
    return true;
}

static (List<int> Even, List<int> Odd) SplitByIndex(List<int> data)
{
    var even = new List<int>();
    var odd = new List<int>();
    for (int i = 0; i < data.Count; i++)
        (i % 2 == 0 ? even : odd).Add(data[i]);
    return (even, odd);
}

static void Show(List<int> data, int divisor)
{
    Console.WriteLine($"Вхід: [{string.Join(", ", data)}]");
    var selected = SelectMultiples(data, divisor);
    Console.WriteLine($"Кратні {divisor}: [{string.Join(", ", selected)}]; кількість {selected.Count}.");
    double threshold = Average(data);
    var forward = new List<int>(data);
    var backward = new List<int>(data);
    RemoveForwardWrong(forward, threshold);
    RemoveBackward(backward, threshold);
    var copy = KeepAtLeast(data, threshold);
    var skipped = new List<int>();
    foreach (int value in forward)
        if (value < threshold)
            skipped.Add(value);
    Console.WriteLine($"Середнє {threshold:F2}; хибний [{string.Join(", ", forward)}]; пропустив [{string.Join(", ", skipped)}].");
    Console.WriteLine($"Зворотний [{string.Join(", ", backward)}]; новий [{string.Join(", ", copy)}]; збіг: {Same(backward, copy)}.");
    var (even, odd) = SplitByIndex(data);
    Console.WriteLine($"Парні індекси [{string.Join(", ", even)}]; непарні [{string.Join(", ", odd)}]; кількість збігається: {even.Count + odd.Count == data.Count}.");
}

static void ShowMerge(List<int> first, List<int> second)
{
    var merged = Merge(first, second);
    Console.WriteLine($"Злиття {first.Count} + {second.Count}: [{string.Join(", ", merged)}]; впорядковано: {Ordered(merged)}.");
}

static double Measure(Action<int> operation)
{
    operation(1_000);
    var timer = Stopwatch.StartNew();
    operation(100_000);
    return timer.Elapsed.TotalMilliseconds;
}

static void AddItems(int size, bool reserve)
{
    var data = reserve ? new List<int>(size) : new List<int>();
    for (int i = 0; i < size; i++)
        data.Add(i);
}

static void InsertItems(int size)
{
    var data = new List<int>();
    for (int i = 0; i < size; i++)
        data.Insert(0, i);
}

static double MeasureContains()
{
    var data = new List<int>(100_000);
    for (int i = 0; i < 100_000; i++)
        data.Add(i);
    _ = CountHits(data, 100);
    var timer = Stopwatch.StartNew();
    int found = CountHits(data, 1_000);
    timer.Stop();
    Console.WriteLine($"Влучань Contains: {found}.");
    return timer.Elapsed.TotalMilliseconds;
}

static int CountHits(List<int> data, int queries)
{
    int found = 0;
    for (int i = 0; i < queries; i++)
        if (data.Contains(-1))
            found++;
    return found;
}
