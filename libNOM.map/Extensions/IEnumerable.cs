namespace libNOM.map.Extensions;


internal static class IEnumerableExtensions
{
    /// <summary>
    /// Splits the mapping entries into two pieces and makes the cut before the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>An IEnumerable with length 2. Either both pieces of self if the specified element was found else self twice.</returns>
    internal static IEnumerable<(IEnumerable<KeyValuePair<string, string>> Mapping, bool UseAccount)> SplitAtElement(this IEnumerable<KeyValuePair<string, string>> self, string value)
    {
        var element = self.Select((pair, Index) => (pair.Value, Index)).FirstOrDefault(i => i.Value.Equals(value));
        if (element.Value is not null)
        {
            yield return (self.Take(element.Index), false);
            yield return (self.Skip(element.Index), true);
        }
        else
            for (var i = 0; i < 2; i++)
                yield return (self, i != 0);
    }
}
