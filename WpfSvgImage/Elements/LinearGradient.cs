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
    /// Represents an SVG linearGradient element, which defines a linear gradient for use in brushes.
    /// </summary>
    internal class LinearGradient : SvgElement
    {
        public LinearGradient(XElement xElement, Defs reusableDefinitions, SvgGroupProperties? groupProperties = null) : base(xElement, reusableDefinitions, groupProperties)
        {
        }

        /// <summary>
        /// Parses an SVG &lt;linearGradient&gt; element into a LinearGradientBrush.
        /// </summary>
        /// <param name="element">The linearGradient XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A LinearGradientBrush for the gradient.</returns>
        public override object Parse()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush();

            // Parse gradient attributes and stops.
            foreach (var attribute in _xElement.Attributes())
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
                        _reusableDefinitions.Elements.TryGetValue(key, out object? reusableElement);
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
            foreach (var childElement in _xElement.Elements())
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
                            var style = SvgHelpers.ParseStyle(styleAttribute.Value, _reusableDefinitions);
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
    }
}
