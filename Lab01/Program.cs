using System.Text;

Console.OutputEncoding = Encoding.UTF8;
const int number = 19;
const string name = "Василь";
int letter = "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ".IndexOf(char.ToUpperInvariant(name[0])) + 1;
Console.WriteLine($"N = {number}, K = {letter}, a = {number % 3}, b = {number % 4}, c = {number % 5}");
Console.WriteLine("1 — дата, 2 — послідовність, 3 — крайові випадки, 4 — НСД");
switch (Console.ReadLine())
{
    case "1": DateTask(); break;
    case "2":
        Console.WriteLine("Вводьте цілі числа по одному в рядку. 0 завершує введення.");
        Console.WriteLine(ReadSequence(Console.In, Console.Out, letter));
        break;
    case "3":
        Check("Порожній вхід", "", new(0, 0, null), letter);
        Check("Одне число", "-6\n0", new(-6, 1, -6), letter);
        Check("Однакові числа", "3\n3\n3\n0", new(9, 1, 3), letter);
        Check("Жодного кратного", "1\n-2\n4\n0", new(3, 1, null), letter);
        break;
    case "4": GcdTask(number); break;
    default: Console.WriteLine("Введіть номер завдання від 1 до 4."); break;
}

static void DateTask()
{
    Console.WriteLine("День, місяць і рік, кожне число в окремому рядку:");
    if (!int.TryParse(Console.ReadLine(), out int day) ||
        !int.TryParse(Console.ReadLine(), out int month) ||
        !int.TryParse(Console.ReadLine(), out int year) || year < 1 || month is < 1 or > 12)
    {
        Console.WriteLine("Некоректна дата.");
        return;
    }

    bool leap = year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
    int monthLength = month switch { 2 => leap ? 29 : 28, 4 or 6 or 9 or 11 => 30, _ => 31 };
    if (day < 1 || day > monthLength)
    {
        Console.WriteLine("Некоректна дата.");
        return;
    }

    int previousDays = month switch
    {
        1 => 0, 2 => 31, 3 => 59, 4 => 90, 5 => 120, 6 => 151,
        7 => 181, 8 => 212, 9 => 243, 10 => 273, 11 => 304, _ => 334
    };
    string season = month switch
    {
        12 or 1 or 2 => "зима", 3 or 4 or 5 => "весна",
        6 or 7 or 8 => "літо", _ => "осінь"
    };
    Console.WriteLine($"{season}, день року: {previousDays + day + (leap && month > 2 ? 1 : 0)}");
}

static SequenceResult ReadSequence(TextReader input, TextWriter messages, int divisor)
{
    long sum = 0;
    int zeros = 0;
    int? maximum = null;
    while (input.ReadLine() is { } line)
    {
        if (!int.TryParse(line, out int value))
        {
            messages.WriteLine("Введіть ціле число.");
            continue;
        }
        if (value == 0)
        {
            zeros++;
            break;
        }
        sum += value;
        if (value % divisor == 0 && (maximum is null || value > maximum))
            maximum = value;
    }
    return new(sum, zeros, maximum);
}

static void Check(string label, string input, SequenceResult expected, int divisor)
{
    var actual = ReadSequence(new StringReader(input), TextWriter.Null, divisor);
    Console.WriteLine($"{label} | {expected} | {actual} | {expected == actual}");
}

static void GcdTask(int number)
{
    int a = 1_000_000 + number;
    int b = 999_999 - number;
    int divisor = Math.Min(a, b);
    int tries = 1;
    while (a % divisor != 0 || b % divisor != 0)
    {
        divisor--;
        tries++;
    }
    int steps = 0;
    while (b != 0)
    {
        (a, b) = (b, a % b);
        steps++;
    }
    Console.WriteLine($"Перебір: НСД = {divisor}, {tries} ітерацій.");
    Console.WriteLine($"Евклід: НСД = {a}, {steps} ітерацій.");
    Console.WriteLine($"Відношення: {(double)tries / steps:F2}");
}

readonly record struct SequenceResult(long Sum, int Zeros, int? Maximum)
{
    public override string ToString() =>
        $"сума {Sum}, нулів {Zeros}, максимум {Maximum?.ToString() ?? "немає даних"}";
}
