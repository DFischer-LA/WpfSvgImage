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
                        group.Children.Add(new Path(childElement, reusableDefinitions, groupProperties).Parse());
                        break;
                    case SvgNames.ellipse:
                        group.Children.Add(new Ellipse(childElement, reusableDefinitions, groupProperties).Parse());
                        break;
                    case SvgNames.rect:
                        group.Children.Add(new Rectangle(childElement, reusableDefinitions, groupProperties).Parse());
                        break;
                    case SvgNames.circle:
                        group.Children.Add(new Circle(childElement, reusableDefinitions, groupProperties).Parse());
                        break;
                    case SvgNames.line:
                        group.Children.Add(new Line(childElement, reusableDefinitions, groupProperties).Parse());
                        break;
                    case SvgNames.polyline:
                        group.Children.Add(new Polyline(childElement, reusableDefinitions, groupProperties).Parse());
                        break;
                    case SvgNames.polygon:
                        group.Children.Add(new Polygon(childElement, reusableDefinitions, groupProperties).Parse());
                        break;
                    case SvgNames.text:
                        group.Children.Add(new Text(childElement, reusableDefinitions, groupProperties).Parse());
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
                            reusableDefinitions.Elements[idAttribute.Value] = new Path(childElement, reusableDefinitions, groupProperties).Parse();
                            break;
                        case SvgNames.ellipse:
                            reusableDefinitions.Elements[idAttribute.Value] = new Ellipse(childElement, reusableDefinitions, groupProperties).Parse();
                            break;
                        case SvgNames.rect:
                            reusableDefinitions.Elements[idAttribute.Value] = new Rectangle(childElement, reusableDefinitions, groupProperties).Parse();
                            break;
                        case SvgNames.circle:
                            reusableDefinitions.Elements[idAttribute.Value] = new Circle(childElement, reusableDefinitions, groupProperties).Parse();
                            break;
                        case SvgNames.line:
                            reusableDefinitions.Elements[idAttribute.Value] = new Line(childElement, reusableDefinitions, groupProperties).Parse();
                            break;
                        case SvgNames.polyline:
                            reusableDefinitions.Elements[idAttribute.Value] = new Polyline(childElement, reusableDefinitions, groupProperties).Parse();
                            break;
                        case SvgNames.polygon:
                            reusableDefinitions.Elements[idAttribute.Value] = new Polygon(childElement, reusableDefinitions, groupProperties).Parse();
                            break;
                        case SvgNames.text:
                            reusableDefinitions.Elements[idAttribute.Value] = new Text(childElement, reusableDefinitions, groupProperties).Parse();
                            break;
                        case SvgNames.linearGradient:
                            reusableDefinitions.Elements[idAttribute.Value] = new LinearGradient(childElement, reusableDefinitions, groupProperties).Parse();
                            break;
                        case SvgNames.radialGradient:
                            reusableDefinitions.Elements[idAttribute.Value] = new RadialGradient(childElement, reusableDefinitions, groupProperties).Parse();
                            break;
                    }
                }
            }
        }
    }
}
