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
    /// Represents an SVG &lt;line&gt; element, which defines a straight line segment between two points.
    /// </summary>
    internal class Line : SvgElement
    {
        public Line(XElement xElement, Defs reusableDefinitions, SvgGroupProperties? groupProperties = null) : base(xElement, reusableDefinitions, groupProperties)
        {
        }

        /// <summary>
        /// Parses an SVG &lt;line&gt; element into a GeometryDrawing.
        /// </summary>
        /// <param name="element">The line XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the line.</returns>
        public override GeometryDrawing Parse()
        {
            // Create a new GeometryDrawing for the line.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, _groupProperties);

            LineGeometry lineGeometry = new LineGeometry();
            geometryDrawing.Geometry = lineGeometry;

            // Populate style, stroke, and transform information.
            PopulateCommonInfo(geometryDrawing, lineGeometry, _xElement, _reusableDefinitions);

            // Parse line attributes.
            foreach (var attribute in _xElement.Attributes())
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
    }
}
