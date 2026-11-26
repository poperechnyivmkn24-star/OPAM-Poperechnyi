using System;

public static class Program
{
    public static readonly Func<int, int, int> Cmp = (x, y) => x.CompareTo(y);
    public static readonly string[] Names = { "Вставки", "Швидке, медіана трьох" };
    public static readonly SortAlgorithm<int>[] Algorithms = { Sorts.Insertion, Sorts.QuickMedian };
    public static void Main()
    {
        var r = new Report(); r.Line("# Результати лабораторної 9"); r.EnvironmentInfo();
        Trace(r); TwoKeys(r); Edges(r); Counters(r); Stability.Run(r); Measurements.Run(r); r.Save("Results.md");
    }
    public static int[] Input(int n, int kind)
    {
        var a = Data.Numbers(n, 19, 0, 60);
        if (kind > 0) Array.Sort(a); // Підготовка входу поза власними сортуваннями.
        if (kind == 2)
            for (int i = 0; i < a.Length / 2; i++) { int t = a[i]; a[i] = a[a.Length - 1 - i]; a[a.Length - 1 - i] = t; }
        if (kind == 3 && a.Length > 0)
        {
            var rng = new Random(19);
            for (int i = 0; i < 3; i++) { int x = rng.Next(n), y = rng.Next(n); int t = a[x]; a[x] = a[y]; a[y] = t; }
        }
        return a;
    }
    private static void Trace(Report r)
    {
        r.Line("## 1. Трасування 9 елементів, перевірене проходами");
        var a = Data.Numbers(9, 19, 0, 60); r.Line("| Прохід | Стан | Порівняння за прохід | Записи за прохід |"); r.Line("|---|---|---:|---:|");
        r.Line($"| Вхід | {string.Join(", ", a)} | — | — |");
        for (int pass = 1; pass < a.Length; pass++)
        {
            int value = a[pass], j = pass, checks = 0, moves = 0;
            while (j > 0)
            {
                checks++;
                if (a[j - 1] <= value) break;
                a[j] = a[j - 1];
                j--;
                moves++;
            }
            if (j != pass)
            {
                a[j] = value;
                moves++;
            }
            r.Line($"| {pass} | {string.Join(", ", a)} | {checks} | {moves} |");
        }
        r.Line();
    }

    public static Student[] Records(int size)
    {
        var original = Data.Students();
        var random = new Random(19);
        var result = new Student[size];
        for (int i = 0; i < size; i++)
        {
            var item = original[random.Next(original.Count)];
            result[i] = new Student(item.Name, item.Group, random.Next(101));
        }
        return result;
    }

    public static int Compound(Student a, Student b)
    {
        int group = StringComparer.Ordinal.Compare(a.Group, b.Group);
        return group == 0 ? a.Score.CompareTo(b.Score) : group;
    }

    public static void TwoPass(Student[] values, out long comparisons, out long moves)
    {
        Sorts.MergeBottomUp(values, (x, y) => x.Score.CompareTo(y.Score), out long first, out long firstWrites);
        Sorts.MergeBottomUp(values, (x, y) => StringComparer.Ordinal.Compare(x.Group, y.Group), out long last, out long lastWrites);
        comparisons = first + last;
        moves = firstWrites + lastWrites;
    }

