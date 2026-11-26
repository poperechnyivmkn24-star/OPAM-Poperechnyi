using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main()
    {
        var r = new Report(); r.Line("# Результати лабораторної 7"); r.EnvironmentInfo();
        var numbers = new List<int>(Data.Numbers(20, 19, -3, 3));
        var words = new List<string> { "алгоритм", "код", "а", "блок-схема", "ввід", "вивід", "онлайн-курс", "цикл", "тип", "тест-кейс", "мова", "дані", "пам'ять" };
        var students = Data.Students();
        r.Line("Числа: " + string.Join(", ", numbers)); r.Line("Слова: " + string.Join(", ", words)); r.Line();
        r.Line("## 1. Filter");
        Show(r, "Числа, кратні 3", Algorithms.Filter(numbers, x => x % 3 == 0));
        Show(r, "Слова з голосної", Algorithms.Filter(words, x => StartsWithVowel(x)));
        Show(r, "Група КН-3/1", Algorithms.Filter(students, s => s.Group == "КН-3/1"));
        r.Line("## 2. Map");
        Show(r, "Числа в дужках", Algorithms.Map(numbers, x => $"({x})"));
        Show(r, "Кількість голосних", Algorithms.Map(words, CountVowels));
        Show(r, "Склав", Algorithms.Map(students, s => s.Score >= 60));
        Show(r, "Композиція Filter → Map", Algorithms.Map(Algorithms.Filter(students, s => s.Group == "КН-3/1"), s => s.Score >= 60));
        r.Line("## 3. Aggregate");
        int sum = Algorithms.Aggregate(numbers, 0, (s, x) => s + x);
        long product = Algorithms.Aggregate(numbers, 1L, (p, x) => p * x);
        int max = Maximum(numbers);
        var stats = Algorithms.Aggregate(numbers, (Sum: 0L, Count: 0), (a, x) => (a.Sum + x, a.Count + 1));
        r.Line($"Сума: {sum}; добуток: {product}; максимум: {max}; середнє: {(double)stats.Sum / stats.Count:F2}."); r.Line();
        r.Line("## 4. GroupBy");
        ShowGroups(r, "За групою", Algorithms.GroupBy(students, s => s.Group));
        ShowGroups(r, "За першою літерою", Algorithms.GroupBy(students, s => s.Name[0]));
        ShowGroups(r, "За діапазоном балу", Algorithms.GroupBy(students, s => s.Score < 60 ? "0–59" : s.Score < 75 ? "60–74" : s.Score < 90 ? "75–89" : "90–100"));
        ShowGroups(r, "Слова за довжиною", Algorithms.GroupBy(words, x => x.Length));
        Captures(r); Edges(r); r.Save("Results.md");
    }
    private const string Vowels = "аеєиіїоуюя";

    private static bool StartsWithVowel(string text) =>
        text.Length > 0 && Vowels.IndexOf(char.ToLowerInvariant(text[0])) >= 0;

    private static int CountVowels(string text)
    {
        int count = 0;
        foreach (char letter in text)
            if (Vowels.IndexOf(char.ToLowerInvariant(letter)) >= 0)
                count++;
        return count;
    }
    public static int Maximum(List<int> src)
    {
        if (src.Count == 0) throw new ArgumentException("Порожній список не має максимуму.", nameof(src));
        return Algorithms.Aggregate(src, src[0], (m, x) => Math.Max(m, x));
    }
    private static void Show<T>(Report r, string title, List<T> values)
    { r.Line($"{title} ({values.Count}): {string.Join(", ", values)}"); r.Line(); }
    private static void ShowGroups<T, TKey>(Report r, string title, Dictionary<TKey, List<T>> groups) where TKey : notnull
    { r.Line(title); foreach (var g in groups) r.Line($"- {g.Key}: {g.Value.Count}"); r.Line(); }
    private static void Captures(Report r)
    {
        r.Line("## 5. Захоплення змінних"); int threshold = 3;
        Func<int, bool> predicate = x => x > threshold;
        bool before = predicate(4); threshold = 1000; bool after = predicate(4);
        Bench.Check(before && !after, "Захоплення порога");
        r.Line($"predicate(4): до зміни {before}, після threshold=1000: {after}.");
        var functions = new List<Func<int>>(); for (int i = 0; i < 3; i++) functions.Add(() => i);
        var values = Algorithms.Map(functions, f => f()); r.Line("for: " + string.Join(", ", values));
        Bench.Check(values[0] == 3 && values[1] == 3 && values[2] == 3, "for");
        functions.Clear(); foreach (int i in new[] { 0, 1, 2 }) functions.Add(() => i);
        values = Algorithms.Map(functions, f => f()); r.Line("foreach: " + string.Join(", ", values));
        Bench.Check(values[0] == 0 && values[1] == 1 && values[2] == 2, "foreach");
        functions.Clear(); for (int i = 0; i < 3; i++) { int copy = i; functions.Add(() => copy); }
        r.Line("Виправлений for: " + string.Join(", ", Algorithms.Map(functions, f => f()))); r.Line();
    }
    private static void Edges(Report r)
    {
        r.Line("## Крайові випадки"); r.Line("| Вхід | Очікувано | Отримано |"); r.Line("|---|---|---|");
        var empty = new List<int>();
        Bench.Check(Algorithms.Filter(empty, x => true).Count == 0 && Algorithms.Map(empty, x => x.ToString()).Count == 0 && Algorithms.GroupBy(empty, x => x).Count == 0, "Порожні алгоритми");
        r.Line("| Порожній список: Filter, Map, GroupBy | порожні результати | 0, 0, 0, OK |");
        Bench.Check(Algorithms.Aggregate(empty, int.MinValue, (m, x) => Math.Max(m, x)) == int.MinValue, "Старт на порожньому");
        bool thrown = false; try { Maximum(empty); } catch (ArgumentException) { thrown = true; }
        Bench.Check(thrown, "Порожній максимум");
        r.Line("| Порожній максимум | Aggregate повертає старт; Maximum відхиляє вхід | int.MinValue; ArgumentException, OK |");
        Bench.Check(Algorithms.Filter(new List<int> { 1, 2 }, x => x % 3 == 0).Count == 0, "Нічого не підходить");
        r.Line("| Нічого не підходить | порожній Filter | 0, OK |");
        var group = Algorithms.GroupBy(new List<int> { 1, 2, 3 }, x => "одна");
        Bench.Check(group.Count == 1 && group.TryGetValue("одна", out var items) && items.Count == 3, "Одна група");
        r.Line("| Усі елементи в одній групі | 1 група із 3 елементів | 1; 3, OK |");
        var one = new List<int> { -2 };
        Bench.Check(Algorithms.Aggregate(one, 0, (s, x) => s + x) == -2 && Algorithms.Aggregate(one, 1L, (p, x) => p * x) == -2 && Maximum(one) == -2 && Algorithms.Aggregate(one, (Sum: 0, Count: 0), (a, x) => (a.Sum + x, a.Count + 1)) == (-2, 1), "Один елемент");
        r.Line("| Один елемент −2 | сума −2, добуток −2, максимум −2, середнє −2 | −2; −2; −2; −2, OK |");
    }
}
