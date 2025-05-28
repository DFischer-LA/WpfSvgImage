using System;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfSvgImage;

namespace WpfSvgImageTests
{
    [STATestClass]
    public class SvgImageTests
    {
        private const string ValidSvgContent = @"<svg xmlns='http://www.w3.org/2000/svg' width='10' height='10'><rect x='1' y='1' width='8' height='8' fill='red'/></svg>";
        private const string InvalidSvgContent = @"<notSvg></notSvg>";

        [ClassInitialize]
        public static void SetupPackSchema(TestContext context)
        {
            if (Application.Current == null)
            { new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }; }
        }

        [TestMethod]
        public void CreateSvgImage_FromFilePath_ValidSvg_SetsSource()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, ValidSvgContent);

                var svgImage = SvgImage.CreateSvgImage(tempFile);

                Assert.IsNotNull(svgImage);
                Assert.IsNotNull(svgImage.Source);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateSvgImage_FromFilePath_InvalidSvg_Throws()
        {
            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, InvalidSvgContent);

            try
            {
                SvgImage.CreateSvgImage(tempFile);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void CreateSvgImage_FromStream_ValidSvg_SetsSource()
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(ValidSvgContent));
            var svgImage = SvgImage.CreateSvgImage(stream);

            Assert.IsNotNull(svgImage);
            Assert.IsNotNull(svgImage.Source);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateSvgImage_FromStream_InvalidSvg_Throws()
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(InvalidSvgContent));
            SvgImage.CreateSvgImage(stream);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateSvgImage_FromStream_EmptyStream_Throws()
        {
            using var stream = new MemoryStream();
            SvgImage.CreateSvgImage(stream);
        }

    }
}
