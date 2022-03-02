using Newtonsoft.Json.Linq;

namespace libNOM.map.Extensions;


internal static class NewtonsoftExtensions
{
    /// <summary>
    /// Rename a JSON property.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="newName"></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <seealso href="https://stackoverflow.com/a/47269811"/>
    internal static void Rename(this JToken token, string newName)
    {
        JProperty existingProperty;
        if (token.Type == JTokenType.Property)
        {
            if (token.Parent is null)
                throw new InvalidOperationException("Cannot rename a property with no parent!");

            existingProperty = (JProperty)(token);
        }
        else
        {
            if (token.Parent is null || token.Parent.Type != JTokenType.Property)
                throw new InvalidOperationException("Cannot rename as the parent of this token is not a JProperty!");

            existingProperty = (JProperty)(token.Parent);
        }

        // To avoid triggering a clone of the existing value, we save a reference to it
        // and then null out property.Value before adding the value to the new JProperty.
        var existingValue = existingProperty.Value;
        existingProperty.Value = null!;

        var newProperty = new JProperty(newName, existingValue);
        existingProperty.Replace(newProperty);
    }
}
