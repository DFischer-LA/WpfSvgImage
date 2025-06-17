using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace WpfSvgImage.Elements
{
    /// <summary>
    /// Represents an SVG &lt;rect&gt; element, which defines a rectangle shape with specified position, size, and corner radii.
    /// </summary>
    internal class Rectangle : SvgElement
    {
        public Rectangle(XElement xElement, Defs reusableDefinitions, SvgGroupProperties? groupProperties = null) : base(xElement, reusableDefinitions, groupProperties)
        {
        }

        /// <summary>
        /// Parses an SVG &lt;rect&gt; element into a GeometryDrawing.
        /// </summary>
        /// <param name="element">The rect XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the rectangle.</returns>
        public override GeometryDrawing Parse()
        {
            // Create a new GeometryDrawing for the rectangle.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, _groupProperties);

            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            geometryDrawing.Geometry = rectangleGeometry;

            // Populate style, stroke, fill, and transform information.
            PopulateCommonInfo(geometryDrawing, rectangleGeometry, _xElement, _reusableDefinitions);

            // Parse rectangle attributes.
            foreach (var attribute in _xElement.Attributes())
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
    }
}