    private static void TwoKeys(Report r)
    {
        r.Line("## 3. Два ключі");
        var original = Data.Students().ToArray();
        var single = (Student[])original.Clone();
        var two = (Student[])original.Clone();
        Sorts.MergeBottomUp(single, Compound, out long oneChecks, out long oneWrites);
        TwoPass(two, out long twoChecks, out long twoWrites);
        Checks.Verify(original, single, Compound);
        Checks.Verify(original, two, Compound);
        for (int i = 0; i < two.Length; i++)
            Bench.Check(ReferenceEquals(single[i], two[i]), "Два ключі");
        r.Line($"Один прохід: {oneChecks}/{oneWrites}. Два проходи: {twoChecks}/{twoWrites}. Послідовності збігаються, бо останнє стабільне сортування за групою зберігає порядок за балом усередині групи.");
        r.Line();
    }
    private static void Counters(Report r)
    {
        r.Line("## 4. Порівняння / записи елементів у масиви");
        r.Line("Рахуються записи як в основний масив, так і в буфер T[]. Присвоєння локальній змінній та службові частоти не враховуються. Лічильники long, бо на n=100000 int переповнюється.");
        for (int alg = 0; alg < Algorithms.Length; alg++)
        {
            r.Line(); r.Line("### " + Names[alg]); r.Line("| n | Випадковий | Відсортований | Навпаки | Майже |"); r.Line("|---:|---|---|---|---|");
            foreach (int n in new[] { 100, 1000, 10000 })
            {
                string row = $"| {n}";
                for (int kind = 0; kind < 4; kind++)
                {
                    var original = Input(n, kind); var a = (int[])original.Clone(); Algorithms[alg](a, Cmp, out long c, out long m); Checks.Verify(original, a, Cmp);
                    if (alg == 0 && kind == 1) Bench.Check(c == n - 1 && m == 0, "Вставки на впорядкованому");
                    row += $" | {c} / {m}";
                }
                r.Line(row + " |");
            }
        }
        r.Line("### Два ключі: один / два проходи");
        r.Line("| n | Випадковий | Відсортований | Навпаки | Майже |");
        r.Line("|---:|---|---|---|---|");
        foreach (int size in new[] { 100, 1000, 10000 })
        {
            var input = Records(size);
            var cells = new string[4];
            for (int kind = 0; kind < cells.Length; kind++)
            {
                var records = (Student[])input.Clone();
                if (kind > 0)
                    Sorts.MergeBottomUp(records, Compound, out _, out _);
                if (kind == 2)
                    for (int i = 0; i < records.Length / 2; i++)
                        (records[i], records[records.Length - 1 - i]) = (records[records.Length - 1 - i], records[i]);
                if (kind == 3)
                {
                    var random = new Random(19);
                    for (int swap = 0; swap < 3; swap++)
                    {
                        int i = random.Next(size), j = random.Next(size);
                        (records[i], records[j]) = (records[j], records[i]);
                    }
                }

                var single = (Student[])records.Clone();
                var two = (Student[])records.Clone();
                Sorts.MergeBottomUp(single, Compound, out long checks, out long writes);
                TwoPass(two, out long checksTwo, out long writesTwo);
                Checks.Verify(records, single, Compound);
                Checks.Verify(records, two, Compound);
                for (int i = 0; i < size; i++)
                    Bench.Check(ReferenceEquals(single[i], two[i]), "Два ключі");
                cells[kind] = $"{checks}/{writes} · {checksTwo}/{writesTwo}";
            }
            r.Line($"| {size} | {string.Join(" | ", cells)} |");
        }
        var randomInput = Input(10000, 0); var b = (int[])randomInput.Clone(); var merge = (int[])randomInput.Clone();
        Sorts.Insertion(b, Cmp, out long bc, out _); Sorts.QuickMedian(merge, Cmp, out long mc, out _);
        r.Line(); r.Line($"Відношення порівнянь вставок до швидкого на випадкових n=10000: {Bench.F((double)bc / mc)} (лекційне 376 — інша модель і вхід)."); r.Line();
    }
    private static void Edges(Report r)
    {
        r.Line("## Крайові випадки"); r.Line("| Вхід | Очікувано | Отримано |"); r.Line("|---|---|---|");
        int[][] inputs = { Array.Empty<int>(), new[] { 5 }, new[] { 1, 2 }, new[] { 2, 1 }, new[] { 7, 7, 7, 7 }, new[] { 2, 2, 2, 1, 1, 1 } };
        foreach (var input in inputs)
        {
            foreach (var sort in Algorithms)
            { var a = (int[])input.Clone(); sort(a, Cmp, out _, out _); Checks.Verify(input, a, Cmp); }
            r.Line($"| [{string.Join(",", input)}] | IsSorted і IsPermutationOf = true | вставки і швидке: true / true, OK |");
        }
        // Додаткові розміри перевіряють неповні хвости злиття.
        for (int n = 0; n <= 257; n++)
        {
            var original = Input(n, 0);
            foreach (var sort in Algorithms) { var a = (int[])original.Clone(); sort(a, Cmp, out _, out _); Checks.Verify(original, a, Cmp); }
        }
        var equal = new int[10000];
        Sorts.QuickMedian(equal, Cmp, out long c, out long m);
        r.Line($"10000 рівних елементів: {c} порівнянь, {m} записів. Три частини залишають усі рівні опорному поза наступними викликами.");
        r.Line("Додатково перевірено масиви всіх довжин 0–257."); r.Line();
    }
}
