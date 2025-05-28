using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfSvgImage;

namespace WpfSvgImageTests
{
    [TestClass]
    public class SvgParserTests
    {
        [TestMethod]
        public void ParsePath_ValidPathData_ReturnsGeometryDrawing()
        {
            var parser = new SvgParser();
            var pathElement = XElement.Parse(@"<path d='M 10,10 L 100,100' fill='red' stroke='blue' stroke-width='2'/>");

            var drawing = parser.ParsePath(pathElement, null);

            Assert.IsNotNull(drawing);
            Assert.IsInstanceOfType(drawing, typeof(GeometryDrawing));
            Assert.IsInstanceOfType(drawing.Geometry, typeof(PathGeometry));
            Assert.IsNotNull(drawing.Brush);
            Assert.IsNotNull(drawing.Pen);
            Assert.AreEqual(2, drawing.Pen.Thickness, 0.01);
        }

        [TestMethod]
        public void ParseEllipse_ValidEllipse_ReturnsGeometryDrawing()
        {
            var parser = new SvgParser();
            var ellipseElement = XElement.Parse(@"<ellipse cx='50' cy='60' rx='20' ry='10' fill='green'/>");

            var drawing = parser.ParseEllipse(ellipseElement, null);

            Assert.IsNotNull(drawing);
            Assert.IsInstanceOfType(drawing.Geometry, typeof(EllipseGeometry));
            var ellipse = (EllipseGeometry)drawing.Geometry;
            Assert.AreEqual(new Point(50, 60), ellipse.Center);
            Assert.AreEqual(20, ellipse.RadiusX, 0.01);
            Assert.AreEqual(10, ellipse.RadiusY, 0.01);
        }

        [TestMethod]
        public void ParseRectangle_ValidRect_ReturnsGeometryDrawing()
        {
            var parser = new SvgParser();
            var rectElement = XElement.Parse(@"<rect x='5' y='10' width='20' height='30' fill='blue'/>");

            var drawing = parser.ParseRectangle(rectElement, null);

            Assert.IsNotNull(drawing);
            Assert.IsInstanceOfType(drawing.Geometry, typeof(RectangleGeometry));
            var rect = (RectangleGeometry)drawing.Geometry;
            Assert.AreEqual(new Rect(5, 10, 20, 30), rect.Rect);
        }

        [TestMethod]
        public void ParseCircle_ValidCircle_ReturnsGeometryDrawing()
        {
            var parser = new SvgParser();
            var circleElement = XElement.Parse(@"<circle cx='15' cy='25' r='10' fill='yellow'/>");

            var drawing = parser.ParseCircle(circleElement, null);

            Assert.IsNotNull(drawing);
            Assert.IsInstanceOfType(drawing.Geometry, typeof(EllipseGeometry));
            var ellipse = (EllipseGeometry)drawing.Geometry;
            Assert.AreEqual(new Point(15, 25), ellipse.Center);
            Assert.AreEqual(10, ellipse.RadiusX, 0.01);
            Assert.AreEqual(10, ellipse.RadiusY, 0.01);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SvgToImage_InvalidRoot_Throws()
        {
            var parser = new SvgParser();
            var doc = XDocument.Parse("<notSvg></notSvg>");
            parser.SvgToImage(doc);
        }
    }
}
