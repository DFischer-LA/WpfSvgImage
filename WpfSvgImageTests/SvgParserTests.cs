using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using WpfSvgImage;
using WpfSvgImage.Elements;

namespace WpfSvgImageTests
{
    [TestClass]
    public class SvgParserTests
    {
        [TestMethod]
        public void ParsePath_ValidPathData_ReturnsGeometryDrawing()
        {
            var pathElement = XElement.Parse(@"<path d='M 10,10 L 100,100' fill='red' stroke='blue' stroke-width='2'/>");

            Defs defs = new Defs();

            var path = new WpfSvgImage.Elements.Path(pathElement, defs, null);
            var drawing = path.Parse();

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
            var ellipseElement = XElement.Parse(@"<ellipse cx='50' cy='60' rx='20' ry='10' fill='green'/>");

            Defs defs = new Defs();

            var ellipse = new Ellipse(ellipseElement, defs, null);
            var drawing = ellipse.Parse();

            Assert.IsNotNull(drawing);
            Assert.IsInstanceOfType(drawing.Geometry, typeof(EllipseGeometry));
            var ellipseGeometry = (EllipseGeometry)drawing.Geometry;
            Assert.AreEqual(new Point(50, 60), ellipseGeometry.Center);
            Assert.AreEqual(20, ellipseGeometry.RadiusX, 0.01);
            Assert.AreEqual(10, ellipseGeometry.RadiusY, 0.01);
        }

        [TestMethod]
        public void ParseRectangle_ValidRect_ReturnsGeometryDrawing()
        {
            var rectElement = XElement.Parse(@"<rect x='5' y='10' width='20' height='30' fill='blue'/>");

            Defs defs = new Defs();

            var rectangle = new Rectangle(rectElement, defs, null);
            var drawing = rectangle.Parse();
            Assert.IsNotNull(drawing);
            Assert.IsInstanceOfType(drawing.Geometry, typeof(RectangleGeometry));
            var rect = (RectangleGeometry)drawing.Geometry;
            Assert.AreEqual(new Rect(5, 10, 20, 30), rect.Rect);
        }

