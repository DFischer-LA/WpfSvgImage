using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace WpfSvgImage.Elements
{
    /// <summary>
    /// Represents an SVG &lt;path&gt; element, which defines a complex shape using a series of commands and coordinates.
    /// </summary>
    internal class Path : SvgElement
    {
        public Path(XElement xElement, Defs reusableDefinitions, SvgGroupProperties? groupProperties = null) : base(xElement, reusableDefinitions, groupProperties) {}

        /// <summary>
        /// Parses an SVG &lt;path&gt; element into a GeometryDrawing.
        /// </summary>
        /// <param name="element">The ellipse XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the ellipse.</returns>
        public override GeometryDrawing Parse()
        {
            // Create a new GeometryDrawing for the path.
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            PathGeometry pathGeometry = new PathGeometry();
            geometryDrawing.Geometry = pathGeometry;
            pathGeometry.FillRule = _groupProperties?.FillRule == SvgNames.evenodd ? FillRule.EvenOdd : FillRule.Nonzero;

            // Populate style, stroke, fill, and transform information.
            PopulateCommonInfo(geometryDrawing, pathGeometry, _xElement, _reusableDefinitions, _groupProperties);
          
            // Parse path data and fill rule.
            foreach (var attribute in _xElement.Attributes())
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
    }
}
