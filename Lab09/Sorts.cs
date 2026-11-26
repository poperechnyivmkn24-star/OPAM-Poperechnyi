using System;

public delegate void SortAlgorithm<T>(T[] a, Func<T, T, int> cmp, out long comparisons, out long moves);

public static class Sorts
{
    public static void Insertion<T>(T[] data, Func<T, T, int> compare, out long comparisons, out long moves)
    {
        comparisons = moves = 0;
        for (int i = 1; i < data.Length; i++)
        {
            T value = data[i];
            int j = i;
            while (j > 0)
            {
                comparisons++;
                if (compare(data[j - 1], value) <= 0)
                    break;
                data[j] = data[j - 1];
                moves++;
                j--;
            }
            if (j != i)
            {
                data[j] = value;
                moves++;
            }
        }
    }

    public static void QuickMedian<T>(T[] data, Func<T, T, int> compare, out long comparisons, out long moves)
    {
        long checks = 0, writes = 0;
        Sort(0, data.Length - 1);
        comparisons = checks;
        moves = writes;

        int Compare(T x, T y)
        {
            checks++;
            return compare(x, y);
        }

        void Swap(int i, int j)
        {
            if (i == j) return;
            (data[i], data[j]) = (data[j], data[i]);
            writes += 2;
        }

        T Median(T a, T b, T c)
        {
            if (Compare(a, b) > 0) (a, b) = (b, a);
            if (Compare(b, c) > 0) b = c;
            return Compare(a, b) > 0 ? a : b;
        }

        void Sort(int lo, int hi)
        {
            while (lo < hi)
            {
                T pivot = Median(data[lo], data[lo + (hi - lo) / 2], data[hi]);
                int lt = lo, i = lo, gt = hi;
                while (i <= gt)
                {
                    int order = Compare(data[i], pivot);
                    if (order < 0) Swap(lt++, i++);
                    else if (order > 0) Swap(i, gt--);
                    else i++;
                }

                if (lt - lo < hi - gt)
                {
                    Sort(lo, lt - 1);
                    lo = gt + 1;
                }
                else
                {
                    Sort(gt + 1, hi);
                    hi = lt - 1;
                }
            }
        }
    }

    public static void MergeBottomUp<T>(T[] a, Func<T, T, int> cmp, out long comparisons, out long moves)
    {
        comparisons = 0; moves = 0; var buffer = new T[a.Length];
        for (int width = 1; width < a.Length; width = width > a.Length / 2 ? a.Length : width * 2)
        {
            for (int start = 0; start < a.Length; start += 2 * width)
            {
                int middle = Math.Min(start + width, a.Length), end = Math.Min(start + 2 * width, a.Length);
                int left = start, right = middle, pos = start;
                while (left < middle && right < end)
                {
                    comparisons++;
                    if (cmp(a[left], a[right]) <= 0) buffer[pos++] = a[left++];
                    else buffer[pos++] = a[right++];
                    moves++;
                }
                while (left < middle) { buffer[pos++] = a[left++]; moves++; }
                while (right < end) { buffer[pos++] = a[right++]; moves++; }
                for (int i = start; i < end; i++) { a[i] = buffer[i]; moves++; }
            }
        }
    }
    // Підрахунок не є порівняльним сортуванням. Узагальнення — за елементом,
    // а цілочисельний ключ і його діапазон передаються параметрами.
    public static void Counting<T>(T[] a, Func<T, int> keyOf, int min, int max, out long comparisons, out long moves)
    {
        if (max < min) throw new ArgumentException("Некоректний діапазон");
        comparisons = 0; moves = 0; var counts = new int[checked(max - min + 1)];
        foreach (T item in a)
        {
            int key = keyOf(item);
            if (key < min || key > max) throw new ArgumentOutOfRangeException(nameof(a));
            counts[key - min]++;
        }
        for (int i = 1; i < counts.Length; i++) counts[i] += counts[i - 1];
        var buffer = new T[a.Length];
        for (int i = a.Length - 1; i >= 0; i--) { buffer[--counts[keyOf(a[i]) - min]] = a[i]; moves++; }
        for (int i = 0; i < a.Length; i++) { a[i] = buffer[i]; moves++; }
        // counts — службові частоти, не масив елементів T; їх зміни не є moves.
    }
}
