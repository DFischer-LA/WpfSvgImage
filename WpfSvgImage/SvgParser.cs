
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace WpfSvgImage
{
    /// <summary>
    /// Holds inherited SVG group properties such as stroke, fill, and fill rule.
    /// </summary>
    internal class SvgGroupProperties
    {
        public string? Stroke { get; set; }
        public double? StrokeWidth { get; set; }
        public string? Fill { get; set; }
        public string? FillRule { get; set; }
    }

    /// <summary>
    /// Parses SVG XML elements and converts them into WPF Drawing objects.
    /// </summary>
    internal class SvgParser
    {
        /// <summary>
        /// Converts an SVG XDocument to a WPF DrawingImage.
        /// </summary>
        /// <param name="svgXDoc">The SVG XML document.</param>
        /// <returns>A DrawingImage representing the SVG.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the root element is not &lt;svg&gt;.</exception>
        public DrawingImage SvgToImage(XDocument svgXDoc)
        {
            var root = svgXDoc.Root;
            if (root == null || root.Name.LocalName != SvgNames.svg)
            {
                // Root element of an SVG file must be the <svg> element.
                throw new InvalidOperationException("Invalid SVG document.");
            }

            // Parse the SVG and create a DrawingGroup.
            var drawing = ParseSVG(root);

            // Create the DrawingImage for WPF rendering.
            DrawingImage geometryImage = new DrawingImage(drawing);
            geometryImage.Freeze(); // Freeze for performance.

            return geometryImage;
        }

        /// <summary>
        /// Parses the root SVG element and returns a DrawingGroup.
        /// </summary>
        /// <param name="svgElement">The root SVG XElement.</param>
        /// <returns>A DrawingGroup representing the SVG content.</returns>
        internal DrawingGroup ParseSVG(XElement svgElement)
        {
            DrawingGroup rootDrawingGroup = ParseGroup(svgElement, null);

            // Optionally handle width/height attributes here if needed.
            foreach (var attribute in svgElement.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.width:
                        // Handle width if needed.
                        break;
                    case SvgNames.height:
                        // Handle height if needed.
                        break;
                }
            }

            return rootDrawingGroup;
        }

        /// <summary>
        /// Recursively parses an SVG group (&lt;g&gt;) or the root element and its children.
        /// </summary>
        /// <param name="element">The group or root XElement.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A DrawingGroup for the group and its children.</returns>
        internal DrawingGroup ParseGroup(XElement element, SvgGroupProperties? groupProperties)
        {
            var group = new DrawingGroup();

            // Inherit or override group properties from attributes.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.stroke:
                        groupProperties ??= new SvgGroupProperties();
                        groupProperties.Stroke = attribute.Value;
                        break;
                    case SvgNames.strokeWidth:
                        groupProperties ??= new SvgGroupProperties();
                        if (double.TryParse(attribute.Value, out double strokeWidth))
                        {
                            groupProperties.StrokeWidth = strokeWidth;
                        }
                        break;
                    case SvgNames.fill:
                        groupProperties ??= new SvgGroupProperties();
                        groupProperties.Fill = attribute.Value;
                        break;
                    case SvgNames.fillRule:
                        groupProperties ??= new SvgGroupProperties();
                        groupProperties.FillRule = attribute.Value;
                        break;
                }
            }

            // Parse child elements.
            foreach (var childElement in element.Elements())
            {
                switch (childElement.Name.LocalName)
                {
                    case SvgNames.g:
                        group.Children.Add(ParseGroup(childElement, groupProperties));
                        break;
                    case SvgNames.path:
                        group.Children.Add(ParsePath(childElement, groupProperties));
                        break;
                    case SvgNames.ellipse:
                        group.Children.Add(ParseEllipse(childElement, groupProperties));
                        break;
                    case SvgNames.rect:
                        group.Children.Add(ParseRectangle(childElement, groupProperties));
                        break;
                    case SvgNames.circle:
                        group.Children.Add(ParseCircle(childElement, groupProperties));
                        break;
                    case SvgNames.line:
                        group.Children.Add(ParseLine(childElement, groupProperties));
                        break;
                    case SvgNames.polyline:
                        group.Children.Add(ParsePolyline(childElement, groupProperties));
                        break;
                    case SvgNames.polygon:
                        group.Children.Add(ParsePolygon(childElement, groupProperties));
                        break;
                    case SvgNames.text:
                        group.Children.Add(ParseText(childElement, groupProperties));
                        break;
                }
            }

            return group;
        }

        /// <summary>
        /// Parses an SVG &lt;path&gt; element into a GeometryDrawing.
        /// </summary>
        /// <param name="element">The path XElement.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the path.</returns>
        internal GeometryDrawing ParsePath(XElement element, SvgGroupProperties? groupProperties)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            PathGeometry pathGeometry = new PathGeometry();
            geometryDrawing.Geometry = pathGeometry;
            pathGeometry.FillRule = groupProperties?.FillRule == SvgNames.evenodd ? FillRule.EvenOdd : FillRule.Nonzero;

            PopulateStrokeInfo(geometryDrawing, element);
            PopulateFillInfo(geometryDrawing, element);
            PopulateTransformInfo(pathGeometry, element);

            // Parse path data and fill rule.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.d:
                        // Convert SVG path data to PathFigureCollection.
                        PathFigureCollectionConverter converter = new PathFigureCollectionConverter();
                        var figureCollection = converter.ConvertFromString(attribute.Value) as PathFigureCollection;
                        if (figureCollection != null)
                        {
                            for (int i = 0; i < figureCollection.Count; i++)
                            {
                                var figure = figureCollection[i];
                                pathGeometry.Figures.Add(figure);
                            }
                        }
                        break;
                    case SvgNames.fillRule:
                        pathGeometry.FillRule = attribute.Value == SvgNames.evenodd ? FillRule.EvenOdd : FillRule.Nonzero;
                        break;
                }
            }

            return geometryDrawing;
        }

        /// <summary>
        /// Parses an SVG &lt;ellipse&gt; element into a GeometryDrawing.
        /// </summary>
        internal GeometryDrawing ParseEllipse(XElement element, SvgGroupProperties? groupProperties)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            EllipseGeometry ellipseGeometry = new EllipseGeometry();
            geometryDrawing.Geometry = ellipseGeometry;

            PopulateStrokeInfo(geometryDrawing, element);
            PopulateFillInfo(geometryDrawing, element);
            PopulateTransformInfo(ellipseGeometry, element);

            // Parse ellipse attributes.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.cx:
                        if (double.TryParse(attribute.Value, out double centerX))
                        {
                            ellipseGeometry.Center = new Point(centerX, ellipseGeometry.Center.Y);
                        }
                        break;
                    case SvgNames.cy:
                        if (double.TryParse(attribute.Value, out double centerY))
                        {
                            ellipseGeometry.Center = new Point(ellipseGeometry.Center.X, centerY);
                        }
                        break;
                    case SvgNames.rx:
                        if (double.TryParse(attribute.Value, out double radiusX))
                        {
                            ellipseGeometry.RadiusX = radiusX;
                        }
                        break;
                    case SvgNames.ry:
                        if (double.TryParse(attribute.Value, out double radiusY))
                        {
                            ellipseGeometry.RadiusY = radiusY;
                        }
                        break;
                }
            }

            return geometryDrawing;
        }

        /// <summary>
        /// Parses an SVG &lt;circle&gt; element into a GeometryDrawing.
        /// </summary>
        internal GeometryDrawing ParseCircle(XElement element, SvgGroupProperties? groupProperties)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            EllipseGeometry ellipseGeometry = new EllipseGeometry();
            geometryDrawing.Geometry = ellipseGeometry;

            PopulateStrokeInfo(geometryDrawing, element);
            PopulateFillInfo(geometryDrawing, element);
            PopulateTransformInfo(ellipseGeometry, element);

            // Parse circle attributes.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.cx:
                        if (double.TryParse(attribute.Value, out double centerX))
                        {
                            ellipseGeometry.Center = new Point(centerX, ellipseGeometry.Center.Y);
                        }
                        break;
                    case SvgNames.cy:
                        if (double.TryParse(attribute.Value, out double centerY))
                        {
                            ellipseGeometry.Center = new Point(ellipseGeometry.Center.X, centerY);
                        }
                        break;
                    case SvgNames.r:
                        if (double.TryParse(attribute.Value, out double radius))
                        {
                            ellipseGeometry.RadiusX = radius;
                            ellipseGeometry.RadiusY = radius;
                        }
                        break;
                }
            }

            return geometryDrawing;
        }

        /// <summary>
        /// Parses an SVG &lt;rect&gt; element into a GeometryDrawing.
        /// </summary>
        internal GeometryDrawing ParseRectangle(XElement element, SvgGroupProperties? groupProperties)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            geometryDrawing.Geometry = rectangleGeometry;

            PopulateStrokeInfo(geometryDrawing, element);
            PopulateFillInfo(geometryDrawing, element);
            PopulateTransformInfo(rectangleGeometry, element);

            // Parse rectangle attributes.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.x:
                        if (double.TryParse(attribute.Value, out double x))
                        {
                            rectangleGeometry.Rect = new Rect(x, rectangleGeometry.Rect.Y, Math.Max(0, rectangleGeometry.Rect.Width), Math.Max(0, rectangleGeometry.Rect.Height));
                        }
                        break;
                    case SvgNames.y:
                        if (double.TryParse(attribute.Value, out double y))
                        {
                            rectangleGeometry.Rect = new Rect(rectangleGeometry.Rect.X, y, Math.Max(0, rectangleGeometry.Rect.Width), Math.Max(0, rectangleGeometry.Rect.Height));
                        }
                        break;
                    case SvgNames.width:
                        if (double.TryParse(attribute.Value, out double width))
                        {
                            rectangleGeometry.Rect = new Rect(rectangleGeometry.Rect.X, rectangleGeometry.Rect.Y, width, Math.Max(0, rectangleGeometry.Rect.Height));
                        }
                        break;
                    case SvgNames.height:
                        if (double.TryParse(attribute.Value, out double height))
                        {
                            rectangleGeometry.Rect = new Rect(rectangleGeometry.Rect.X, rectangleGeometry.Rect.Y, Math.Max(0, rectangleGeometry.Rect.Width), height);
                        }
                        break;
                    case SvgNames.rx:
                        if (double.TryParse(attribute.Value, out double radiusX))
                        {
                            rectangleGeometry.RadiusX = radiusX;
                        }
                        break;
                    case SvgNames.ry:
                        if (double.TryParse(attribute.Value, out double radiusY))
                        {
                            rectangleGeometry.RadiusY = radiusY;
                        }
                        break;
                }
            }

            return geometryDrawing;
        }

        /// <summary>
        /// Parses an SVG &lt;line&gt; element into a GeometryDrawing.
        /// </summary>
        internal GeometryDrawing ParseLine(XElement element, SvgGroupProperties? groupProperties)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            LineGeometry lineGeometry = new LineGeometry();
            geometryDrawing.Geometry = lineGeometry;

            PopulateStrokeInfo(geometryDrawing, element);
            PopulateTransformInfo(lineGeometry, element);

            // Parse line attributes.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.x1:
                        if (double.TryParse(attribute.Value, out double x1))
                        {
                            lineGeometry.StartPoint = new Point(x1, lineGeometry.StartPoint.Y);
                        }
                        break;
                    case SvgNames.y1:
                        if (double.TryParse(attribute.Value, out double y1))
                        {
                            lineGeometry.StartPoint = new Point(lineGeometry.StartPoint.X, y1);
                        }
                        break;
                    case SvgNames.x2:
                        if (double.TryParse(attribute.Value, out double x2))
                        {
                            lineGeometry.EndPoint = new Point(x2, lineGeometry.EndPoint.Y);
                        }
                        break;
                    case SvgNames.y2:
                        if (double.TryParse(attribute.Value, out double y2))
                        {
                            lineGeometry.EndPoint = new Point(lineGeometry.EndPoint.X, y2);
                        }
                        break;
                }
            }

            return geometryDrawing;
        }

        /// <summary>
        /// Parses an SVG &lt;polyline&gt; element into a GeometryDrawing.
        /// </summary>
        internal GeometryDrawing ParsePolyline(XElement element, SvgGroupProperties? groupProperties)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            PathGeometry pathGeometry = new PathGeometry();
            geometryDrawing.Geometry = pathGeometry;

            PopulateStrokeInfo(geometryDrawing, element);
            PopulateFillInfo(geometryDrawing, element);
            PopulateTransformInfo(pathGeometry, element);

            // Parse polyline points.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.points:
                        PathFigure polyLineFigure = new PathFigure();
                        PointCollectionConverter converter = new PointCollectionConverter();
                        PolyLineSegment segment = new PolyLineSegment();
                        segment.Points = converter.ConvertFrom(attribute.Value) as PointCollection;
                        polyLineFigure.Segments.Add(segment);
                        pathGeometry.Figures.Add(polyLineFigure);
                        break;
                }
            }

            return geometryDrawing;
        }

        /// <summary>
        /// Parses an SVG &lt;polygon&gt; element into a GeometryDrawing.
        /// </summary>
        internal GeometryDrawing ParsePolygon(XElement element, SvgGroupProperties? groupProperties)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            PathGeometry pathGeometry = new PathGeometry();
            geometryDrawing.Geometry = pathGeometry;

            PopulateStrokeInfo(geometryDrawing, element);
            PopulateFillInfo(geometryDrawing, element);
            PopulateTransformInfo(pathGeometry, element);

            // Parse polygon points and ensure closure.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.points:
                        PathFigure polyLineFigure = new PathFigure();
                        PointCollectionConverter converter = new PointCollectionConverter();
                        PolyLineSegment segment = new PolyLineSegment();
                        segment.Points = converter.ConvertFrom(attribute.Value) as PointCollection;
                        if (segment.Points == null)
                            break;
                        if (segment.Points[0] != segment.Points[segment.Points.Count - 1])
                        {
                            // Close the polygon by adding the first point at the end.
                            segment.Points.Add(segment.Points[0]);
                        }
                        polyLineFigure.Segments.Add(segment);
                        pathGeometry.Figures.Add(polyLineFigure);
                        break;
                }
            }

            return geometryDrawing;
        }

        /// <summary>
        /// Parses an SVG &lt;text&gt; element into a GlyphRunDrawing.
        /// </summary>
        internal GlyphRunDrawing ParseText(XElement element, SvgGroupProperties? groupProperties)
        {
            GlyphRunDrawing glyphRunDrawing = new GlyphRunDrawing();

            // Get system DPI for text rendering.
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            int dpiX = 120;
            if (dpiXProperty != null)
            {
                object? oDpiX = dpiXProperty.GetValue(null, null);
                if (oDpiX != null)
                {
                    dpiX = (int)oDpiX;
                }
            }
            float pixelsPerDip = dpiX / 96f;

            // Default text properties.
            FontFamily fontFamily = new FontFamily("Arial");
            FontWeight fontWeight = FontWeights.Normal;
            FontStyle fontStyle = FontStyles.Normal;
            double fontSize = 12;
            double x = 0;
            double y = 0;

            // Parse text attributes.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.fill:
                        glyphRunDrawing.ForegroundBrush = SvgHelpers.ParseBrush(attribute.Value);
                        break;
                    case SvgNames.fontFamily:
                        FontFamilyConverter fontFamilyConverter = new FontFamilyConverter();
                        if (fontFamilyConverter.ConvertFrom(attribute.Value) is FontFamily family)
                        {
                            fontFamily = family;
                        }
                        break;
                    case SvgNames.fontSize:
                        FontSizeConverter fontSizeConverter = new FontSizeConverter();
                        if (fontSizeConverter.ConvertFrom(attribute.Value) is double size)
                        {
                            fontSize = size;
                        }
                        break;
                    case SvgNames.fontWeight:
                        FontWeightConverter fontWeightConverter = new FontWeightConverter();
                        if (fontWeightConverter.ConvertFrom(attribute.Value) is FontWeight weight)
                        {
                            fontWeight = weight;
                        }
                        break;
                    case SvgNames.fontStyle:
                        FontStyleConverter fontStyleConverter = new FontStyleConverter();
                        if (fontStyleConverter.ConvertFrom(attribute.Value) is FontStyle style)
                        {
                            fontStyle = style;
                        }
                        break;
                    case SvgNames.x:
                        if (double.TryParse(attribute.Value, out double xValue))
                        {
                            x = xValue;
                        }
                        break;
                    case SvgNames.y:
                        if (double.TryParse(attribute.Value, out double yValue))
                        {
                            y = yValue;
                        }
                        break;
                }
            }

            // Build the GlyphRun for the text.
            Typeface typeface = new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal);
            GlyphTypeface typeFace;
            if (typeface.TryGetGlyphTypeface(out typeFace))
            {
                double textWidth = 0;
                var glyphIndices = new ushort[element.Value.Length];
                var glyphAdvanceWidths = new double[element.Value.Length];
                var glyphOffsets = new Point[element.Value.Length];
                var characters = element.Value.ToCharArray();
                for (int i = 0; i < element.Value.Length; i++)
                {
                    char c = element.Value[i];
                    ushort glyphIndex = typeFace.CharacterToGlyphMap[c];
                    glyphIndices[i] = glyphIndex;

                    double width = typeFace.AdvanceWidths[glyphIndex] * fontSize;
                    glyphAdvanceWidths[i] = width;

                    glyphOffsets[i] = new Point(0, 0); // No offset for now.

                    textWidth += width;
                }

                GlyphRun glyphRun = new GlyphRun(
                        typeFace,
                        0,
                        false, // isSideways
                        fontSize, // Font rendering size
                        pixelsPerDip, // pixels per dip
                        glyphIndices, // Glyph indices
                        new Point(x, y), // Baseline origin
                        glyphAdvanceWidths, // Advance widths
                        glyphOffsets, // Glyph offsets
                        characters, // Characters
                        null, // Device font name
                        null, // Cluster map
                        null, // Caret stops
                        null); // Language

                glyphRunDrawing.GlyphRun = glyphRun;
            }

            return glyphRunDrawing;
        }

        /// <summary>
        /// Sets the stroke (Pen) on a GeometryDrawing from SVG attributes.
        /// </summary>
        internal void PopulateStrokeInfo(GeometryDrawing drawing, XElement element)
        {
            var strokeAttribute = element.Attribute(SvgNames.stroke);
            var strokeWidthAtttribute = element.Attribute(SvgNames.strokeWidth);

            if (strokeAttribute != null)
            {
                double strokeWidth = -1;
                if (strokeWidthAtttribute != null)
                {
                    double.TryParse(strokeWidthAtttribute.Value, out strokeWidth);
                }
                drawing.Pen = new Pen(SvgHelpers.ParseBrush(strokeAttribute.Value), strokeWidth > 0 ? strokeWidth : 1);
            }
        }

        /// <summary>
        /// Sets the fill (Brush) on a GeometryDrawing from SVG attributes.
        /// </summary>
        internal void PopulateFillInfo(GeometryDrawing drawing, XElement element)
        {
            var fillAttribute = element.Attribute(SvgNames.fill);
            if (fillAttribute != null)
            {
                drawing.Brush = SvgHelpers.ParseBrush(fillAttribute.Value);
            }
        }

        /// <summary>
        /// Applies a transform to a Geometry from the SVG 'transform' attribute.
        /// </summary>
        internal void PopulateTransformInfo(Geometry geometry, XElement element)
        {
            var transformAttribute = element.Attribute(SvgNames.transform);
            if (transformAttribute != null)
            {
                string transformData = transformAttribute.Value;
                Matrix transformMatrix = SvgHelpers.ParseTransform(transformData);

                TransformConverter converter = new TransformConverter();
                var transform = converter.ConvertFromString(transformMatrix.ToString()) as Transform;
                if (transform != null)
                {
                    geometry.Transform = transform;
                }
            }
        }

        /// <summary>
        /// Applies inherited group properties (stroke, fill) to a GeometryDrawing.
        /// </summary>
        internal void PopulateInheritedGroupProperties(GeometryDrawing drawing, SvgGroupProperties? groupProperties)
        {
            if (groupProperties != null)
            {
                if (!string.IsNullOrEmpty(groupProperties.Stroke))
                {
                    drawing.Pen = new Pen(SvgHelpers.ParseBrush(groupProperties.Stroke), groupProperties.StrokeWidth ?? 1);
                }
                if (!string.IsNullOrEmpty(groupProperties.Fill))
                {
                    drawing.Brush = SvgHelpers.ParseBrush(groupProperties.Fill);
                }
            }
        }
    }
}
