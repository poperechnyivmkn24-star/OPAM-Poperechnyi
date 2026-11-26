public static class TextAlgorithms
{
    public static List<string> Words(string text)
    {
        var words = new List<string>();
        int i = 0;
        while (i < text.Length)
        {
            if (!char.IsLetter(text[i]))
            {
                i++;
                continue;
            }
            int start = i++;
            while (i < text.Length && (char.IsLetter(text[i]) ||
                    ((text[i] is '\'' or '’' or 'ʼ') && i + 1 < text.Length && char.IsLetter(text[i + 1]))))
                i++;
            words.Add(text[start..i]);
        }
        return words;
    }

    public static (string Word, int Count)[] TopNaive(List<string> words, int limit)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var counts = new List<(string Word, int Count)>();
        foreach (string word in words)
        {
            if (!seen.Add(word))
                continue;
            int count = 0;
            foreach (string other in words)
                if (word.Equals(other, StringComparison.OrdinalIgnoreCase))
                    count++;
            counts.Add((word, count));
        }
        return Rank(counts.ToArray(), limit);
    }

    public static (string Word, int Count)[] TopFast(List<string> words, int limit)
    {
        var table = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (string word in words)
        {
            table.TryGetValue(word, out int count);
            table[word] = count + 1;
        }
        var counts = new (string Word, int Count)[table.Count];
        int index = 0;
        foreach (var pair in table)
            counts[index++] = (pair.Key, pair.Value);
        return Rank(counts, limit);
    }

    private static (string Word, int Count)[] Rank((string Word, int Count)[] values, int limit)
    {
        Array.Sort(values, (a, b) =>
        {
            int order = b.Count.CompareTo(a.Count);
            return order != 0 ? order : StringComparer.OrdinalIgnoreCase.Compare(a.Word, b.Word);
        });
        return values[..Math.Min(limit, values.Length)];
    }

    public static bool Same((string Word, int Count)[] a, (string Word, int Count)[] b)
    {
        if (a.Length != b.Length)
            return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i].Count != b[i].Count || !a[i].Word.Equals(b[i].Word, StringComparison.OrdinalIgnoreCase))
                return false;
        return true;
    }

    public static int IndexOf(string text, string pattern, out int comparisons)
    {
        comparisons = 0;
        for (int start = 0; start <= text.Length - pattern.Length; start++)
        {
            int i = 0;
            while (i < pattern.Length)
            {
                comparisons++;
                if (text[start + i] != pattern[i])
                    break;
                i++;
            }
            if (i == pattern.Length)
                return start;
        }
        return -1;
    }
}
