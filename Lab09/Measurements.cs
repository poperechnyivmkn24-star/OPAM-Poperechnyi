using System;
using System.Diagnostics;

public static class Measurements
{
    public static void Run(Report r)
    {
        r.Line("## 5. Час на випадковому вході, мс");
        r.Line("Копіювання вхідного масиву й обидві перевірки поза таймером; виділення робочого буфера сортування входить у час. Кожний результат перевірено, включно з прогрівом.");
        r.Line("| n | Вставки | Швидке | Два ключі (один / два проходи) | Array.Sort |"); r.Line("|---:|---:|---:|---:|---:|");
        foreach (int n in new[] { 1000, 10000, 100000, 1000000 })
        {
            Console.WriteLine($"Вимірювання n={n}...");
            var source = Program.Input(n, 0);
            double bubble = n <= 100000 ? Time(source, Program.Algorithms[0]) : 0;
            double merge = Time(source, Program.Algorithms[1]); var keys = Program.Records(n); double one = TimeKeys(keys, false), two = TimeKeys(keys, true);
            double standard = Time(source, Standard);
            r.Line($"| {n} | {(n <= 100000 ? Bench.F(bubble) : "—")} | {Bench.F(merge)} | {Bench.F(one)} / {Bench.F(two)} | {Bench.F(standard)} |");
        }
        r.Line(); r.Line("## Межа однієї секунди");
        r.Line("Межа визначається для випадкового входу Random(19) з кроком 1000: перший розмір на цій сітці з мінімальним часом трьох запусків >1000 мс. Це експериментальна межа на цьому комп'ютері, а не властивість для кожного можливого масиву.");
        int low = 1000, high = 2000;
        double lowTime = Time(Program.Input(low, 0), Program.Algorithms[0]);
        double highTime = Time(Program.Input(high, 0), Program.Algorithms[0]);
        while (highTime <= 1000)
        {
            low = high;
            lowTime = highTime;
            high *= 2;
            highTime = Time(Program.Input(high, 0), Program.Algorithms[0]);
        }

        while (high - low > 1000)
        {
            int middle = ((low + high) / 2000) * 1000;
            if (middle <= low) middle = low + 1000;
            double time = Time(Program.Input(middle, 0), Program.Algorithms[0]);
            if (time > 1000)
                (high, highTime) = (middle, time);
            else
                (low, lowTime) = (middle, time);
        }
        r.Line($"Останній перевірений нижче межі: n={low}, {Bench.F(lowTime)} мс. Сусідній перевірений вище: n={high}, {Bench.F(highTime)} мс. Шум секундоміра може змінити сусідні точки.");
    }
    private static double TimeKeys(Student[] original, bool twoPass)
    {
        double best = double.MaxValue;
        for (int run = -1; run < 3; run++)
        {
            var values = (Student[])original.Clone();
            var timer = Stopwatch.StartNew();
            if (twoPass) Program.TwoPass(values, out _, out _);
            else Sorts.MergeBottomUp(values, Program.Compound, out _, out _);
            timer.Stop();
            Checks.Verify(original, values, Program.Compound);
            if (run >= 0)
                best = Math.Min(best, timer.Elapsed.TotalMilliseconds);
        }
        return best;
    }

    private static void Standard(int[] a, Func<int, int, int> cmp, out long comparisons, out long moves)
    { Array.Sort(a); comparisons = moves = 0; }
    private static double Time(int[] original, SortAlgorithm<int> sort)
    {
        double best = double.MaxValue;
        for (int run = -1; run < 3; run++)
        {
            var a = (int[])original.Clone(); var sw = Stopwatch.StartNew();
            sort(a, Program.Cmp, out _, out _); sw.Stop(); Checks.Verify(original, a, Program.Cmp);
            if (run >= 0) best = Math.Min(best, sw.Elapsed.TotalMilliseconds);
        }
        return best;
    }
}
