namespace Bloxstrap.Extensions
{
    static class CursorTypeEx
    {
        public static IReadOnlyDictionary<string, CursorType> Selections => new Dictionary<string, CursorType>
        {
            { "Default", CursorType.Default },
            { "Custom (Hand-drawn Cartoony)", CursorType.From2020 },
            { "2013 (Angular)", CursorType.From2013 },
            { "2006 (Cartoony)", CursorType.From2006 },
        };
    }
}
