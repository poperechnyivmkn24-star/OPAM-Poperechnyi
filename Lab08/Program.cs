using System;

public static class Program
{
    public static readonly Func<string, string, int> Cmp = (x, y) => x.CompareTo(y);
    public static void Main()
    {
        var r = new Report(); r.Line("# Результати лабораторної 8"); r.EnvironmentInfo();
        var unsorted = Codes(100000); var a = (string[])unsorted.Clone(); Array.Sort(a, (x, y) => Cmp(x, y));
        Queries(r, a); Failures(r, a, unsorted); Boundaries(r, a); Edges(r); Measurements.Run(r); r.Save("Results.md");
    }
    private static void Queries(Report r, string[] a)
    {
        r.Line("## 1, 2, 4. Пошуки");
        r.Line("| Запит | Лінійний індекс / порівняння | Бінарний | Інтерполяційний |"); r.Line("|---|---|---|---|");
        foreach (string x in new[] { "В-00", a[0], "Я-99" })
        {
            int l = Search.LinearSearch(a, x, Cmp, out int lc), b = Search.BinarySearch(a, x, Cmp, out int bc);
            int rec = Interpolated(a, x, Cmp, out int rc);
            Bench.Check((l < 0) == (b < 0) && (b < 0) == (rec < 0), "Пошуки не узгоджені");
            r.Line($"| {x} | {l} / {lc} | {b} / {bc} | {rec} / {rc} |");
        }
        r.Line("Відсутній код вимагає повного лінійного проходу. Інтерполяційний крок допомагає за рівномірного розподілу ключів, а за нерівномірного далі працює звичайний бінарний пошук.");
    }

    public const string Letters = "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮ";
    public static string Code(Random random) => $"{Letters[random.Next(Letters.Length)]}-{random.Next(100):D2}";

    public static string[] Codes(int count)
    {
        var result = new string[count];
        var random = new Random(19);
        for (int i = 0; i < result.Length; i++)
            result[i] = Code(random);
        return result;
    }

    private static int Interpolated(string[] values, string target, Func<string, string, int> comparison, out int comparisons) =>
        Search.Interpolated(values, target, comparison, value => value[0], out comparisons);
    private static void Failures(Report r, string[] a, string[] unsorted)
    {
        r.Line("## Чотири поломки");
        int bad = Broken.StrictLoop(new[] { 7 }, 7, (x, y) => x.CompareTo(y), out int comparisons);
        Bench.Check(bad == -1, "Очікувана поломка <="); r.Line($"1. [7], шукаємо 7: {bad}, порівнянь {comparisons}. Останній кандидат не перевірений.");
        bad = Broken.Stuck(new[] { 1, 2 }, 2, (x, y) => x.CompareTo(y), out comparisons, out int lo, out int hi);
        Bench.Check(comparisons == 100 && lo == 0 && hi == 1, "Очікуване зависання");
        r.Line($"2. [1,2], шукаємо 2: зупинено після {comparisons} кроків; lo={lo}, hi={hi}, результат {bad}. Без обмежувача — нескінченний цикл.");
        int index = Broken.UnsafeMidpoint(a, "В-00", Cmp, out comparisons);
        int largeLo = 1073741824, largeHi = 1073741824;
        r.Line($"3. На 100000 записах індекс={index}, порівнянь={comparisons}. Без великого масиву: lo=hi={largeLo}, unchecked((lo+hi)/2)={unchecked((largeLo + largeHi) / 2)}.");
        Bench.Check(index >= 0 && a[index] == "В-00", "Мала unsafe версія");
        int misses = 0; var random = new Random(119);
        for (int i = 0; i < 100; i++)
        {
            string target = unsorted[random.Next(unsorted.Length)];
            if (Search.BinarySearch(unsorted, target, Cmp, out _) < 0) misses++;
        }
        r.Line($"4. Невідсортовані записи: з 100 запитів гарантовано наявних кодів не знайдено {misses}."); r.Line();
    }
    private static void Boundaries(Report r, string[] a)
    {
        r.Line("## 3. Межі");
        const string x = "В-00", y = "В-99";
        int lower = Search.Boundary(a, value => Cmp(value, x) >= 0, out int lowerChecks);
        int upper = Search.Boundary(a, value => Cmp(value, x) > 0, out int upperChecks);
        int right = Search.Boundary(a, value => Cmp(value, y) > 0, out int rangeChecks);
        int equal = 0, inRange = 0;
        foreach (string value in a)
        {
            int cmp = Cmp(value, x);
            if (cmp == 0) equal++;
            if (cmp >= 0 && Cmp(value, y) <= 0) inRange++;
        }
        Bench.Check(upper - lower == equal && right - lower == inRange, "Межі");
        r.Line($"X: [{lower}, {upper}), кількість {equal}, порівнянь {lowerChecks + upperChecks}. Діапазон [X; Y]: {inRange}, порівнянь {lowerChecks + rangeChecks}.");
        r.Line($"Лінійна перевірка читає всі {a.Length} елементів. Кожен пошук межі ділить діапазон індексів навпіл незалежно від довжини блоку однакових значень.");
        int wrong = Search.Boundary(a, HasSeven, out int checks);
        int expected = 0;
        while (expected < a.Length && !HasSeven(a[expected])) expected++;
        r.Line($"Перший код із цифрою 7: межа={wrong}, лінійно={expected}, збіг={wrong == expected}, перевірок={checks}. Умова немонотонна.");
    }

    private static bool HasSeven(string value) => value[2] == '7' || value[3] == '7';
    private static void Edges(Report r)
    {
        r.Line("## Крайові випадки"); r.Line("| Вхід / запит | Очікувано: Binary, Lower, Upper | Отримано |"); r.Line("|---|---|---|");
        Edge(r, Array.Empty<int>(), 1, -1, 0, 0);
        Edge(r, new[] { 5 }, 5, 0, 0, 1);
        Edge(r, new[] { 1, 2 }, 2, 1, 1, 2);
        Edge(r, new[] { 3, 4 }, 1, -1, 0, 0);
        Edge(r, new[] { 3, 4 }, 5, -1, 2, 2);
        Edge(r, new[] { 7, 7, 7 }, 7, 1, 0, 3); r.Line();
    }
    private static void Edge(Report r, int[] a, int x, int eb, int el, int eu)
    {
        int b = Search.BinarySearch(a, x, (p, q) => p.CompareTo(q), out _);
        int l = Search.Boundary(a, v => v >= x, out _), u = Search.Boundary(a, v => v > x, out _);
        int rec = Search.RecursiveSearch(a, x, (p, q) => p.CompareTo(q), out _);
        Bench.Check(b == eb && l == el && u == eu && rec == b, "Крайовий пошук");
        r.Line($"| [{string.Join(",", a)}], X={x} | {eb}, {el}, {eu} | {b}, {l}, {u}; рекурсивний {rec}, OK |");
    }
}
