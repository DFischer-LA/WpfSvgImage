using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfSvgImage;

namespace WpfSvgImageTests
{
    [TestClass]
    public class SvgHelpersTests
    {
        [TestMethod]
        public void ParseBrush_ShouldReturnTransparent_WhenValueIsNone()
        {
            var brush = SvgHelpers.ParseBrush("none");
            Assert.IsInstanceOfType(brush, typeof(System.Windows.Media.SolidColorBrush));
            Assert.AreEqual(System.Windows.Media.Colors.Transparent, ((System.Windows.Media.SolidColorBrush)brush).Color);
        }

        [TestMethod]
        public void ParseBrush_ShouldReturnSolidColorBrush_WhenValueIsValidColorHex()
        {
            var brush = SvgHelpers.ParseBrush("#FF0000");
            Assert.IsInstanceOfType(brush, typeof(System.Windows.Media.SolidColorBrush));
            Assert.AreEqual(System.Windows.Media.Colors.Red, ((System.Windows.Media.SolidColorBrush)brush).Color);
        }

        [TestMethod]
        public void ParseBrush_ShouldReturnSolidColorBrush_WhenValueIsValidColorName()
        {
            var brush = SvgHelpers.ParseBrush("red");
            Assert.IsInstanceOfType(brush, typeof(System.Windows.Media.SolidColorBrush));
            Assert.AreEqual(System.Windows.Media.Colors.Red, ((System.Windows.Media.SolidColorBrush)brush).Color);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnIdentityMatrix_WhenTransformIsEmpty()
        {
            var matrix = SvgHelpers.ParseTransform("");
            Assert.AreEqual(Matrix.Identity, matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnTranslationMatrix_WhenTransformIsTranslate()
        {
            var matrix = SvgHelpers.ParseTransform("translate(10, 20)");
            Assert.AreEqual(new Matrix(1, 0, 0, 1, 10, 20), matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnScaleMatrix_WhenTransformIsScale()
        {
            var matrix = SvgHelpers.ParseTransform("scale(2, 3)");
            Assert.AreEqual(new Matrix(2, 0, 0, 3, 0, 0), matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnCombinedMatrix_WhenTransformContainsMultipleCommands()
        {
            var matrix = SvgHelpers.ParseTransform("translate(10, 20) scale(2, 3)");
            Assert.AreEqual(new Matrix(2, 0, 0, 3, 20, 60), matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldHandleMultipleSpacesAndCommas()
        {
            var matrix = SvgHelpers.ParseTransform("translate(10, 20) , scale(2, 3)");
            Assert.AreEqual(new Matrix(2, 0, 0, 3, 20, 60), matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldHandleSingleValueTranslate()
        {
            var matrix = SvgHelpers.ParseTransform("translate(10)");
            Assert.AreEqual(new Matrix(1, 0, 0, 1, 10, 0), matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldHandleSingleValueScale()
        {
            var matrix = SvgHelpers.ParseTransform("scale(2)");
            Assert.AreEqual(new Matrix(2, 0, 0, 2, 0, 0), matrix);
        }

        [TestMethod]
        public void ParseTranform_ShouldHandleSingleValueRotate()
        {
            var matrix = SvgHelpers.ParseTransform("rotate(45)");
            // [sqrt(2)/2, -sqrt(2)/2, 0]
            // [sqrt(2)/2,  sqrt(2)/2, 0]
            // [0,          0,          1]
            Assert.AreEqual(new Matrix(Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, -Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, 0, 0), matrix);
        }

        [TestMethod]
        public void ParseTranform_ShouldHandleSingleValueRotateNegative()
        {
            var matrix = SvgHelpers.ParseTransform("rotate(-45)");
            // [sqrt(2)/2, sqrt(2)/2, 0]
            // [-sqrt(2)/2,  sqrt(2)/2, 0]
            // [0,          0,          1]
            Assert.AreEqual(new Matrix(Math.Sqrt(2) / 2, -Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, 0, 0), matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldHandleSingleValueSkewX()
        {
            var matrix = SvgHelpers.ParseTransform("skewX(30)");
            // [1, Math.Tan(30), 0]
            // [0, 1, 0]
            // [0, 0, 1]
            Assert.AreEqual(new Matrix(1, 0, Math.Tan(Math.PI / 6), 1, 0, 0), matrix);
            
        }

        [TestMethod]
        public void ParseTransform_ShouldHandleSingleValueSkewY()
        {
            var matrix = SvgHelpers.ParseTransform("skewY(30)");
            // [1, Math.Tan(30), 0]
            // [0, 1, 0]
            // [0, 0, 1]
            Assert.AreEqual(new Matrix(1, Math.Tan(Math.PI / 6), 0, 1, 0, 0), matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnIdentityMatrix_WhenTransformIsIdentity()
        {
            var matrix = SvgHelpers.ParseTransform("matrix(1, 0, 0, 1, 0, 0)");
            Assert.AreEqual(Matrix.Identity, matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnIdentityMatrix_WhenTransformIsEmptyMatrix()
        {
            var matrix = SvgHelpers.ParseTransform("matrix(1,0,0,1,20,20)");
            Assert.AreEqual(new Matrix(1,0,0,1,20,20), matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnIdentityMatrix_WhenTransformIsNone()
        {
            var matrix = SvgHelpers.ParseTransform("none");
            Assert.AreEqual(Matrix.Identity, matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnIdentityMatrix_WhenTransformIsEmptyWhitespace()
        {
            var matrix = SvgHelpers.ParseTransform("   ");
            Assert.AreEqual(Matrix.Identity, matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldThrowFormatException_WhenTransformIsInvalid()
        {
            Assert.ThrowsException<System.FormatException>(() => SvgHelpers.ParseTransform("invalid(10, 20)"));
        }

        [TestMethod]
        public void ParseTransform_ShouldThrowFormatException_WhenTransformHasInvalidParameters()
        {
            Assert.ThrowsException<System.FormatException>(() => SvgHelpers.ParseTransform("translate(10, invalid)"));
        }
    }
}
