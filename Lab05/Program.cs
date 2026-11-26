using System.Text;

Console.OutputEncoding = Encoding.UTF8;
const int n = 19, k = 3;
foreach (string test in new[] { "(a[b]{c})", "([)]", ")(", "((", "", "<{[}]>", "([{}]", "звичайний текст" })
{
    int standard = Brackets(test, false, k % 4 + 2);
    int custom = Brackets(test, true, k % 4 + 2);
    if (standard != custom)
        throw new InvalidOperationException("Результати стеків різні.");
    Console.WriteLine($"Дужки \"{test}\": {standard}");
}
foreach (string test in new[] { "3 4 + 2 *", "5 3 -", "8 2 /", "2 3 4 * +", "5 0 /", "7 3 min 2 * 3 -", "", "5", "2 3", "5 +", "5 X" })
{
    var answer = Evaluate(test);
    Console.WriteLine($"ЗПЗ \"{test}\": {answer.Error ?? answer.Value.ToString()}");
}

var random = new Random(n);
var weights = new int[8 + k % 5];
for (int i = 0; i < weights.Length; i++)
    weights[i] = random.Next(1, 10);
Simulate(weights, k % 3 + 2);
Simulate([], k % 3 + 2);

var testStack = new IntStack(k % 4 + 2);
try { testStack.Pop(); }
catch (InvalidOperationException ex) { Console.WriteLine(ex.Message); }
for (int i = 1; i <= k % 4 + 3; i++)
{
    testStack.Push(i);
    Console.WriteLine($"Push {i}: Count={testStack.Count}, Capacity={testStack.Capacity}");
}

static int Brackets(string text, bool custom, int capacity)
{
    var standard = custom ? null : new Stack<int>();
    var own = custom ? new IntStack(capacity) : null;
    int Count() => own?.Count ?? standard!.Count;
    void Push(int index)
    {
        if (own is null) standard!.Push(index);
        else own.Push(index);
    }
    int Pop() => own?.Pop() ?? standard!.Pop();

    for (int i = 0; i < text.Length; i++)
    {
        char letter = text[i];
        if (letter is '(' or '[' or '{' or '<')
            Push(i);
        else if (letter is ')' or ']' or '}' or '>')
        {
            if (Count() == 0)
                return i;
            char open = text[Pop()];
            if ((open, letter) is not (('(', ')') or ('[', ']') or ('{', '}') or ('<', '>')))
                return i;
        }
    }

    int firstUnclosed = -1;
    while (Count() > 0)
        firstUnclosed = Pop();
    return firstUnclosed;
}

static (int Value, string? Error) Evaluate(string expression)
{
    var stack = new Stack<int>();
    foreach (string token in expression.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
    {
        if (int.TryParse(token, out int value))
        {
            stack.Push(value);
            continue;
        }
        if (token is not ("+" or "-" or "*" or "/" or "min" or "max"))
            return (0, $"Невідомий токен {token}.");
        if (stack.Count < 2)
            return (0, "Бракує операндів.");
        int right = stack.Pop(), left = stack.Pop();
        if (token == "/" && right == 0)
            return (0, "Ділення на нуль.");
        try
        {
            int result = token switch
            {
                "+" => checked(left + right), "-" => checked(left - right), "*" => checked(left * right),
                "/" => left / right, "min" => Math.Min(left, right), _ => Math.Max(left, right)
            };
            stack.Push(result);
        }
        catch (OverflowException)
        {
            return (0, "Переповнення цілих чисел.");
        }
    }
    return stack.Count == 1 ? (stack.Pop(), null) : (0, "Очікується рівно один результат.");
}

static void Simulate(int[] weights, int quantum)
{
    var queue = new Queue<int>();
    var rest = (int[])weights.Clone();
    var starts = new int[weights.Length];
    var ends = new int[weights.Length];
    for (int i = 0; i < weights.Length; i++)
    {
        starts[i] = -1;
        queue.Enqueue(i);
    }
    int maxQueue = queue.Count;
    int minute = 0;
    while (queue.Count > 0)
    {
        int id = queue.Dequeue();
        if (starts[id] < 0)
            starts[id] = minute;
        int served = Math.Min(quantum, rest[id]);
        minute += served;
        rest[id] -= served;
        if (rest[id] > 0)
            queue.Enqueue(id);
        else
            ends[id] = minute;
        maxQueue = Math.Max(maxQueue, queue.Count);
    }
    for (int i = 0; i < weights.Length; i++)
        Console.WriteLine($"Заявка {i + 1}: вага {weights[i]}, початок {starts[i]}, кінець {ends[i]}.");
    Console.WriteLine($"Максимальна черга: {maxQueue}.");
}

sealed class IntStack(int capacity)
{
    private int[] items = new int[capacity];
    public int Count { get; private set; }
    public int Capacity => items.Length;

    public void Push(int value)
    {
        if (Count == Capacity)
            Array.Resize(ref items, Math.Max(1, Capacity * 2));
        items[Count++] = value;
    }

    public int Pop()
    {
        int value = Peek();
        Count--;
        return value;
    }

    public int Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException("Стек порожній.");
        return items[Count - 1];
    }
}
