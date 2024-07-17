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
        var path = "download/mapping.json";

        // Act
        File.WriteAllBytes(path, content); // fake existing file to test workflow independent of download result
        Mapping.Update();

        // Assert
        Assert.IsTrue(Mapping.Version > initialVersion);
    }
}
