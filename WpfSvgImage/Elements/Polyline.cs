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
    /// Represents an SVG &lt;polyline&gt; element, which defines a series of connected line segments.
    /// </summary>
    internal class Polyline : SvgElement
    {
        public Polyline(XElement xElement, Defs reusableDefinitions, SvgGroupProperties? groupProperties = null) : base(xElement, reusableDefinitions, groupProperties)
        {
        }

        /// <summary>
        /// Parses an SVG &lt;polyline&gt; element into a GeometryDrawing.
        /// </summary>
        /// <param name="element">The polyline XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the polyline.</returns>
        public override GeometryDrawing Parse()
        {
            // Create a new GeometryDrawing for the polyline.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PopulateInheritedGroupProperties(geometryDrawing, _groupProperties);

            PathGeometry pathGeometry = new PathGeometry();
            geometryDrawing.Geometry = pathGeometry;

            // Populate style, stroke, fill, and transform information.\
            PopulateCommonInfo(geometryDrawing, pathGeometry, _xElement, _reusableDefinitions);

            // Parse polyline points.
            foreach (var attribute in _xElement.Attributes())
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
    }
}
