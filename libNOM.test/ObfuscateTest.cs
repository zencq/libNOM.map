using libNOM.map;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace libNOM.test;


[TestClass]
public class ObfuscateTest
{
    [TestMethod]
    public void Obfuscate_Compiler()
    {
        // Arrange
        var expected = Properties.Resources.compiler_375_obfuscated;
        var jsonObject = JsonConvert.DeserializeObject(Properties.Resources.compiler_375_deobfuscated) as JObject;

        // Act
#pragma warning disable CS8604 // Loaded from resource and therefore not null.
        Mapping.Instance.Obfuscate(jsonObject);
#pragma warning restore CS8604

        // Assert
        var actual = JsonConvert.SerializeObject(jsonObject);
        Assert.AreEqual(expected, actual, "Compiler not obfuscated correctly");
    }

    [TestMethod]
    public void Obfuscate_Legacy()
    {
        // Arrange
        var expected = Properties.Resources.legay_350_obfuscated;
        var jsonObject = JsonConvert.DeserializeObject(Properties.Resources.legay_350_deobfuscated) as JObject;

        // Act
#pragma warning disable CS8604 // Loaded from resource and therefore not null.
        Mapping.Instance.Obfuscate(jsonObject);
#pragma warning restore CS8604

        // Assert
        var actual = JsonConvert.SerializeObject(jsonObject);
        Assert.AreEqual(expected, actual, "Legacy not obfuscated correctly");
    }

    [TestMethod]
    public void Obfuscate_Wizard()
    {
        // Arrange
        var expected = Properties.Resources.wizard_362_obfuscated;
        var jsonObject = JsonConvert.DeserializeObject(Properties.Resources.wizard_362_deobfuscated) as JObject;

        // Act
#pragma warning disable CS8604 // Loaded from resource and therefore not null.
        Mapping.Instance.Obfuscate(jsonObject);
#pragma warning restore CS8604

        // Assert
        var actual = JsonConvert.SerializeObject(jsonObject);
        Assert.AreEqual(expected, actual, "Wizard not obfuscated correctly");
    }
}
