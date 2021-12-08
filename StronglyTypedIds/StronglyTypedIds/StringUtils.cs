namespace Enmeshed.StronglyTypedIds
{
    public static class StringUtils
    {
        private static readonly Random Random = new();

        public static string Generate(char[] chars, int resultLength)
        {
            return new string(Enumerable.Repeat(chars, resultLength).Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}