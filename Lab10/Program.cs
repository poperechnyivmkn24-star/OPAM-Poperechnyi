using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("uk-UA");
const int n = 19, k = 3;
int rod = 30 + k, limit = 5 + k;
string prose = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "prose.txt"));
string reportPath = Path.GetFullPath(args.Length > 0 ? args[0] : "Results.md");
if (prose.Length < 50_000)
    throw new InvalidOperationException("Потрібно 50 000 символів прози.");
using var report = new StreamWriter(reportPath, false, new UTF8Encoding(false));
void Log(string line)
{
    Console.WriteLine(line);
    report.WriteLine(line);
}

Log("# Фактичні результати лабораторної №10");
Log("Release, Stopwatch, один прогрів і мінімум із трьох вимірювань.\n");
Measurements.Pitfalls(Log, TextAlgorithms.Words(prose[..50_000]));
Log("## 2. Найчастіші слова");
Log("| Символів | Вкладений цикл, мс | Словник, мс | Виграш |");
Log("| ---: | ---: | ---: | ---: |");
double shortNaive = 0, shortFast = 0, fullNaive = 0, fullFast = 0;
int wordCount = 0;
(string Word, int Count)[] top = [];
foreach (int length in new[] { 1_000, 10_000, 50_000 })
{
    var words = TextAlgorithms.Words(prose[..length]);
    var expected = TextAlgorithms.TopNaive(words, limit);
    top = TextAlgorithms.TopFast(words, limit);
    Check(TextAlgorithms.Same(expected, top), "Топ слів");
    double slow = Measurements.Time(() => GC.KeepAlive(TextAlgorithms.TopNaive(words, limit)));
    double fast = Measurements.Time(() => GC.KeepAlive(TextAlgorithms.TopFast(words, limit)));
    if (length == 1_000) (shortNaive, shortFast) = (slow, fast);
    if (length == 50_000) (fullNaive, fullFast, wordCount) = (slow, fast, words.Count);
    Log($"| {length} | {slow:F4} | {fast:F4} | {slow / fast:F2} |");
}
Log("\nТоп-8 на 50 000 символах: " + string.Join(", ", top));
Log("\n## 3. Пошук підрядка");
Log("| Вхід | Шаблон | Індекс | Порівняння | Власний, мс | String.IndexOf, мс |");
Log("| --- | --- | ---: | ---: | ---: | ---: |");
string sample = prose[..50_000];
const string missing = "qzxvнеіснуючийкодовиймаркер";
string present = TextAlgorithms.Words(sample[25_000..])[1];
SearchRow("Проза, є збіг", sample, present, present);
SearchRow("Проза, без збігу", sample, missing, missing);
SearchRow("Повтори", new string('В', 10_000), new string('В', 929) + 'Г', "929 × В та Г");
Log("\nНа повторах майже кожен кандидат збігається на 929 позиціях. Вбудований пошук може перевіряти блоки символів і відкидати кандидатів швидше.\n");

Log($"## 4. Стрижень {rod} см");
var random = new Random(n);
int optimal = 0, minimalSize = int.MaxValue;
double worstGap = 0;
int[] counterexample = [];
int counterGreedy = 0, counterBest = 0;
for (int test = 0; test < 100; test++)
{
    int[] parts = Cutting.Parts(random, random.Next(1, 16));
    int greedy = Cutting.Greedy(rod, parts);
    int best = Cutting.Exhaustive(rod, parts, false).Best;
    Check(best == Cutting.Exhaustive(rod, parts, true).Best, "Відсікання");
    if (greedy == best) optimal++;
    else
    {
        worstGap = Math.Max(worstGap, 100.0 * (best - greedy) / best);
        if (parts.Length < minimalSize)
        {
            minimalSize = parts.Length;
            counterexample = parts;
            counterGreedy = greedy;
            counterBest = best;
        }
    }
}
Log($"- Оптимум жадібного алгоритму: {optimal}/100.");
Log($"- Найгірше відставання від максимальної використаної довжини: {worstGap:F2}%.");
Log($"- Найменший знайдений контрприклад за числом деталей: [{string.Join(", ", counterexample)}].");
Log($"- Використана довжина для нього: жадібно {counterGreedy} см, оптимально {counterBest} см.");
foreach (int count in new[] { 15, 20 })
{
    var parts = Cutting.Parts(random, count);
    var exhaustive = Cutting.Exhaustive(rod, parts, false);
    var pruned = Cutting.Exhaustive(rod, parts, true);
    Check(exhaustive.Best == pruned.Best, "Оптимум відсікання");
    double time = Measurements.Time(() => GC.KeepAlive(Cutting.Exhaustive(rod, parts, false)));
    double prunedTime = Measurements.Time(() => GC.KeepAlive(Cutting.Exhaustive(rod, parts, true)));
    Log($"- {count} деталей: без відсікання {exhaustive.Nodes} вузлів, {time:F4} мс. З відсіканням {pruned.Nodes} вузлів, {prunedTime:F4} мс.");
}

