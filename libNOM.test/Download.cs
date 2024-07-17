using libNOM.map;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace libNOM.test;


[TestClass]
public class DownloadTest
{
    [TestMethod]
    public void Download()
    {
        // Arrange
        var content = Properties.Resources.mapping_46504_download; // version altered to 9.99.0.9
        var initialVersion = Mapping.Version;

        // Act
        Directory.CreateDirectory("download");
        File.WriteAllBytes("download/mapping.json", content); // fake existing file as fallback to test workflow even if download itself fails
        Mapping.Update();

        // Assert
        Assert.IsTrue(Mapping.Version > initialVersion);
    }
}
