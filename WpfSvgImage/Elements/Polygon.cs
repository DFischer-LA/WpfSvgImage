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
    /// Represents an SVG &lt;polygon&gt; element, which defines a closed shape consisting of a series of connected line segments.
    /// </summary>
    internal class Polygon : SvgElement
    {
        public Polygon(XElement xElement, Defs reusableDefinitions, SvgGroupProperties? groupProperties = null) : base(xElement, reusableDefinitions, groupProperties)
        {
        }

        /// <summary>
        /// Parses an SVG &lt;polygon&gt; element into a GeometryDrawing.
        /// </summary>
        /// <param name="element">The polygon XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the polygon.</returns>
        public override GeometryDrawing Parse()
        {
            // Create a new GeometryDrawing for the polygon.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PathGeometry pathGeometry = new PathGeometry();
            geometryDrawing.Geometry = pathGeometry;

            // Populate style, stroke, fill, and transform information.
            PopulateCommonInfo(geometryDrawing, pathGeometry, _xElement, _reusableDefinitions, _groupProperties);

            // Parse polygon points and ensure closure.
            foreach (var attribute in _xElement.Attributes())
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
    }
}