Log("\n## 5. Вибір алгоритму");
Log($"1. На вході 50 000 символів прози, з яких виділено {wordCount} слів.");
Log($"2. Запит один: повернути {limit} найчастіших слів. Під час одного вимірювання текст незмінний.");
Log("3. Новий фрагмент потребує нового підрахунку. Для повторних запитів до того самого тексту можна кешувати частоти.");
Log($"4. Для 50 000 символів словник виграв у часі в {fullNaive / fullFast:F2} раза. Словник витрачає пам’ять на записи частот, а наївний спосіб багато разів перечитує слова.");
Log("5. Dictionary і Array.Sort забезпечують зберігання та впорядкування. Токенізація й правила нічиєї залишаються частиною задачі.");
Log($"6. На 1 000 символів отримано {shortNaive:F4} проти {shortFast:F4} мс, на 50 000 — {fullNaive:F4} проти {fullFast:F4} мс. Для повного фрагмента обираю словник.");

Log("\n## Крайові випадки");
Log("| Вхід | Очікувано | Отримано |");
Log("| --- | --- | --- |");
foreach (var test in new[] { ("", 0, 0), ("В", 1, 1), ("ВВВ", 1, 1), ("  В  в ", 1, 2) })
{
    var words = TextAlgorithms.Words(test.Item1);
    var result = TextAlgorithms.TopFast(words, limit);
    int frequency = result.Length == 0 ? 0 : result[0].Count;
    Check(TextAlgorithms.Same(result, TextAlgorithms.TopNaive(words, limit)), "Крайові топу");
    Check(result.Length == test.Item2 && frequency == test.Item3, "Крайовий текст");
    Log($"| Текст «{test.Item1}» | {test.Item2} різних, частота {test.Item3} | {result.Length}, {frequency} |");
}
foreach (var test in new[] { ("", "В", -1), ("В", "ВВ", -1), ("В", "", 0), ("", "", 0) })
{
    int index = TextAlgorithms.IndexOf(test.Item1, test.Item2, out _);
    Check(index == test.Item3, "Крайовий пошук");
    Log($"| «{test.Item2}» у «{test.Item1}» | {test.Item3} | {index} |");
}
foreach (int[] parts in new int[][] { [], [5] })
{
    int goal = parts.Length == 0 ? 0 : 5;
    int result = Cutting.Exhaustive(rod, parts, true).Best;
    Check(result == goal && result == Cutting.Greedy(rod, parts), "Крайовий розкрій");
    Log($"| Деталей {parts.Length} | {goal} см | {result} см |");
}

void SearchRow(string label, string text, string pattern, string shown)
{
    int index = TextAlgorithms.IndexOf(text, pattern, out int comparisons);
    Check(index == text.IndexOf(pattern, StringComparison.Ordinal), "Пошук підрядка");
    double naive = Measurements.Time(() => GC.KeepAlive(TextAlgorithms.IndexOf(text, pattern, out _)));
    double standard = Measurements.Time(() => GC.KeepAlive(text.IndexOf(pattern, StringComparison.Ordinal)));
    Log($"| {label} | {shown} | {index} | {comparisons} | {naive:F4} | {standard:F4} |");
}

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
