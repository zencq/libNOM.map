using Newtonsoft.Json.Linq;

namespace libNOM.map.Extensions;


internal static class NewtonsoftExtensions
{
    /// <summary>
    /// Rename a JSON property.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="name">New name of the property.</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <seealso href="https://stackoverflow.com/a/47269811"/>
    internal static void Rename(this JToken self, string name)
    {
        JProperty existingProperty;
        if (self.Type == JTokenType.Property)
        {
            if (self.Parent is null)
                throw new InvalidOperationException("Cannot rename a property with no parent!");

            existingProperty = (JProperty)(self);
        }
        else
        {
            if (self.Parent is null || self.Parent.Type != JTokenType.Property)
                throw new InvalidOperationException("Cannot rename as the parent of this token is not a JProperty!");

            existingProperty = (JProperty)(self.Parent);
        }

        // Stop if parent already has a property with the new name.
        if (self.Parent.SelectTokens(name).Any())
            return;

        // To avoid triggering a clone of the existing value, we save a reference to it
        // and then null out JProperty.Value before adding the value to the new one.
        var content = existingProperty.Value;
        existingProperty.Value = null!;
        existingProperty.Replace(new JProperty(name, content));
    }
}
