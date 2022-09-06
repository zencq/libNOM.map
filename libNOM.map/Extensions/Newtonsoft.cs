using Newtonsoft.Json.Linq;

namespace libNOM.map.Extensions;


internal static class NewtonsoftExtensions
{
    /// <summary>
    /// Rename a JSON property.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="newName"></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <seealso href="https://stackoverflow.com/a/47269811"/>
    internal static void Rename(this JToken input, string newName)
    {
        JProperty existingProperty;
        if (input.Type == JTokenType.Property)
        {
            if (input.Parent is null)
                throw new InvalidOperationException("Cannot rename a property with no parent!");

            existingProperty = (JProperty)(input);
        }
        else
        {
            if (input.Parent is null || input.Parent.Type != JTokenType.Property)
                throw new InvalidOperationException("Cannot rename as the parent of this token is not a JProperty!");

            existingProperty = (JProperty)(input.Parent);
        }

        // Stop if parent already has a property with newName.
        if (input.Parent.SelectTokens(newName).Any())
            return;

        // To avoid triggering a clone of the existing value, we save a reference to it
        // and then null out JProperty.Value before adding the value to the new one.
        var existingValue = existingProperty.Value;
        existingProperty.Value = null!;

        var newProperty = new JProperty(newName, existingValue);
        existingProperty.Replace(newProperty);
    }
}
