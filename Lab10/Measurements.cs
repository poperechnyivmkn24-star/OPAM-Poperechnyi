using System.Diagnostics;
using System.Globalization;
using System.Text;

public static class Measurements
{
    public static double Time(Action action)
    {
        action();
        double best = double.MaxValue;
        for (int i = 0; i < 3; i++)
        {
            var timer = Stopwatch.StartNew();
            action();
            timer.Stop();
            best = Math.Min(best, timer.Elapsed.TotalMilliseconds);
        }
        return best;
    }

    public static void Pitfalls(Action<string> log, List<string> words)
    {
        log("## 1. Пастки");
        log("| Додавань | +=, мс | StringBuilder, мс |");
        log("| ---: | ---: | ---: |");
        foreach (int n in new[] { 10_000, 100_000 })
        {
            double slow = Time(() =>
            {
                string value = "";
                for (int i = 0; i < n; i++) value += 'В';
                GC.KeepAlive(value);
            });
            double fast = Time(() =>
            {
                var value = new StringBuilder();
                for (int i = 0; i < n; i++) value.Append('В');
                GC.KeepAlive(value.ToString());
            });
            log($"| {n} | {slow:F4} | {fast:F4} |");
        }

        int lowerMatches = 0, ordinalMatches = 0;
        double lower = Time(() =>
        {
            int same = 0;
            for (int i = 0; i < 100_000; i++)
                if (words[i % words.Count].ToLower() == words[i / 2 % words.Count].ToLower())
                    same++;
            lowerMatches = same;
        });
        double ordinal = Time(() =>
        {
            int same = 0;
            for (int i = 0; i < 100_000; i++)
                if (words[i % words.Count].Equals(words[i / 2 % words.Count], StringComparison.OrdinalIgnoreCase))
                    same++;
            ordinalMatches = same;
        });
        log($"\n100 000 пар: ToLower — {lower:F4} мс ({lowerMatches} збігів), OrdinalIgnoreCase — {ordinal:F4} мс ({ordinalMatches} збігів).");

        string[] sample = ["гай", "ґанок", "ґава", "ґазда", "ґатунок", "ґвалт", "ґедзь", "ґелґіт", "ґердан", "ґиндичка", "ґирлиґа", "ґлей", "ґонт", "ґонта", "ґраніт", "ґречний", "ґринджоли", "ґрунт", "ґудзик", "ґуля", "ґума", "дар"];
        var cultural = (string[])sample.Clone();
        Array.Sort(sample, StringComparer.Ordinal);
        Array.Sort(cultural, StringComparer.Create(CultureInfo.GetCultureInfo("uk-UA"), ignoreCase: false));
        log("Ординально: " + string.Join(", ", sample));
        log("За українською культурою: " + string.Join(", ", cultural));
        log("\nКожне += копіює вже накопичений незмінний рядок. OrdinalIgnoreCase порівнює без створення двох нижньорегістрових копій. Українське порівняння розміщує ґ між г і д.\n");
    }
}
