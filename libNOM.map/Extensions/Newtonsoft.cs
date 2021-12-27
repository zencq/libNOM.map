using Newtonsoft.Json.Linq;

namespace libNOM.map.Extensions;


internal static class NewtonsoftExtensions
{
    // https://stackoverflow.com/a/47269811
    internal static void Rename(this JToken token, string newName)
    {
        ArgumentNullException.ThrowIfNull(token, "Cannot rename a null token!");

        JProperty property;
        if (token.Type == JTokenType.Property)
        {
            if (token.Parent == null)
                throw new InvalidOperationException("Cannot rename a property with no parent!");

            property = (JProperty)(token);
        }
        else
        {
            if (token.Parent is null || token.Parent.Type != JTokenType.Property)
                throw new InvalidOperationException("Cannot rename as the parent of this token is not a JProperty!");

            property = (JProperty)(token.Parent);
        }

        // Note:
        // To avoid triggering a clone of the existing property's value, we need to save a reference to it
        // and then null out property.Value before adding the value to the new JProperty.
        var existingValue = property.Value;
#pragma warning disable CS8625 // Can be null nonetheless.
        property.Value = null;
#pragma warning restore CS8625

        var newProperty = new JProperty(newName, existingValue);
        property.Replace(newProperty);
    }
}
