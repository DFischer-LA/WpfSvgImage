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
    /// Represents an SVG &lt;circle&gt; element, which defines a circle shape with a specified center and radius.
    /// </summary>
    internal class Circle : SvgElement
    {
        public Circle(XElement xElement, Defs reusableDefinitions, SvgGroupProperties? groupProperties = null) : base(xElement, reusableDefinitions, groupProperties)
        {
        }

        /// <summary>
        /// Parses an SVG &lt;circle&gt; element into a GeometryDrawing.
        /// </summary>
        /// <param name="element">The circle XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the circle.</returns>
        public override GeometryDrawing Parse()
        {
            // Create a new GeometryDrawing for the circle.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, _groupProperties);

            EllipseGeometry ellipseGeometry = new EllipseGeometry();
            geometryDrawing.Geometry = ellipseGeometry;

            // Populate style, stroke, fill, and transform information.
            PopulateCommonInfo(geometryDrawing, ellipseGeometry, _xElement, _reusableDefinitions);

            // Parse circle attributes.
            foreach (var attribute in _xElement.Attributes())
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
    }
}
