using BookMatrix.Models;

namespace BookMatrix.Data;

/// <summary>
/// Demo data. This is a faithful port of the generator in the original index.html:
/// the same titles, publishers and seeded random sequence (mulberry32, seed 2026),
/// so the catalog contains exactly the same 10,000 books as the HTML prototype.
/// </summary>
public static class CatalogSeed
{
    public const int BookCount = 10000;
    public const uint RandomSeed = 2026;

    private static readonly (string Title, string Author)[] TitleData =
    {
        ("Под игото", "Иван Вазов"),
        ("Пряпорец и гусла", "Иван Вазов"),
        ("Железният светилник", "Димитър Димов"),
        ("Тютюн", "Димитър Димов"),
        ("Ако можеха камъните да говорят", "Йордан Йовков"),
        ("Старопланински легенди", "Йордан Йовков"),
        ("Чичовци", "Иван Вазов"),
        ("Бай Ганьо", "Алеко Константинов"),
        ("Дядо Йоцо гледа", "Иван Вазов"),
        ("Хайдути", "Христо Ботев"),
        ("Опълченците на Шипка", "Иван Вазов"),
        ("Нощем с белите коне", "Павел Вежинов"),
        ("Бариерата", "Павел Вежинов"),
        ("Една българска роза", "Емилиян Станев"),
        ("Иван Кондарев", "Емилиян Станев"),
        ("Вражалец", "Елин Пелин"),
        ("Гераците", "Елин Пелин"),
        ("Пижо и Пендо", "Елин Пелин"),
        ("Албена", "Йордан Йовков"),
        ("Козият рог", "Николай Хайтов"),
        ("Диви разкази", "Николай Хайтов"),
        ("Господарят на пръстените", "Дж. Р. Р. Толкин"),
        ("Малкият принц", "Антоан дьо Сент-Екзюпери"),
        ("Престъпление и наказание", "Фьодор Достоевски"),
        ("Ромео и Жулиета", "Уилям Шекспир"),
        ("Хамлет", "Уилям Шекспир"),
        ("Старецът и морето", "Ърнест Хемингуей"),
        ("1984", "Джордж Оруел"),
        ("Гордост и предразсъдъци", "Джейн Остин"),
        ("Граф Монте Кристо", "Александър Дюма"),
    };

    private static readonly string[] Publishers =
    {
        "Народна просвета",
        "Захарий Стоянов",
        "Труд",
        "Просвета",
        "Хермес",
        "Колибри",
        "Сиела",
        "Изкуства",
    };

    /// <summary>Builds the 10,000 books. The order of the random calls must not change.</summary>
    public static List<Book> BuildBooks(DateTime utcNow)
    {
        var rand = new Mulberry32(RandomSeed);
        var books = new List<Book>(BookCount);

        for (var i = 0; i < BookCount; i++)
        {
            var (title, author) = TitleData[i % TitleData.Length];

            var book = new Book
            {
                Id = i + 1,
                Title = title,
                Author = author,
                Publisher = Publishers[(int)Math.Floor(rand.Next() * Publishers.Length)],
                Year = 1985 + (int)Math.Floor(rand.Next() * 39),
                SearchText = (title + " " + author).ToLowerInvariant()
            };

            if (rand.Next() < 0.18)
            {
                _ = rand.Next();
                _ = rand.Next();
            }

            books.Add(book);
        }

        return books;
    }
}

/// <summary>The same 32-bit PRNG the HTML prototype used (mulberry32), with identical wrap-around arithmetic.</summary>
public sealed class Mulberry32
{
    private uint _state;

    public Mulberry32(uint seed) => _state = seed;

    public double Next()
    {
        unchecked
        {
            _state += 0x6D2B79F5u;
            var t = Imul(_state ^ (_state >> 15), 1u | _state);
            t = (t + Imul(t ^ (t >> 7), 61u | t)) ^ t;
            return (t ^ (t >> 14)) / 4294967296.0;
        }
    }

    private static uint Imul(uint a, uint b) => unchecked(a * b);
}
