using libNOM.map;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace libNOM.test;


[TestClass]
public class DeobfuscateTest
{
    [TestMethod]
    public void Deobfuscate_Compiler()
    {
        // Arrange
        var expected = Properties.Resources.compiler_375_deobfuscated;
        var jsonObject = JsonConvert.DeserializeObject(Properties.Resources.compiler_375_obfuscated) as JObject;

        // Act
        var unknownKeys = Mapping.Deobfuscate(jsonObject!);

        // Assert
        Assert.AreEqual(0, unknownKeys.Count);

        var actual = JsonConvert.SerializeObject(jsonObject);
        Assert.AreEqual(expected, actual, "Compiler not deobfuscated correctly");
    }

    [TestMethod]
    public void Deobfuscate_Legacy()
    {
        // Arrange
        var expected = Properties.Resources.legay_350_deobfuscated;
        var jsonObject = JsonConvert.DeserializeObject(Properties.Resources.legay_350_obfuscated) as JObject;

        // Act
        var unknownKeys = Mapping.Deobfuscate(jsonObject!);

        // Assert
        Assert.AreEqual(0, unknownKeys.Count);

        var actual = JsonConvert.SerializeObject(jsonObject);
        Assert.AreEqual(expected, actual, "Legacy not deobfuscated correctly");
    }

    [TestMethod]
    public void Deobfuscate_Wizard()
    {
        // Arrange
        var expected = Properties.Resources.wizard_362_deobfuscated;
        var jsonObject = JsonConvert.DeserializeObject(Properties.Resources.wizard_362_original) as JObject;

        // Act
        var unknownKeys = Mapping.Deobfuscate(jsonObject!);

        // Assert
        Assert.AreEqual(0, unknownKeys.Count);

        var actual = JsonConvert.SerializeObject(jsonObject);
        Assert.AreEqual(expected, actual, "Wizard not deobfuscated correctly");
    }
}
