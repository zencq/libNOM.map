using libNOM.map;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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
