using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfSvgImage;
using WpfSvgImage.Elements;

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
            Assert.AreEqual(Transform.Identity, matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnTranslationMatrix_WhenTransformIsTranslate()
        {
            var transform = SvgHelpers.ParseTransform("translate(10, 20)");

            Assert.IsInstanceOfType(transform, typeof(TranslateTransform));
            Assert.AreEqual(new TranslateTransform(10, 20).X, ((TranslateTransform)transform).X = 10);
            Assert.AreEqual(new TranslateTransform(10, 20).Y, ((TranslateTransform)transform).Y = 20);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnScaleMatrix_WhenTransformIsScale()
        {
            var transform = SvgHelpers.ParseTransform("scale(2, 3)");

            Assert.IsInstanceOfType(transform, typeof(ScaleTransform));
            Assert.AreEqual(new ScaleTransform(2,3).ScaleX, ((ScaleTransform)transform).ScaleX);
            Assert.AreEqual(new ScaleTransform(2,3).ScaleY, ((ScaleTransform)transform).ScaleY);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnCombinedMatrix_WhenTransformContainsMultipleCommands()
        {
            var transform = SvgHelpers.ParseTransform("translate(10, 20) scale(2, 3)");

            Assert.IsInstanceOfType(transform, typeof(TransformGroup));
            Assert.AreEqual(new Matrix(2, 0, 0, 3, 20, 60), transform.Value);
        }

        [TestMethod]
        public void ParseTransform_ShouldHandleMultipleSpacesAndCommas()
        {
            var transform = SvgHelpers.ParseTransform("translate(10, 20) , scale(2, 3)");

            Assert.IsInstanceOfType(transform, typeof(TransformGroup));
            Assert.AreEqual(new MatrixTransform(new Matrix(2, 0, 0, 3, 20, 60)).Matrix, ((TransformGroup)transform).Value);
        }

        [TestMethod]
        public void ParseTransform_ShouldHandleSingleValueTranslate()
        {
            var transform = SvgHelpers.ParseTransform("translate(10)");

            Assert.IsInstanceOfType(transform, typeof(TranslateTransform));
            Assert.AreEqual(new TranslateTransform(10,0).X, ((TranslateTransform)transform).X);
            Assert.AreEqual(0, ((TranslateTransform)transform).Y);
        }

        [TestMethod]
        public void ParseTransform_ShouldHandleSingleValueScale()
        {
            var transform = SvgHelpers.ParseTransform("scale(2)");

            Assert.IsInstanceOfType(transform, typeof(ScaleTransform));
            Assert.AreEqual(new ScaleTransform(2,2).ScaleX, ((ScaleTransform)transform).ScaleX);
            Assert.AreEqual(new ScaleTransform(2, 2).ScaleY, ((ScaleTransform)transform).ScaleY);
        }

        [TestMethod]
        public void ParseTranform_ShouldHandleSingleValueRotate()
        {
            var transform = SvgHelpers.ParseTransform("rotate(45)");

            Assert.IsInstanceOfType(transform, typeof(RotateTransform));
            Assert.AreEqual(new RotateTransform(45).Angle, ((RotateTransform)transform).Angle );
        }

        [TestMethod]
        public void ParseTranform_ShouldHandleSingleValueRotateNegative()
        {
            var transform = SvgHelpers.ParseTransform("rotate(-45)");
            
            Assert.IsInstanceOfType(transform, typeof(RotateTransform));
            Assert.AreEqual(new RotateTransform(-45).Angle, ((RotateTransform)transform).Angle);
        }

        [TestMethod]
        public void ParseTransform_ShouldHandleSingleValueSkewX()
        {
            var transform = SvgHelpers.ParseTransform("skewX(30)");
           
            Assert.IsInstanceOfType(transform, typeof(SkewTransform));
            Assert.AreEqual(new SkewTransform(30,0).AngleX, ((SkewTransform)transform).AngleX);
            
        }

        [TestMethod]
        public void ParseTransform_ShouldHandleSingleValueSkewY()
        {
            var transform = SvgHelpers.ParseTransform("skewY(30)");

            Assert.IsInstanceOfType(transform, typeof(SkewTransform));
            Assert.AreEqual(new SkewTransform(0,30).AngleY, ((SkewTransform)transform).AngleY);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnIdentityMatrix_WhenTransformIsIdentity()
        {
            var transform = SvgHelpers.ParseTransform("matrix(1, 0, 0, 1, 0, 0)");
            Assert.AreEqual(Matrix.Identity, transform.Value);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnIdentityMatrix_WhenTransformIsEmptyMatrix()
        {
            var transform = SvgHelpers.ParseTransform("matrix(1,0,0,1,20,20)");

            Assert.IsInstanceOfType(transform, typeof(MatrixTransform));
            Assert.AreEqual(new Matrix(1, 0, 0, 1, 20, 20), transform.Value);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnIdentityMatrix_WhenTransformIsNone()
        {
            var matrix = SvgHelpers.ParseTransform("none");
            Assert.AreEqual(Transform.Identity, matrix);
        }

        [TestMethod]
        public void ParseTransform_ShouldReturnIdentityMatrix_WhenTransformIsEmptyWhitespace()
        {
            var matrix = SvgHelpers.ParseTransform("   ");
            Assert.AreEqual(Transform.Identity, matrix);
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

        [TestMethod]
        public void ParseStyle_ParsesMultipleProperties()
        {
            var style = SvgHelpers.ParseStyle("fill: #123456; stroke: #654321;", new Defs());
            Assert.IsTrue(style.ContainsKey("fill"));
            Assert.IsTrue(style.ContainsKey("stroke"));
        }

        [TestMethod]
        public void ApplyOpacityToColor_ChangesAlpha()
        {
            var color = Color.FromArgb(255, 10, 20, 30);
            var result = SvgHelpers.ApplyOpacityToColor(color, 0.5);
            Assert.AreEqual(127, result.A, 1);
        }

        [TestMethod]
        public void IsUseReference_RecognizesUrlSyntax()
        {
            bool isRef = SvgHelpers.IsUseReference("url(#myId)", out string id);
            Assert.IsTrue(isRef);
            Assert.AreEqual("myId", id);
        }
    }
}
