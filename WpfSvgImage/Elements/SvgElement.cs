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
    /// Base class for SVG elements that can be parsed into WPF objects
    /// </summary>
    internal abstract class SvgElement
    {
        protected XElement _xElement;
        protected Defs _reusableDefinitions;
        protected SvgGroupProperties? _groupProperties;

        public SvgElement(XElement xElement, Defs reusableDefinitions, SvgGroupProperties? groupProperties = null)
        {
            _xElement = xElement;
            _reusableDefinitions = reusableDefinitions;
            _groupProperties = groupProperties;
        }

        /// <summary>
        /// Parses an SVG element into a WPF object (GeometryDrawing, GlyphRunDrawing, Brush, etc.).
        /// </summary>
        /// <param name="element">The ellipse XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GeometryDrawing for the ellipse.</returns>
        public abstract object Parse();

        /// <summary>
        /// Populates common properties (style, stroke, fill, transform) on a GeometryDrawing.
        /// </summary>
        /// <param name="drawing"></param>
        /// <param name="geometry"></param>
        /// <param name="element"></param>
        /// <param name="reusableDefinitions"></param>
        protected void PopulateCommonInfo(GeometryDrawing drawing, Geometry geometry, XElement element, Defs reusableDefinitions, SvgGroupProperties? groupProperties)
        {
            PopulateInheritedGroupProperties(drawing, groupProperties);
            // Populate style, stroke, fill, and transform information.
            PopulateStyleInfo(drawing, _xElement, _reusableDefinitions);
            PopulateStrokeInfo(drawing, _xElement, _reusableDefinitions, groupProperties?.Opacity ?? 1.0);
            PopulateFillInfo(drawing, _xElement, _reusableDefinitions, groupProperties?.Opacity ?? 1.0);
            PopulateTransformInfo(drawing, geometry, _xElement, _reusableDefinitions);
            if(drawing.Brush == null)
            {
                drawing.Brush = Brushes.Black; // Default to black fill if no fill specified
            }
        }

        /// <summary>
        /// Applies inherited group properties (stroke, fill) to a GeometryDrawing.
        /// </summary>
        /// <param name="drawing">The GeometryDrawing to update.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        private void PopulateInheritedGroupProperties(GeometryDrawing drawing, SvgGroupProperties? groupProperties)
        {
            if (groupProperties != null)
            {
                if (!string.IsNullOrEmpty(groupProperties.Stroke))
                {
                    Brush brush = SvgHelpers.ParseBrush(groupProperties.Stroke);
                    brush = SvgHelpers.ApplyOpacityToBrush(brush, groupProperties.Opacity);
                    drawing.Pen = new Pen(brush, groupProperties.StrokeWidth ?? 1);
                }
                if (!string.IsNullOrEmpty(groupProperties.Fill))
                {
                    Brush brush = SvgHelpers.ParseBrush(groupProperties.Fill);
                    brush = SvgHelpers.ApplyOpacityToBrush(brush, groupProperties.Opacity);
                    drawing.Brush = brush;
                }
            }
        }


        /// <summary>
        /// Sets the stroke (Pen) on a GeometryDrawing from SVG attributes.
        /// </summary>
        /// <param name="drawing">The GeometryDrawing to update.</param>
        /// <param name="element">The SVG element with stroke attributes.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        private void PopulateStrokeInfo(GeometryDrawing drawing, XElement element, Defs reusableDefinitions, double opacity)
        {
            var strokeAttribute = element.Attribute(SvgNames.stroke);
            var strokeWidthAtttribute = element.Attribute(SvgNames.strokeWidth);
            var strokeCapAttribute = element.Attribute(SvgNames.strokeLinecap);

            if (strokeAttribute != null)
            {
                double strokeWidth = -1;
                Brush strokeBrush = Brushes.Black;
                if (SvgHelpers.IsUseReference(strokeAttribute, out string strokeId))
                {
                    strokeBrush = reusableDefinitions.GetElement<Brush>(strokeId) ?? Brushes.Black;
                    strokeBrush = strokeBrush.Clone(); // Clone to avoid modifying the cached brush
                }
                else
                {
                    strokeBrush = SvgHelpers.ParseBrush(strokeAttribute.Value);
                }
                strokeBrush = SvgHelpers.ApplyOpacityToBrush(strokeBrush, opacity);
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
                drawing.Pen.StartLineCap = drawing.Pen.EndLineCap = SvgHelpers.ParseLineCap(strokeCapAttribute);
            }
        }

        /// <summary>
        /// Sets the fill (Brush) on a GeometryDrawing from SVG attributes.
        /// </summary>
        /// <param name="drawing">The GeometryDrawing to update.</param>
        /// <param name="element">The SVG element with fill attributes.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        private void PopulateFillInfo(GeometryDrawing drawing, XElement element, Defs reusableDefinitions, double opacity)
        {
            var fillAttribute = element.Attribute(SvgNames.fill);
            if (fillAttribute != null)
            {
                var fillOpacityAttribute = element.Attribute(SvgNames.fillOpacity);
                if (fillOpacityAttribute != null && double.TryParse(fillOpacityAttribute.Value, out double fillOpacity))
                {
                    opacity *= fillOpacity; // Combine with base opacity
                }
                var opacityAttribute = element.Attribute(SvgNames.opacity);
                if (opacityAttribute != null && double.TryParse(opacityAttribute.Value, out double elementOpacity))
                {
                    opacity *= elementOpacity; // Combine with base opacity
                }
                Brush fillBrush = Brushes.Black;
                if (SvgHelpers.IsUseReference(fillAttribute, out string fillId))
                {
                    fillBrush = reusableDefinitions.GetElement<Brush>(fillId) ?? Brushes.Transparent;
                    fillBrush = fillBrush.Clone(); // Clone to avoid modifying the cached brush
                }
                else
                {
                    // Parse the fill color directly.
                    fillBrush = SvgHelpers.ParseBrush(fillAttribute.Value);
                }
                fillBrush = SvgHelpers.ApplyOpacityToBrush(fillBrush, opacity);
                drawing.Brush = fillBrush;
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
        private void PopulateTransformInfo(GeometryDrawing drawing, Geometry geometry, XElement element, Defs reusableDefinitions)
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
        private void PopulateStyleInfo(GeometryDrawing drawing, XElement element, Defs reusableDefinitions)
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
                if (style.TryGetValue(SvgNames.strokeDasharray, out object? strokeDashArray))
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
    }
}