        [TestMethod]
        public void ParseCircle_ValidCircle_ReturnsGeometryDrawing()
        {
            var circleElement = XElement.Parse(@"<circle cx='15' cy='25' r='10' fill='yellow'/>");

            Defs defs = new Defs();

            var circle = new Circle(circleElement, defs, null);
            var drawing = circle.Parse();

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

        [TestMethod]
        public void ParseRectangle_WithTransformAndGradient_AppliesTransformToBrush()
        {
            var rectElement = XElement.Parse(@"
                <rect x='0' y='0' width='100' height='100' fill='url(#grad1)' transform='rotate(45)'/>
            ");
            var gradElement = XElement.Parse(@"
                <linearGradient id='grad1' x1='0' y1='0' x2='1' y2='1'>
                    <stop offset='0' stop-color='red'/>
                    <stop offset='1' stop-color='blue'/>
                </linearGradient>
            ");
            Defs defs = new Defs();

            var linearGradient = new LinearGradient(gradElement, defs, null);
            defs.Elements["grad1"] = linearGradient.Parse();

            var rectangle = new Rectangle(rectElement, defs, null);
            var drawing = rectangle.Parse();

            Assert.IsNotNull(drawing.Brush);
            Assert.IsInstanceOfType(drawing.Brush, typeof(LinearGradientBrush));
            var brush = (LinearGradientBrush)drawing.Brush;
            Assert.IsInstanceOfType(brush.Transform, typeof(TransformGroup));
        }

        [TestMethod]
        public void ParsePath_WithFillRuleEvenOdd_SetsFillRule()
        {
            var pathElement = XElement.Parse(@"<path d='M0,0 L10,0 L10,10 L0,10 Z' fill-rule='evenodd'/>");
            Defs defs = new Defs();

            var path = new WpfSvgImage.Elements.Path(pathElement, defs, null);
            var drawing = path.Parse();

            Assert.IsInstanceOfType(drawing.Geometry, typeof(PathGeometry));
            var pathGeometry = (PathGeometry)drawing.Geometry;
            Assert.AreEqual(FillRule.EvenOdd, pathGeometry.FillRule);
        }

        [TestMethod]
        public void ParseEllipse_WithStyleAttribute_AppliesStyle()
        {
            var ellipseElement = XElement.Parse(@"<ellipse cx='10' cy='20' rx='5' ry='5' style='fill: #123456; stroke: #654321; stroke-width: 3;'/>");
            Defs defs = new Defs();

            var ellipse = new Ellipse(ellipseElement, defs, null);
            var drawing = ellipse.Parse();

            Assert.IsNotNull(drawing.Brush);
            Assert.IsNotNull(drawing.Pen);
            Assert.AreEqual(3, drawing.Pen.Thickness, 0.01);
        }

        [TestMethod]
        public void ParsePolygon_ClosesShape()
        {
            var polygonElement = XElement.Parse(@"<polygon points='0,0 10,0 10,10 0,10'/>");
            Defs defs = new Defs();

            var polygon = new Polygon(polygonElement, defs, null);
            var drawing = polygon.Parse();

            Assert.IsInstanceOfType(drawing.Geometry, typeof(PathGeometry));
            var pathGeometry = (PathGeometry)drawing.Geometry;
            var figure = pathGeometry.Figures[0];
            Assert.IsTrue(figure.Segments.Count > 0);
            var segment = figure.Segments[0] as PolyLineSegment;
            Assert.IsNotNull(segment);
            Assert.AreEqual(segment.Points[0], segment.Points[segment.Points.Count - 1]);
        }

        [TestMethod]
        public void ParseText_WithFontAttributes_AppliesFontSettings()
        {
            var textElement = XElement.Parse(@"<text x='5' y='15' font-family='Arial' font-size='24' font-weight='Bold'>A</text>");
            Defs defs = new Defs();

            var text = new Text(textElement, defs, null);
            var drawing = text.Parse();

            Assert.IsNotNull(drawing.GlyphRun);
            Assert.AreEqual(new Point(5, 15), drawing.GlyphRun.BaselineOrigin);
            Assert.AreEqual(24, drawing.GlyphRun.FontRenderingEmSize, 0.01);
        }

        [TestMethod]
        public void ParseLinearGradient_WithGradientUnitsUserSpaceOnUse_SetsMappingModeAbsolute()
        {
            var gradElement = XElement.Parse(@"
                <linearGradient id='g' gradientUnits='userSpaceOnUse'>
                    <stop offset='0' stop-color='red'/>
                    <stop offset='1' stop-color='blue'/>
                </linearGradient>
            ");
            Defs defs = new Defs();

            var linearGradient = new LinearGradient(gradElement, defs, null);
            var brush = linearGradient.Parse();

            Assert.IsInstanceOfType(brush, typeof(LinearGradientBrush));
            Assert.AreEqual(BrushMappingMode.Absolute, ((LinearGradientBrush)brush).MappingMode);
        }

        [TestMethod]
        public void ParseRadialGradient_WithSpreadMethodReflect_SetsSpreadMethod()
        {
            var gradElement = XElement.Parse(@"
                <radialGradient id='g' spreadMethod='reflect'>
                    <stop offset='0' stop-color='red'/>
                    <stop offset='1' stop-color='blue'/>
                </radialGradient>
            ");
            Defs defs = new Defs();

            var radialGradient = new RadialGradient(gradElement, defs, null);
            var brush = radialGradient.Parse();

            Assert.IsInstanceOfType(brush, typeof(RadialGradientBrush));
            Assert.AreEqual(GradientSpreadMethod.Reflect, ((RadialGradientBrush)brush).SpreadMethod);
        }
    }
}
