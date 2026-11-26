using System;
using System.Collections.Generic;

public static class Program
{
    public const string Text = "Алгоритм описує послідовність дій. Алгоритм можна записати словами, а можна записати кодом. Кодом алгоритм стає тоді, коли його виконує машина, а не людина. Одна задача — багато алгоритмів.";
    public static void Main()
    {
        var r = new Report(); r.Line("# Результати лабораторної 6"); r.EnvironmentInfo();
        Frequencies(r); Groups(r); Sets(r); Edges(r); Measurements.Run(r);
        r.Save("Results.md");
    }
    private static void Frequencies(Report r)
    {
        r.Line("## 1. Частоти літер"); r.Line(Text); r.Line();
        var letters = Letters(Text);
        var dictionary = Algorithms.Frequencies(letters);
        var list = Algorithms.FrequenciesList(letters);
        Bench.Check(dictionary.Count == list.keys.Count, "Кількість літер");
        foreach (var entry in dictionary)
        {
            int i = Algorithms.Position(list.keys, entry.Key);
            Bench.Check(i >= 0 && list.counts[i] == entry.Value, "Частоти різні");
            if (entry.Value >= 2)
                r.Line($"{entry.Key}: {entry.Value}");
        }
        r.Line($"Різних літер: {dictionary.Count}. Обидві реалізації збігаються.");
    }

    private static List<char> Letters(string text)
    {
        var result = new List<char>();
        foreach (char letter in text)
            if (char.IsLetter(letter))
                result.Add(char.ToLowerInvariant(letter));
        return result;
    }

    private static void Groups(Report r)
    {
        r.Line("## 2. Групування"); var students = Data.Students();
        var d = Algorithms.Group(students); var l = Algorithms.GroupList(students);
        Bench.Check(d.Count == l.keys.Count, "Кількість груп");
        foreach (var item in d)
        {
            int i = Algorithms.Position(l.keys, item.Key);
            Bench.Check(i >= 0 && item.Value.Count == l.groups[i].Count, "Розміри груп");
            for (int j = 0; j < item.Value.Count; j++) Bench.Check(ReferenceEquals(item.Value[j], l.groups[i][j]), "Склад груп");
            Bench.Check(Algorithms.Average(item.Value) == Algorithms.Average(l.groups[i]), "Середній бал");
            r.Line($"{item.Key}: {item.Value.Count} студентів, середній бал {Bench.F(Algorithms.Average(item.Value))}");
        }
        r.Line("Спискова й словникова реалізації збігаються."); r.Line();
    }
    private static void Sets(Report r)
    {
        r.Line("## 3. Дублікати"); var values = new List<int>(Data.Numbers(25, 19, 1, 9));
        var unordered = Algorithms.UniqueUnordered(values); var ordered = Algorithms.UniqueOrdered(values); var list = Algorithms.UniqueList(values);
        Bench.Check(Algorithms.Same(new List<int>(unordered), ordered) && Algorithms.Same(list, ordered), "Унікальні елементи");
        for (int i = 0; i < list.Count; i++) Bench.Check(list[i] == ordered[i], "Порядок");
        r.Line("Вхід: " + string.Join(", ", values)); r.Line("HashSet: " + string.Join(", ", unordered));
        r.Line("Із порядком: " + string.Join(", ", ordered)); r.Line();
        r.Line("## 4. Об’єднання"); var a = new List<int>(Data.Numbers(30, 19, 1, 23)); var b = new List<int>(Data.Numbers(30, 119, 1, 23));
        var hs = Algorithms.Union(a, b); var ls = Algorithms.UnionList(a, b);
        Bench.Check(Algorithms.Same(new List<int>(hs), ls), "Об’єднання різні");
        r.Line("A: " + string.Join(", ", a)); r.Line("B: " + string.Join(", ", b));
        r.Line("Об’єднання: " + string.Join(", ", hs)); r.Line($"Кількість: {hs.Count} із {a.Count + b.Count} початкових записів; результати збігаються."); r.Line();
    }
    private static void Edges(Report r)
    {
        r.Line("## Крайові випадки"); r.Line("| Вхід | Очікувано | Отримано |"); r.Line("|---|---|---|");
        Bench.Check(Algorithms.Frequencies(Letters("")).Count == 0, "Порожній текст");
        r.Line("| Порожній текст | 0 літер | 0 |");
        var letters = Algorithms.Frequencies(Letters("кіт дим"));
        Bench.Check(letters.Count == 6, "Усі літери різні");
        r.Line("| кіт дим | 6 різних літер | 6 |");
        var s = new List<Student> { new Student("А", "КН-3/1", 60), new Student("Б", "КН-3/2", 80) };
        Bench.Check(Algorithms.Group(s).Count == 1 && Algorithms.GroupList(s).groups.Count == 1 && Algorithms.Average(s) == 70, "Одна група за довжиною");
        r.Line("| Прізвища з однієї літери | 1 група, середній 70 | 1; 70 |");
        var unique = new List<int> { 1, 2, 3 }; Bench.Check(Algorithms.UniqueUnordered(unique).Count == 3 && Algorithms.Same(Algorithms.UniqueOrdered(unique), unique) && Algorithms.Same(Algorithms.UniqueList(unique), unique), "Без дублікатів");
        r.Line("| Без дублікатів | 1, 2, 3 | усі способи: 1, 2, 3, OK |");
        var other = new List<int> { 4, 5 }; Bench.Check(Algorithms.Union(unique, other).Count == 5 && Algorithms.UnionList(unique, other).Count == 5, "Немає перетину");
        r.Line("| Набори не перетинаються | 5 різних елементів | обидва способи: 5, OK |");
        Bench.Check(Algorithms.Union(unique, unique).Count == 3 && Algorithms.UnionList(unique, unique).Count == 3, "Однакові набори");
        r.Line("| Однакові набори | 1, 2, 3 | обидва способи: 3 елементи, OK |"); r.Line();
    }
}
