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

        [TestMethod]
        public void ParseNestedGroups_AppliesInheritedTransformsAndStyles()
        {
            var svg = @"<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100'>
                <g fill='red' transform='translate(10,10)'>
                    <g fill='green' transform='scale(2)'>
                        <g fill='blue' transform='rotate(45)'>
                            <rect x='0' y='0' width='10' height='10'/>
                        </g>
                    </g>
                </g>
            </svg>";

            var doc = System.Xml.Linq.XDocument.Parse(svg);
            var parser = new SvgParser();
            var image = parser.SvgToImage(doc);
            Assert.IsNotNull(image);
            var rootGroup = image.Drawing as DrawingGroup;
            Assert.IsNotNull(rootGroup);
            // First child: <g fill='red' ...>
            Assert.IsTrue(rootGroup.Children.Count > 0);
            var group1 = rootGroup.Children[0] as DrawingGroup;
            Assert.IsNotNull(group1);
            Assert.IsNotNull(group1.Transform);
            Assert.IsInstanceOfType(group1.Transform, typeof(TranslateTransform));
            // Second child: <g fill='green' ...>
            Assert.IsTrue(group1.Children.Count > 0);
            var group2 = group1.Children[0] as DrawingGroup;
            Assert.IsNotNull(group2);
            Assert.IsNotNull(group2.Transform);
            Assert.IsInstanceOfType(group2.Transform, typeof(TransformGroup));
            var group2TransformGroup = (TransformGroup)group2.Transform;
            Assert.AreEqual(2, group2TransformGroup.Children.Count); // translate, scale
            Assert.IsInstanceOfType(group2TransformGroup.Children[0], typeof(TranslateTransform));
            Assert.IsInstanceOfType(group2TransformGroup.Children[1], typeof(ScaleTransform));
            // Third child: <g fill='blue' ...>
            Assert.IsTrue(group2.Children.Count > 0);
            var group3 = group2.Children[0] as DrawingGroup;
            Assert.IsNotNull(group3);
            Assert.IsNotNull(group3.Transform);
            Assert.IsInstanceOfType(group3.Transform, typeof(TransformGroup));
            var group3TransformGroup = (TransformGroup)group3.Transform;
            Assert.AreEqual(3, group3TransformGroup.Children.Count); // translate, scale, rotate
            Assert.IsInstanceOfType(group3TransformGroup.Children[0], typeof(TranslateTransform));
            Assert.IsInstanceOfType(group3TransformGroup.Children[1], typeof(ScaleTransform));
            Assert.IsInstanceOfType(group3TransformGroup.Children[2], typeof(RotateTransform));
            // Innermost child: <rect ...>
            Assert.IsTrue(group3.Children.Count > 0);
            var rectDrawing = group3.Children[0] as GeometryDrawing;
            Assert.IsNotNull(rectDrawing);
            // The fill should be blue (from innermost group)
            var solidBrush = rectDrawing.Brush as SolidColorBrush;
            Assert.IsNotNull(solidBrush);
            Assert.AreEqual(Colors.Blue, solidBrush.Color);
        }

        [TestMethod]
        public void ParseNestedGroups_FillOnGroup_AppliesInheritedOpacity()
        {
            var svg = @"<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100'>
                <g fill='red' opacity='0.5'>
                    <g opacity='0.5'>
                        <rect x='0' y='0' width='10' height='10'/>
                    </g>
                </g>
            </svg>";

            var doc = System.Xml.Linq.XDocument.Parse(svg);
            var parser = new SvgParser();
            var image = parser.SvgToImage(doc);
            Assert.IsNotNull(image);
            var rootGroup = image.Drawing as DrawingGroup;
            Assert.IsNotNull(rootGroup);
            // First child: <g fill='red' opacity=0.5>
            Assert.IsTrue(rootGroup.Children.Count > 0);
            var group1 = rootGroup.Children[0] as DrawingGroup;
            Assert.IsNotNull(group1);
            // Second child: <g opacity=0.5>
            Assert.IsTrue(group1.Children.Count > 0);
            var group2 = group1.Children[0] as DrawingGroup;
            Assert.IsNotNull(group2);
            // Inner object rect
            Assert.IsTrue(group2.Children.Count > 0);
            var rectDrawing = group2.Children[0] as GeometryDrawing;
            Assert.IsNotNull(rectDrawing);
            // The opacity should be 0.25 (from both groups)
            SolidColorBrush brush = (SolidColorBrush)rectDrawing.Brush;
            Assert.AreEqual((int)(255 * 0.25), brush.Color.A);
        }

        [TestMethod]
        public void ParseNestedGroups_FillOnElement_AppliesInheritedOpacity()
        {
            var svg = @"<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100'>
                <g opacity='0.5'>
                    <g opacity='0.5'>
                        <rect fill='red' x='0' y='0' width='10' height='10'/>
                    </g>
                </g>
            </svg>";

            var doc = System.Xml.Linq.XDocument.Parse(svg);
            var parser = new SvgParser();
            var image = parser.SvgToImage(doc);
            Assert.IsNotNull(image);
            var rootGroup = image.Drawing as DrawingGroup;
            Assert.IsNotNull(rootGroup);
            // First child: <g fill='red' opacity=0.5>
            Assert.IsTrue(rootGroup.Children.Count > 0);
            var group1 = rootGroup.Children[0] as DrawingGroup;
            Assert.IsNotNull(group1);
            // Second child: <g opacity=0.5>
            Assert.IsTrue(group1.Children.Count > 0);
            var group2 = group1.Children[0] as DrawingGroup;
            Assert.IsNotNull(group2);
            // Inner object rect
            Assert.IsTrue(group2.Children.Count > 0);
            var rectDrawing = group2.Children[0] as GeometryDrawing;
            Assert.IsNotNull(rectDrawing);
            // The opacity should be 0.25 (from both groups)
            SolidColorBrush brush = (SolidColorBrush)rectDrawing.Brush;
            Assert.AreEqual((int)(255 * 0.25), brush.Color.A);
        }

        [TestMethod]
        public void ParsePath_NoFillSpecifiedDefaultsToBlack()
        {
            var svg = @"<svg width=""200"" height=""200"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"">
                            <path d=""M10 20a10 10 0 1 1 0-20 10 10 0 0 1 0 20zm7.75-8a8.01 8.01 0 0 0 0-4h-3.82a28.81 28.81 0 0 1 0 4h3.82zm-.82 2h-3.22a14.44 14.44 0 0 1-.95 3.51A8.03 8.03 0 0 0 16.93 14zm-8.85-2h3.84a24.61 24.61 0 0 0 0-4H8.08a24.61 24.61 0 0 0 0 4zm.25 2c.41 2.4 1.13 4 1.67 4s1.26-1.6 1.67-4H8.33zm-6.08-2h3.82a28.81 28.81 0 0 1 0-4H2.25a8.01 8.01 0 0 0 0 4zm.82 2a8.03 8.03 0 0 0 4.17 3.51c-.42-.96-.74-2.16-.95-3.51H3.07zm13.86-8a8.03 8.03 0 0 0-4.17-3.51c.42.96.74 2.16.95 3.51h3.22zm-8.6 0h3.34c-.41-2.4-1.13-4-1.67-4S8.74 3.6 8.33 6zM3.07 6h3.22c.2-1.35.53-2.55.95-3.51A8.03 8.03 0 0 0 3.07 6z""/>
                        </svg> ";

            var doc = System.Xml.Linq.XDocument.Parse(svg);
            var parser = new SvgParser();
            var image = parser.SvgToImage(doc);
            Assert.IsNotNull(image);
            var rootGroup = image.Drawing as DrawingGroup;
            Assert.IsNotNull(rootGroup);
            // First child: <path>
            Assert.IsTrue(rootGroup.Children.Count > 0);
            var pathDrawing = rootGroup.Children[0] as GeometryDrawing;
            Assert.IsNotNull(pathDrawing);

            Assert.AreEqual(Brushes.Black, pathDrawing.Brush);
        }
    }
}
