using System.Reflection;
using System.Windows;
using System.Windows.Markup.Localizer;
using System.Windows.Media;
using System.Xml.Linq;
using WpfSvgImage.Elements;

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

            // Stores any defs-defined resources for this specific SVG document.
            Defs reusableDefinitions = new Defs();

            // Parse the SVG and create a DrawingGroup.
            var drawing = ParseSVG(root, reusableDefinitions);

            // Create the DrawingImage for WPF rendering.
            DrawingImage geometryImage = new DrawingImage(drawing);
            geometryImage.Freeze(); // Freeze for performance.

            return geometryImage;
        }

        /// <summary>
        /// Parses the root SVG element and returns a DrawingGroup.
        /// </summary>
        /// <param name="svgElement">The root SVG XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for elements like gradients.</param>
        /// <returns>A DrawingGroup representing the SVG content.</returns>
        internal DrawingGroup ParseSVG(XElement svgElement, Defs reusableDefinitions)
        {
            // Parse the root SVG element and its children.
            DrawingGroup rootDrawingGroup = ParseGroup(svgElement, reusableDefinitions, null);

            return rootDrawingGroup;
        }

        /// <summary>
        /// Recursively parses an SVG group (&lt;g&gt;) or the root element and its children.
        /// </summary>
        /// <param name="element">The group or root XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for elements like gradients.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A DrawingGroup for the group and its children.</returns>
        internal DrawingGroup ParseGroup(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
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

            // Parse child elements and add their drawings to the group.
            foreach (var childElement in element.Elements())
            {
                switch (childElement.Name.LocalName)
                {
                    case SvgNames.g:
                        group.Children.Add(ParseGroup(childElement, reusableDefinitions, groupProperties));
                        break;
                    case SvgNames.defs:
                        ParseDefinitions(childElement, reusableDefinitions, groupProperties);
                        break;
                    case SvgNames.path:
                        group.Children.Add(ParsePath(childElement, reusableDefinitions, groupProperties));
                        break;
                    case SvgNames.ellipse:
                        group.Children.Add(ParseEllipse(childElement, reusableDefinitions, groupProperties));
                        break;
                    case SvgNames.rect:
                        group.Children.Add(ParseRectangle(childElement, reusableDefinitions, groupProperties));
                        break;
                    case SvgNames.circle:
                        group.Children.Add(ParseCircle(childElement, reusableDefinitions, groupProperties));
                        break;
                    case SvgNames.line:
                        group.Children.Add(ParseLine(childElement, reusableDefinitions, groupProperties));
                        break;
                    case SvgNames.polyline:
                        group.Children.Add(ParsePolyline(childElement, reusableDefinitions, groupProperties));
                        break;
                    case SvgNames.polygon:
                        group.Children.Add(ParsePolygon(childElement, reusableDefinitions, groupProperties));
                        break;
                    case SvgNames.text:
                        group.Children.Add(ParseText(childElement, reusableDefinitions, groupProperties));
                        break;
                }
            }

            return group;
        }

        /// <summary>
        /// Parses the &lt;defs&gt; section of an SVG, extracting reusable elements (such as shapes and gradients)
        /// and storing them in the provided Defs instance for later reference by id.
        /// </summary>
        /// <param name="element">The &lt;defs&gt; XElement containing reusable definitions.</param>
        /// <param name="reusableDefinitions">The Defs object to store parsed reusable elements.</param>
        /// <param name="groupProperties">Inherited group properties (stroke, fill, etc.), if any.</param>
        internal void ParseDefinitions(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            // Iterate over each child element within <defs>
            foreach (var childElement in element.Elements())
            {
                // Check if the element has an 'id' attribute (required for referencing)
                var idAttribute = childElement.Attribute(SvgNames.id);
                if (idAttribute != null)
                {
                    // Store the parsed element in the Defs dictionary by its id
                    switch (childElement.Name.LocalName)
                    {
                        case SvgNames.path:
                            reusableDefinitions.Elements[idAttribute.Value] = ParsePath(childElement, reusableDefinitions, groupProperties);
                            break;
                        case SvgNames.ellipse:
                            reusableDefinitions.Elements[idAttribute.Value] = ParseEllipse(childElement, reusableDefinitions, groupProperties);
                            break;
                        case SvgNames.rect:
                            reusableDefinitions.Elements[idAttribute.Value] = ParseRectangle(childElement, reusableDefinitions, groupProperties);
                            break;
                        case SvgNames.circle:
                            reusableDefinitions.Elements[idAttribute.Value] = ParseCircle(childElement, reusableDefinitions, groupProperties);
                            break;
                        case SvgNames.line:
                            reusableDefinitions.Elements[idAttribute.Value] = ParseLine(childElement, reusableDefinitions, groupProperties);
                            break;
                        case SvgNames.polyline:
                            reusableDefinitions.Elements[idAttribute.Value] = ParsePolyline(childElement, reusableDefinitions, groupProperties);
                            break;
                        case SvgNames.polygon:
                            reusableDefinitions.Elements[idAttribute.Value] = ParsePolygon(childElement, reusableDefinitions, groupProperties);
                            break;
                        case SvgNames.text:
                            reusableDefinitions.Elements[idAttribute.Value] = ParseText(childElement, reusableDefinitions, groupProperties);
                            break;
                        case SvgNames.linearGradient:
                            reusableDefinitions.Elements[idAttribute.Value] = ParseLinearGradient(childElement, reusableDefinitions, groupProperties);
                            break;
                        case SvgNames.radialGradient:
                            reusableDefinitions.Elements[idAttribute.Value] = ParseRadialGradient(childElement, reusableDefinitions, groupProperties);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Parses an SVG &lt;path&gt; element into a GeometryDrawing.
        /// </summary>
        /// <param name="element">The path XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the path.</returns>
        internal GeometryDrawing ParsePath(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            // Create a new GeometryDrawing for the path.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            PathGeometry pathGeometry = new PathGeometry();
            geometryDrawing.Geometry = pathGeometry;
            pathGeometry.FillRule = groupProperties?.FillRule == SvgNames.evenodd ? FillRule.EvenOdd : FillRule.Nonzero;

            // Populate style, stroke, fill, and transform information.
            PopulateStyleInfo(geometryDrawing, element, reusableDefinitions);
            PopulateStrokeInfo(geometryDrawing, element, reusableDefinitions);
            PopulateFillInfo(geometryDrawing, element, reusableDefinitions);
            PopulateTransformInfo(geometryDrawing, pathGeometry, element, reusableDefinitions);

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
        /// <param name="element">The ellipse XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the ellipse.</returns>
        internal GeometryDrawing ParseEllipse(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            // Create a new GeometryDrawing for the ellipse.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            EllipseGeometry ellipseGeometry = new EllipseGeometry();
            geometryDrawing.Geometry = ellipseGeometry;

            // Populate style, stroke, fill, and transform information.
            PopulateStyleInfo(geometryDrawing, element, reusableDefinitions);
            PopulateStrokeInfo(geometryDrawing, element, reusableDefinitions);
            PopulateFillInfo(geometryDrawing, element, reusableDefinitions);
            PopulateTransformInfo(geometryDrawing, ellipseGeometry, element, reusableDefinitions);

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
        /// <param name="element">The circle XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the circle.</returns>
        internal GeometryDrawing ParseCircle(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            // Create a new GeometryDrawing for the circle.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            EllipseGeometry ellipseGeometry = new EllipseGeometry();
            geometryDrawing.Geometry = ellipseGeometry;

            // Populate style, stroke, fill, and transform information.
            PopulateStyleInfo(geometryDrawing, element, reusableDefinitions);
            PopulateStrokeInfo(geometryDrawing, element, reusableDefinitions);
            PopulateFillInfo(geometryDrawing, element, reusableDefinitions);
            PopulateTransformInfo(geometryDrawing, ellipseGeometry, element, reusableDefinitions);

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
        /// <param name="element">The rect XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the rectangle.</returns>
        internal GeometryDrawing ParseRectangle(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            // Create a new GeometryDrawing for the rectangle.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            geometryDrawing.Geometry = rectangleGeometry;

            // Populate style, stroke, fill, and transform information.
            PopulateStyleInfo(geometryDrawing, element, reusableDefinitions);
            PopulateStrokeInfo(geometryDrawing, element, reusableDefinitions);
            PopulateFillInfo(geometryDrawing, element, reusableDefinitions);
            PopulateTransformInfo(geometryDrawing, rectangleGeometry, element, reusableDefinitions);

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
        /// <param name="element">The line XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the line.</returns>
        internal GeometryDrawing ParseLine(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            // Create a new GeometryDrawing for the line.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            LineGeometry lineGeometry = new LineGeometry();
            geometryDrawing.Geometry = lineGeometry;

            // Populate style, stroke, and transform information.
            PopulateStyleInfo(geometryDrawing, element, reusableDefinitions);
            PopulateStrokeInfo(geometryDrawing, element, reusableDefinitions);
            PopulateTransformInfo(geometryDrawing, lineGeometry, element, reusableDefinitions);

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
        /// <param name="element">The polyline XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the polyline.</returns>
        internal GeometryDrawing ParsePolyline(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            // Create a new GeometryDrawing for the polyline.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            PathGeometry pathGeometry = new PathGeometry();
            geometryDrawing.Geometry = pathGeometry;

            // Populate style, stroke, fill, and transform information.
            PopulateStyleInfo(geometryDrawing, element, reusableDefinitions);
            PopulateStrokeInfo(geometryDrawing, element, reusableDefinitions);
            PopulateFillInfo(geometryDrawing, element, reusableDefinitions);
            PopulateTransformInfo(geometryDrawing, pathGeometry, element, reusableDefinitions);

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
        /// <param name="element">The polygon XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the polygon.</returns>
        internal GeometryDrawing ParsePolygon(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            // Create a new GeometryDrawing for the polygon.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, groupProperties);

            PathGeometry pathGeometry = new PathGeometry();
            geometryDrawing.Geometry = pathGeometry;

            // Populate style, stroke, fill, and transform information.
            PopulateStyleInfo(geometryDrawing, element, reusableDefinitions);
            PopulateStrokeInfo(geometryDrawing, element, reusableDefinitions);
            PopulateFillInfo(geometryDrawing, element, reusableDefinitions);
            PopulateTransformInfo(geometryDrawing, pathGeometry, element, reusableDefinitions);

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
        /// <param name="element">The text XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GlyphRunDrawing for the text.</returns>
        internal GlyphRunDrawing ParseText(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            // Create a new GlyphRunDrawing for the text.
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
        /// Parses an SVG &lt;linearGradient&gt; element into a LinearGradientBrush.
        /// </summary>
        /// <param name="element">The linearGradient XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A LinearGradientBrush for the gradient.</returns>
        internal Brush ParseLinearGradient(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush();

            // Parse gradient attributes and stops.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.x1:
                        if (double.TryParse(attribute.Value, out double x1))
                        {
                            linearGradientBrush.StartPoint = new Point(x1, linearGradientBrush.StartPoint.Y);
                        }
                        break;
                    case SvgNames.y1:
                        if (double.TryParse(attribute.Value, out double y1))
                        {
                            linearGradientBrush.StartPoint = new Point(linearGradientBrush.StartPoint.X, y1);
                        }
                        break;
                    case SvgNames.x2:
                        if (double.TryParse(attribute.Value, out double x2))
                        {
                            linearGradientBrush.EndPoint = new Point(x2, linearGradientBrush.EndPoint.Y);
                        }
                        break;
                    case SvgNames.y2:
                        if (double.TryParse(attribute.Value, out double y2))
                        {
                            linearGradientBrush.EndPoint = new Point(linearGradientBrush.EndPoint.X, y2);
                        }
                        break;
                    case SvgNames.href:
                    case SvgNames.xlinkhref:
                        string key = attribute.Value.TrimStart('#');
                        reusableDefinitions.Elements.TryGetValue(key, out object? reusableElement);
                        if (reusableElement is LinearGradientBrush reusableGradient)
                        {
                            if (linearGradientBrush.StartPoint == new Point(0, 0))
                            {
                                // If no start point was set, use the reusable gradient's point.
                                linearGradientBrush.StartPoint = reusableGradient.StartPoint;
                            }
                            if (linearGradientBrush.EndPoint == new Point(1, 1))
                            {
                                // If no end point was set, use the reusable gradient's point.
                                linearGradientBrush.EndPoint = reusableGradient.EndPoint;
                            }
                            foreach (var stop in reusableGradient.GradientStops)
                            {
                                linearGradientBrush.GradientStops.Add(stop);
                            }
                            if (reusableGradient.MappingMode != BrushMappingMode.Absolute)
                            {
                                linearGradientBrush.MappingMode = reusableGradient.MappingMode;
                            }
                            if (reusableGradient.SpreadMethod != GradientSpreadMethod.Pad)
                            {
                                linearGradientBrush.SpreadMethod = reusableGradient.SpreadMethod;
                            }
                            if (reusableGradient.Transform != Transform.Identity)
                            {
                                linearGradientBrush.Transform = reusableGradient.Transform;
                            }
                        }
                        break;
                    case SvgNames.gradientTransform:
                        Transform transform = SvgHelpers.ParseTransform(attribute.Value);
                        linearGradientBrush.Transform = transform;
                        break;
                    case SvgNames.gradientUnits:
                        if (attribute.Value == SvgNames.userSpaceOnUse)
                        {
                            linearGradientBrush.MappingMode = BrushMappingMode.Absolute;
                        }
                        else
                        {
                            linearGradientBrush.MappingMode = BrushMappingMode.RelativeToBoundingBox;
                        }
                        break;
                }
            }

            // Parse gradient stops.
            foreach (var childElement in element.Elements())
            {
                switch (childElement.Name.LocalName)
                {
                    case SvgNames.stop:
                        // Parse gradient stops.
                        double offset = 0;
                        Color stopColor = Colors.Black;
                        var offsetAttribute = childElement.Attribute(SvgNames.offset);
                        if (offsetAttribute != null && double.TryParse(offsetAttribute.Value, out double parsedOffset))
                        {
                            offset = parsedOffset;
                        }
                        var stopColorAttribute = childElement.Attribute(SvgNames.stopColor);
                        if (stopColorAttribute != null)
                        {
                            stopColor = SvgHelpers.ParseColor(stopColorAttribute.Value);
                        }
                        var stopOpacityAttribute = childElement.Attribute(SvgNames.stopOpacity); // Parse this after the stop-color, to ensure accurate color
                        if (stopOpacityAttribute != null && double.TryParse(stopOpacityAttribute.Value, out double stopOpacity))
                        {
                            stopColor = SvgHelpers.ApplyOpacityToColor(stopColor, stopOpacity);
                        }
                        var styleAttribute = childElement.Attribute(SvgNames.style);
                        if (styleAttribute != null)
                        {
                            var style = SvgHelpers.ParseStyle(styleAttribute.Value, reusableDefinitions);
                            if (style.TryGetValue(SvgNames.stopColor, out object? styleStopColor) && styleStopColor is Color)
                            {
                                stopColor = (Color)styleStopColor;
                            }
                            if (style.TryGetValue(SvgNames.stopOpacity, out object? styleStopOpacity) && styleStopOpacity is double)
                            {
                                stopColor = SvgHelpers.ApplyOpacityToColor(stopColor, (double)styleStopOpacity);
                            }
                        }

                        linearGradientBrush.GradientStops.Add(new GradientStop(stopColor, offset));
                        break;

                }
            }
            return linearGradientBrush;
        }

        /// <summary>
        /// Parses an SVG &lt;radialGradient&gt; element into a RadialGradientBrush.
        /// </summary>
        /// <param name="element">The radialGradient XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A RadialGradientBrush for the gradient.</returns>
        internal Brush ParseRadialGradient(XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            RadialGradientBrush radialGradientBrush = new RadialGradientBrush();

            // Parse gradient attributes and stops.
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.cx:
                        if (double.TryParse(attribute.Value, out double cx))
                        {
                            radialGradientBrush.Center = new Point(cx, radialGradientBrush.Center.Y);
                        }
                        break;
                    case SvgNames.cy:
                        if (double.TryParse(attribute.Value, out double cy))
                        {
                            radialGradientBrush.Center = new Point(radialGradientBrush.Center.X, cy);
                        }
                        break;
                    case SvgNames.fx:
                        if (double.TryParse(attribute.Value, out double fx))
                        {
                            radialGradientBrush.GradientOrigin = new Point(fx, radialGradientBrush.GradientOrigin.Y);
                        }
                        break;
                    case SvgNames.fy:
                        if (double.TryParse(attribute.Value, out double fy))
                        {
                            radialGradientBrush.GradientOrigin = new Point(radialGradientBrush.GradientOrigin.X, fy);
                        }
                        break;
                    case SvgNames.r:
                        if (double.TryParse(attribute.Value, out double radius))
                        {
                            radialGradientBrush.RadiusX = radius;
                            radialGradientBrush.RadiusY = radius;
                        }
                        break;
                    case SvgNames.href:
                    case SvgNames.xlinkhref:
                        string key = attribute.Value.TrimStart('#');
                        reusableDefinitions.Elements.TryGetValue(key, out object? reusableElement);
                        if (reusableElement is RadialGradientBrush reusableRadialGradient)
                        {
                            if (radialGradientBrush.Center == new Point(0.5, 0.5))
                            {
                                // If no center point was set, use the reusable gradient's point.
                                radialGradientBrush.Center = reusableRadialGradient.Center;
                            }
                            if (radialGradientBrush.GradientOrigin == new Point(0.5, 0.5))
                            {
                                // If no end point was set, use the reusable gradient's point.
                                radialGradientBrush.GradientOrigin = reusableRadialGradient.GradientOrigin;
                            }
                            if (reusableRadialGradient.SpreadMethod != GradientSpreadMethod.Pad)
                            {
                                radialGradientBrush.SpreadMethod = reusableRadialGradient.SpreadMethod;
                            }
                        }
                        if (reusableElement is GradientBrush reusableGradient)
                        {
                            foreach (var stop in reusableGradient.GradientStops)
                            {
                                radialGradientBrush.GradientStops.Add(stop);
                            }
                            if (reusableGradient.Transform != Transform.Identity)
                            {
                                radialGradientBrush.Transform = reusableGradient.Transform;
                            }
                            if (reusableGradient.MappingMode != BrushMappingMode.Absolute)
                            {
                                radialGradientBrush.MappingMode = reusableGradient.MappingMode;
                            }
                        }
                        break;
                    case SvgNames.gradientTransform:
                        Transform transform = SvgHelpers.ParseTransform(attribute.Value);
                        radialGradientBrush.Transform = transform;
                        break;
                    case SvgNames.gradientUnits:
                        if (attribute.Value == SvgNames.userSpaceOnUse)
                        {
                            radialGradientBrush.MappingMode = BrushMappingMode.Absolute;
                        }
                        else
                        {
                            radialGradientBrush.MappingMode = BrushMappingMode.RelativeToBoundingBox;
                        }
                        break;
                    case SvgNames.spreadMethod:
                        switch (attribute.Value)
                        {
                            case SvgNames.pad:
                                radialGradientBrush.SpreadMethod = GradientSpreadMethod.Pad;
                                break;
                            case SvgNames.reflect:
                                radialGradientBrush.SpreadMethod = GradientSpreadMethod.Reflect;
                                break;
                            case SvgNames.repeat:
                                radialGradientBrush.SpreadMethod = GradientSpreadMethod.Repeat;
                                break;
                            default:
                                radialGradientBrush.SpreadMethod = GradientSpreadMethod.Pad; // Default to Pad if not specified
                                break;
                        }
                        break;
                }
            }

            // Parse gradient stops.
            foreach (var childElement in element.Elements())
            {
                switch (childElement.Name.LocalName)
                {
                    case SvgNames.stop:
                        // Parse gradient stops.
                        double offset = 0;
                        Color stopColor = Colors.Black;
                        var offsetAttribute = childElement.Attribute(SvgNames.offset);
                        if (offsetAttribute != null && double.TryParse(offsetAttribute.Value, out double parsedOffset))
                        {
                            offset = parsedOffset;
                        }
                        var stopColorAttribute = childElement.Attribute(SvgNames.stopColor);
                        if (stopColorAttribute != null)
                        {
                            stopColor = SvgHelpers.ParseColor(stopColorAttribute.Value);
                        }
                        var stopOpacityAttribute = childElement.Attribute(SvgNames.stopOpacity); // Parse this after the stop-color, to ensure accurate color
                        if (stopOpacityAttribute != null && double.TryParse(stopOpacityAttribute.Value, out double stopOpacity))
                        {
                            stopColor = SvgHelpers.ApplyOpacityToColor(stopColor, stopOpacity);
                        }
                        var styleAttribute = childElement.Attribute(SvgNames.style);
                        if (styleAttribute != null)
                        {
                            var style = SvgHelpers.ParseStyle(styleAttribute.Value, reusableDefinitions);
                            if (style.TryGetValue(SvgNames.stopColor, out object? styleStopColor) && styleStopColor is Color)
                            {
                                stopColor = (Color)styleStopColor;
                            }
                            if (style.TryGetValue(SvgNames.stopOpacity, out object? styleStopOpacity) && styleStopOpacity is double)
                            {
                                stopColor = SvgHelpers.ApplyOpacityToColor(stopColor, (double)styleStopOpacity);
                            }
                        }

                        radialGradientBrush.GradientStops.Add(new GradientStop(stopColor, offset));
                        break;

                }
            }
            return radialGradientBrush;
        }

        /// <summary>
        /// Sets the stroke (Pen) on a GeometryDrawing from SVG attributes.
        /// </summary>
        /// <param name="drawing">The GeometryDrawing to update.</param>
        /// <param name="element">The SVG element with stroke attributes.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        internal void PopulateStrokeInfo(GeometryDrawing drawing, XElement element, Defs reusableDefinitions)
        {
            var strokeAttribute = element.Attribute(SvgNames.stroke);
            var strokeWidthAtttribute = element.Attribute(SvgNames.strokeWidth);

            if (strokeAttribute != null)
            {
                double strokeWidth = -1;
                Brush strokeBrush = Brushes.Black;
                if (SvgHelpers.IsUseReference(strokeAttribute, out string strokeId))
                {
                    strokeBrush = reusableDefinitions.GetElement<Brush>(strokeId) ?? Brushes.Black;
                }
                else
                {
                    strokeBrush = SvgHelpers.ParseBrush(strokeAttribute.Value);
                }
                if (strokeWidthAtttribute != null)
                {
                    if (SvgHelpers.IsUseReference(strokeWidthAtttribute, out string strokeWidthId))
                    {
                        strokeWidth = reusableDefinitions.GetElement<double>(strokeWidthId);
                    }
                    else
                    {
                        double.TryParse(strokeWidthAtttribute.Value, out strokeWidth);
                    }
                }
                drawing.Pen = new Pen(strokeBrush, strokeWidth > 0 ? strokeWidth : 1);
            }
        }

        /// <summary>
        /// Sets the fill (Brush) on a GeometryDrawing from SVG attributes.
        /// </summary>
        /// <param name="drawing">The GeometryDrawing to update.</param>
        /// <param name="element">The SVG element with fill attributes.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        internal void PopulateFillInfo(GeometryDrawing drawing, XElement element, Defs reusableDefinitions)
        {
            var fillAttribute = element.Attribute(SvgNames.fill);
            if (fillAttribute != null)
            {
                if (SvgHelpers.IsUseReference(fillAttribute, out string fillId))
                {
                    drawing.Brush = reusableDefinitions.GetElement<Brush>(fillId) ?? Brushes.Transparent;
                }
                else
                {
                    // Parse the fill color directly.
                    drawing.Brush = SvgHelpers.ParseBrush(fillAttribute.Value);
                }
            }
        }

        /// <summary>
        /// Applies a transform to a Geometry from the SVG 'transform' attribute.
        /// If the drawing uses a gradient brush, the transform is also applied to the brush.
        /// </summary>
        /// <param name="drawing">The GeometryDrawing to update.</param>
        /// <param name="geometry">The Geometry to transform.</param>
        /// <param name="element">The SVG element with transform attributes.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        internal void PopulateTransformInfo(GeometryDrawing drawing, Geometry geometry, XElement element, Defs reusableDefinitions)
        {
            var transformAttribute = element.Attribute(SvgNames.transform);
            if (transformAttribute != null)
            {
                if (SvgHelpers.IsUseReference(transformAttribute, out string transformId))
                {
                    geometry.Transform = reusableDefinitions.GetElement<Transform>(transformId) ?? Transform.Identity;
                }
                else
                {
                    string transformData = transformAttribute.Value;
                    Transform transform = SvgHelpers.ParseTransform(transformData);
                    geometry.Transform = transform;
                }

                // If the brush is a gradient, apply the same transform to the brush so the gradient follows the shape.
                if (drawing.Brush is GradientBrush gradientBrush && geometry.Transform != null && geometry.Transform != Transform.Identity)
                {
                    // Combine the brush's transform with the geometry's transform.
                    var group = new TransformGroup();
                    if (gradientBrush.Transform != null && gradientBrush.Transform != Transform.Identity)
                        group.Children.Add(gradientBrush.Transform);
                    group.Children.Add(geometry.Transform);
                    gradientBrush = gradientBrush.Clone();
                    gradientBrush.Transform = group;
                    drawing.Brush = gradientBrush;
                }
            }
        }

        /// <summary>
        /// Populates style-related properties (fill, stroke, opacity, etc.) on a GeometryDrawing from the SVG style attribute.
        /// </summary>
        /// <param name="drawing">The GeometryDrawing to update.</param>
        /// <param name="element">The SVG element with style attributes.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        internal void PopulateStyleInfo(GeometryDrawing drawing, XElement element, Defs reusableDefinitions)
        {
            var styleAttribute = element.Attribute(SvgNames.style);
            if (styleAttribute != null)
            {
                var style = SvgHelpers.ParseStyle(styleAttribute.Value, reusableDefinitions);
                double baseOpacity = 1.0; // Default opacity
                if (style.TryGetValue(SvgNames.opacity, out object? o))
                {
                    baseOpacity = o is double ? (double)o : 1.0; // Default to 1 if not a double
                }
                if (style.TryGetValue(SvgNames.fill, out object? fill))
                {
                    if (SvgHelpers.IsUseReference(fill, out string fillId))
                    {
                        var fillBrush = reusableDefinitions.GetElement<Brush>(fillId) ?? Brushes.Black;
                        drawing.Brush = fillBrush.Clone(); // Clone to avoid modifying the cached brush
                    }
                    else
                    {
                        drawing.Brush = fill as Brush ?? Brushes.Black;
                    }
                }
                if (style.TryGetValue(SvgNames.fillOpacity, out object? fillOpacity))
                {
                    if (drawing.Brush is SolidColorBrush solidColorBrush)
                    {
                        if (solidColorBrush != Brushes.Transparent) // Ignore opacity if it's already transparent, some SVGs have fill: none and fill-opacity: 1....
                        {
                            if (fillOpacity is double opacity)
                            {
                                opacity *= baseOpacity; // Combine with base opacity
                                Color color = solidColorBrush.Color;
                                color.A = (byte)(opacity * 255); // Convert opacity to alpha
                                drawing.Brush = new SolidColorBrush(color);
                            }
                        }
                    }
                    else
                    {
                        drawing.Brush.Opacity = fillOpacity is double opacity ? opacity * baseOpacity : 1.0; // Combine with base opacity
                    }
                }
                else
                {
                    // No fill-opacity set
                    drawing.Brush.Opacity = baseOpacity; // Set to base opacity
                }
                if (style.TryGetValue(SvgNames.fillRule, out object? fillRule))
                {
                    if (drawing.Geometry is PathGeometry pathGeometry)
                    {
                        pathGeometry.FillRule = (FillRule)fillRule;
                    }
                }
                if (style.TryGetValue(SvgNames.stroke, out object? stroke))
                {
                    if (SvgHelpers.IsUseReference(stroke, out string strokeId))
                    {
                        var pen = new Pen(reusableDefinitions.GetElement<Brush>(strokeId) ?? Brushes.Black, 1);
                        drawing.Pen = pen.Clone(); // Clone to avoid modifying the cached pen
                    }
                    else
                    {
                        drawing.Pen = new Pen(stroke as Brush ?? Brushes.Black, 1);
                    }
                }
                if (style.TryGetValue(SvgNames.strokeWidth, out object? strokeWidth))
                {
                    double parsedStrokeWidth = 1; // Default stroke width
                    if (SvgHelpers.IsUseReference(strokeWidth, out string strokeWidthId))
                    {
                        parsedStrokeWidth = reusableDefinitions.GetElement<double>(strokeWidthId);
                    }
                    else
                    {
                        if (strokeWidth is double)
                        {
                            parsedStrokeWidth = (double)strokeWidth;
                        }
                    }
                    if (drawing.Pen != null)
                    {
                        drawing.Pen.Thickness = parsedStrokeWidth;
                    }
                }
                if (style.TryGetValue(SvgNames.strokeOpacity, out object? strokeOpacity))
                {
                    if (drawing.Pen != null)
                    {
                        if (strokeOpacity is double opacity)
                        {
                            if (drawing.Pen.Brush is SolidColorBrush solidBrush)
                            {
                                if (solidBrush != Brushes.Transparent) // Ignore opacity if it's already transparent, some SVGs have stroke: none and stroke-opacity: 1....
                                {
                                    opacity *= baseOpacity; // Combine with base opacity
                                    Color color = solidBrush.Color;
                                    color.A = (byte)(opacity * 255); // Convert opacity to alpha
                                    drawing.Pen.Brush = new SolidColorBrush(color);
                                }
                            }
                            else
                            {
                                drawing.Pen.Brush.Opacity = opacity * baseOpacity; // Combine with base opacity
                            }
                        }
                    }
                }
                else
                {
                    // No stroke-opacity set
                    if (drawing.Pen != null)
                    {
                        drawing.Pen.Brush.Opacity = baseOpacity; // Set to base opacity
                    }
                }
                if (style.TryGetValue(SvgNames.strokeLinecap, out object? strokeLineCap))
                {
                    if (drawing.Pen != null && strokeLineCap is PenLineCap lineCap)
                    {
                        drawing.Pen.StartLineCap = lineCap;
                        drawing.Pen.EndLineCap = lineCap;
                    }
                }
                if (style.TryGetValue(SvgNames.strokeLineJoin, out object? strokeLineJoin))
                {
                    if (drawing.Pen != null && strokeLineJoin is PenLineJoin lineJoin)
                    {
                        drawing.Pen.LineJoin = lineJoin;
                    }
                }
                if (style.TryGetValue(SvgNames.strokeMiterLimit, out object? strokeMiterLimit))
                {
                    if (drawing.Pen != null && strokeMiterLimit is double miterLimit)
                    {
                        drawing.Pen.MiterLimit = miterLimit;
                    }
                }
                if(style.TryGetValue(SvgNames.strokeDasharray, out object? strokeDashArray))
                {
                    if (drawing.Pen != null)
                    {
                        if (strokeDashArray is double[] dashArrayDouble)
                        {
                            drawing.Pen.DashStyle = new DashStyle(dashArrayDouble, 0);
                        }
                    }
                }
                if (style.TryGetValue(SvgNames.strokeDashOffset, out object? strokeDashOffset))
                {
                    if (drawing.Pen != null && strokeDashOffset is double dashOffset)
                    {
                        if (drawing.Pen.DashStyle != DashStyles.Solid)
                        {
                            drawing.Pen.DashStyle.Offset = dashOffset;
                        }
                    }
                }
                if (style.TryGetValue(SvgNames.transform, out object? transform))
                {
                    if (SvgHelpers.IsUseReference(transform, out string transformId))
                    {
                        drawing.Geometry.Transform = reusableDefinitions.GetElement<Transform>(transformId) ?? Transform.Identity;
                    }
                    else
                    {
                        drawing.Geometry.Transform = transform as Transform ?? Transform.Identity;
                    }
                }
            }
        }

        /// <summary>
        /// Applies inherited group properties (stroke, fill) to a GeometryDrawing.
        /// </summary>
        /// <param name="drawing">The GeometryDrawing to update.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
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
