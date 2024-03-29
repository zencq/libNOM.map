using libNOM.map;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace libNOM.test;


[TestClass]
public class DownloadTest
{
    [TestMethod]
    public void Download()
    {
        // Act
        Mapping.Update();

        // Assert
        Assert.IsTrue(File.Exists("download/mapping.json"));
    }
}
