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
    /// Represents an SVG &lt;radialGradient&gt; element, which defines a radial gradient for use in brushes.
    /// </summary>
    internal class RadialGradient : SvgElement
    {
        public RadialGradient(XElement xElement, Defs reusableDefinitions, SvgGroupProperties? groupProperties = null) : base(xElement, reusableDefinitions, groupProperties)
        {
        }

        /// <summary>
        /// Parses an SVG &lt;radialGradient&gt; element into a RadialGradientBrush.
        /// </summary>
        /// <param name="element">The radialGradient XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A RadialGradientBrush for the gradient.</returns>
        public override object Parse()
        {
            RadialGradientBrush radialGradientBrush = new RadialGradientBrush();

            // Parse gradient attributes and stops.
            foreach (var attribute in _xElement.Attributes())
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
                        _reusableDefinitions.Elements.TryGetValue(key, out object? reusableElement);
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

                        radialGradientBrush.GradientStops.Add(new GradientStop(stopColor, offset));
                        break;

                }
            }
            return radialGradientBrush;
        }
    }
}
