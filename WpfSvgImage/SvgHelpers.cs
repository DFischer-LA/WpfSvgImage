using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using WpfSvgImage.Elements;

namespace WpfSvgImage
{
    /// <summary>
    /// Provides helper methods for parsing SVG attributes into WPF types.
    /// </summary>
    internal class SvgHelpers
    {
        /// <summary>
        /// Parses an SVG color or 'none' value into a WPF <see cref="Brush"/>.
        /// </summary>
        /// <param name="value">The SVG color string or 'none'.</param>
        /// <returns>A <see cref="Brush"/> representing the color, or Transparent if 'none'.</returns>
        public static Brush ParseBrush(string value)
        {
            // SVG 'none' means no paint; use Transparent in WPF.
            if (value.StartsWith(SvgNames.none))
            {
                return Brushes.Transparent;
            }
            if(value.StartsWith(SvgNames.rgb))
            {
                string[] rgb = value.Substring(4).TrimEnd(')').Split(',');
                if(rgb.Length == 3)
                {
                    byte.TryParse(rgb[0].Trim(), out byte r);
                    byte.TryParse(rgb[1].Trim(), out byte g);
                    byte.TryParse(rgb[2].Trim(), out byte b);
                    return new SolidColorBrush(Color.FromRgb(r, g, b));
                }
                return Brushes.Black;
            }
            // Otherwise, parse the color string (e.g., "#FF0000", "red").
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        }

        /// <summary>
        /// Parses an SVG color or 'none' value into a WPF <see cref="Color"/>.
        /// </summary>
        /// <param name="value">The SVG color string or 'none'.</param>
        /// <returns>A <see cref="Color"/> representing the color, or Transparent if 'none'.</returns>
        public static Color ParseColor(string color)
        {
            // SVG 'none' means no color; use Transparent in WPF.
            if (color.StartsWith(SvgNames.none))
            {
                return Colors.Transparent;
            }
            if(color.StartsWith("rgb"))
            {
                int startIndex = color.IndexOf('(') + 1;
                int endIndex = color.IndexOf(')');
                string rgbValues = color.Substring(startIndex, endIndex - startIndex);
                string[] rgbParts = rgbValues.Split(',');
                if (rgbParts.Length == 3)
                {
                    byte r = byte.Parse(rgbParts[0].Trim());
                    byte g = byte.Parse(rgbParts[1].Trim());
                    byte b = byte.Parse(rgbParts[2].Trim());
                    return Color.FromRgb(r, g, b);
                }
            }
            // Otherwise, parse the color string (e.g., "#FF0000", "red").
            return (Color)ColorConverter.ConvertFromString(color);
        }

        /// <summary>
        /// Applies a given opacity to a WPF <see cref="Color"/>.
        /// </summary>
        /// <param name="color"> The base color to apply opacity to.</param>
        /// <param name="opacity"> The opacity value (0.0 to 1.0).</param>
        /// <returns>A new <see cref="Color"/> with the specified opacity applied.</returns>
        public static Color ApplyOpacityToColor(Color color, double opacity)
        {
            // If opacity is 1, return the color unchanged.
            if (opacity >= 1.0)
            {
                return color;
            }
            // Otherwise, apply the opacity to the color's alpha channel.
            opacity = Math.Clamp(opacity, 0.0, 1.0); // Ensure opacity is between 0 and 1.
            byte alpha = (byte)(color.A * opacity);
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        /// <summary>
        /// Applies a given opacity to a WPF <see cref="Brush"/>.
        /// </summary>
        /// <param name="brush">The base brush to apply opacity to.</param>
        /// <param name="opacity">The opacity value (0.0 to 1.0).</param>
        /// <returns></returns>
        public static Brush ApplyOpacityToBrush(Brush brush, double opacity)
        {
            if(opacity >= 1.0)
            {
                return brush; // No change needed if opacity is 1 or greater.
            }
            if (brush is SolidColorBrush solidColorBrush)
            {
                // If the brush is a SolidColorBrush, apply opacity to its color.
                Color color = solidColorBrush.Color;
                return new SolidColorBrush(ApplyOpacityToColor(color, opacity));
            }
            else if (brush is LinearGradientBrush linearGradientBrush)
            {
                // Apply opacity to each gradient stop's color.
                foreach (var stop in linearGradientBrush.GradientStops)
                {
                    stop.Color = ApplyOpacityToColor(stop.Color, opacity);
                }
                return linearGradientBrush;
            }
            else if (brush is RadialGradientBrush radialGradientBrush)
            {
                // Apply opacity to each gradient stop's color.
                foreach (var stop in radialGradientBrush.GradientStops)
                {
                    stop.Color = ApplyOpacityToColor(stop.Color, opacity);
                }
                return radialGradientBrush;
            }
            return brush; // Return unchanged for other brush types.
        }

        /// <summary>
        /// Parses an SVG 'transform' attribute value into a WPF <see cref="Matrix"/>.
        /// Supports translate, scale, rotate, skewX, skewY, and matrix commands.
        /// </summary>
        /// <param name="transform">The SVG transform string.</param>
        /// <returns>A <see cref="Matrix"/> representing the combined transformation.</returns>
        /// <exception cref="FormatException">Thrown if an invalid transform command is encountered.</exception>
        public static Transform ParseTransform(string transform)
        {
            //Matrix matrix = Matrix.Identity;
            int index = 0;
            char[] parameterSeparators = new char[] { ' ', ',' };

            // Handle 'none' as identity matrix.
            if (transform == SvgNames.none)
            {
                return Transform.Identity;
            }

            List<Transform> transforms = new List<Transform>();
            // Parse each transform command in the string.
            while (index < transform.Length)
            {
                // Skip whitespace and commas.
                while (index < transform.Length && (char.IsWhiteSpace(transform[index]) || transform[index] == ','))
                {
                    index++;
                }
                int nextOpenParen = transform.IndexOf('(', index);
                if (nextOpenParen < 0)
                {
                    break; // No more commands
                }
                string command = transform.Substring(index, nextOpenParen - index).Trim();
                int nextCloseParen = transform.IndexOf(')', nextOpenParen);
                string parameters = transform.Substring(nextOpenParen + 1, nextCloseParen - nextOpenParen - 1).Trim();
                index = nextCloseParen + 1;

                switch (command)
                {
                    case SvgNames.none:
                        // No transformation, return identity matrix.
                        return Transform.Identity;
                    case SvgNames.translate:
                        {
                            // translate(x [, y])
                            var parts = parameters.Split(parameterSeparators).Where(p => !string.IsNullOrEmpty(p.Trim())).ToArray();
                            if (parts.Length == 1)
                            {
                                double x = double.Parse(parts[0]);
                                transforms.Add(new TranslateTransform(x, 0)); // Default y to 0
                            }
                            else if (parts.Length == 2)
                            {
                                double x = double.Parse(parts[0]);
                                double y = double.Parse(parts[1]);
                                transforms.Add(new TranslateTransform(x, y));
                            }
                        }
                        break;
                    case SvgNames.scale:
                        {
                            // scale(sx [, sy])
                            var parts = parameters.Split(parameterSeparators).Where(p => !string.IsNullOrEmpty(p.Trim())).ToArray();
                            if (parts.Length == 1)
                            {
                                double scale = double.Parse(parts[0]);
                                transforms.Add(new ScaleTransform(scale, scale)); // Default sy to sx
                            }
                            else if (parts.Length == 2)
                            {
                                double scaleX = double.Parse(parts[0]);
                                double scaleY = double.Parse(parts[1]);
                                transforms.Add(new ScaleTransform(scaleX, scaleY));
                            }
                        }
                        break;
                    case SvgNames.rotate:
                        {
                            // rotate(angle)
                            double angle = double.Parse(parameters);
                            transforms.Add(new RotateTransform(angle));
                            break;
                        }
                    case SvgNames.skewX:
                        {
                            // skewX(angle)
                            double angle = double.Parse(parameters);
                            transforms.Add(new SkewTransform(angle, 0)); // Default skewY to 0
                            break;
                        }
                    case SvgNames.skewY:
                        {
                            // skewY(angle)
                            double angle = double.Parse(parameters);
                            transforms.Add(new SkewTransform(0, angle)); // Default skewX to 0
                            break;
                        }
                    case SvgNames.matrix:
                        {
                            // matrix(a, b, c, d, e, f)
                            var parts = parameters.Split(parameterSeparators).Where(p => !string.IsNullOrEmpty(p.Trim())).ToArray();
                            if (parts.Length == 6)
                            {
                                double m11 = double.Parse(parts[0]);
                                double m12 = double.Parse(parts[1]);
                                double m21 = double.Parse(parts[2]);
                                double m22 = double.Parse(parts[3]);
                                double dx = double.Parse(parts[4]);
                                double dy = double.Parse(parts[5]);
                                Matrix matrix = new Matrix(m11, m12, m21, m22, dx, dy);
                                transforms.Add(new MatrixTransform(matrix));
                            }
                        }
                        break;
                    default:
                        // Unknown or unsupported transform command.
                        throw new FormatException($"Invalid SVG transform command: {command}");
                }
            }

            if(transforms.Count > 1)
            {
                TransformGroup transformGroup = new TransformGroup();
                foreach (var t in transforms)
                {
                    transformGroup.Children.Add(t);
                }
                return transformGroup;
            }
            if (transforms.Count == 1)
            {
                return transforms[0];
            }
            return Transform.Identity;
        }

        public static Dictionary<string, object> ParseStyle(string styleAttribute, Defs reusableDefinitions)
        {
            var style = new Dictionary<string, object>();

            int index = 0;
            // Parse each style command in the string.
            while (index < styleAttribute.Length)
            {
                // Skip whitespace and commas.
                while (index < styleAttribute.Length && (char.IsWhiteSpace(styleAttribute[index]) || styleAttribute[index] == ','))
                {
                    index++;
                }
                int nextColon = styleAttribute.IndexOf(':', index);
                if (nextColon < 0)
                {
                    break; // No more commands
                }
                string styleElement = styleAttribute.Substring(index, nextColon - index).Trim();
                int nextSemiColon = styleAttribute.IndexOf(';', nextColon);
                string parameters = styleAttribute.Substring(nextColon + 1, nextSemiColon - nextColon - 1).Trim();
                index = nextSemiColon + 1;
                if (parameters.StartsWith(SvgNames.url))
                {
                    style[styleElement] = parameters; // Keep the URL as is, it may be a reference to a gradient or pattern.
                }
                else
                {
                    string refId;
                    switch (styleElement)
                    {
                        case SvgNames.stopColor:
                            style[SvgNames.stopColor] = ParseColor(parameters);
                            break;
                        case SvgNames.stopOpacity:
                            if (double.TryParse(parameters, out double opacity))
                            {
                                style[SvgNames.stopOpacity] = opacity;
                            }
                            break;
                        case SvgNames.opacity:
                            if (double.TryParse(parameters, out double opacityValue))
                            {
                                style[SvgNames.opacity] = opacityValue;
                            }
                            break;
                        case SvgNames.fill:
                            if (SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.fill] = reusableDefinitions.GetElement<Brush>(refId) as Brush ?? Brushes.Black;
                            }
                            else
                            {
                                style[SvgNames.fill] = ParseBrush(parameters);
                            }
                            break;
                        case SvgNames.fillOpacity:
                            if (SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.fillOpacity] = (double)reusableDefinitions.GetElement<double>(refId);
                            }
                            else
                            {
                                if (double.TryParse(parameters, out double fillOpacity))
                                {
                                    style[SvgNames.fillOpacity] = fillOpacity;
                                }
                            }
                            break;
                        case SvgNames.fillRule:
                            if (SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.fillRule] = reusableDefinitions.GetElement<FillRule>(refId);
                            }
                            else
                            {
                                if (parameters.Equals(SvgNames.nonzero, StringComparison.OrdinalIgnoreCase))
                                {
                                    style[SvgNames.fillRule] = FillRule.Nonzero;
                                }
                                else if (parameters.Equals(SvgNames.evenodd, StringComparison.OrdinalIgnoreCase))
                                {
                                    style[SvgNames.fillRule] = FillRule.EvenOdd;
                                }
                                else
                                {
                                    style[SvgNames.fillRule] = FillRule.Nonzero; // Default case
                                }
                            }
                            break;
                        case SvgNames.stroke:
                            if (SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.stroke] = reusableDefinitions.GetElement<Brush>(refId) as Brush ?? Brushes.Black;
                            }
                            else
                            {
                                style[SvgNames.stroke] = ParseBrush(parameters);
                            }
                            break;
                        case SvgNames.strokeWidth:
                            if(SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.strokeWidth] = (double)reusableDefinitions.GetElement<double>(refId);
                            }
                            else
                            {
                                if(double.TryParse(parameters, out double sw))
                                    style[SvgNames.strokeWidth] = sw;
                            }
                            break;
                        case SvgNames.strokeLinecap:
                            if(SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.strokeLinecap] = reusableDefinitions.GetElement<PenLineCap>(refId);
                            }
                            else
                            {
                                if (parameters.Equals(SvgNames.butt, StringComparison.OrdinalIgnoreCase))
                                {
                                    style[SvgNames.strokeLinecap] = PenLineCap.Flat;
                                }
                                else if (parameters.Equals(SvgNames.round, StringComparison.OrdinalIgnoreCase))
                                {
                                    style[SvgNames.strokeLinecap] = PenLineCap.Round;
                                }
                                else if (parameters.Equals(SvgNames.square, StringComparison.OrdinalIgnoreCase))
                                {
                                    style[SvgNames.strokeLinecap] = PenLineCap.Square;
                                }
                                else
                                {
                                    style[SvgNames.strokeLinecap] = PenLineCap.Flat; // Default case
                                }
                            }
                            break;
                        case SvgNames.strokeLineJoin:
                            if(SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.strokeLineJoin] = reusableDefinitions.GetElement<PenLineJoin>(refId);
                            }
                            else
                            {
                                if (parameters.Equals(SvgNames.miter, StringComparison.OrdinalIgnoreCase))
                                {
                                    style[SvgNames.strokeLineJoin] = PenLineJoin.Miter;
                                }
                                else if (parameters.Equals(SvgNames.round, StringComparison.OrdinalIgnoreCase))
                                {
                                    style[SvgNames.strokeLineJoin] = PenLineJoin.Round;
                                }
                                else if (parameters.Equals(SvgNames.bevel, StringComparison.OrdinalIgnoreCase))
                                {
                                    style[SvgNames.strokeLineJoin] = PenLineJoin.Bevel;
                                }
                                else if (parameters.Equals(SvgNames.miterClip, StringComparison.OrdinalIgnoreCase))
                                {
                                    style[SvgNames.strokeLineJoin] = PenLineJoin.Miter; // Miter clip not a value in WPF
                                }
                                else
                                {
                                    style[SvgNames.strokeLineJoin] = PenLineJoin.Miter; // Default case
                                }
                            }
                            break;
                        case SvgNames.strokeMiterLimit:
                            if (SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.strokeMiterLimit] = Math.Max(1,(double)reusableDefinitions.GetElement<double>(refId));
                            }
                            else
                            {
                                if (double.TryParse(parameters, out double miterLimit))
                                    style[SvgNames.strokeMiterLimit] = Math.Max(1, miterLimit);
                            }
                            break;
                        case SvgNames.strokeDasharray:
                            if (SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.strokeDasharray] = reusableDefinitions.GetElement<double[]>(refId) ?? Array.Empty<double>();
                            }
                            else
                            {
                                // Parse dash array values, e.g., "5, 10, 15"
                                var dashArray = parameters.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(p => double.TryParse(p, out double value) ? value : 0.0)
                                                          .ToArray();
                                style[SvgNames.strokeDasharray] = dashArray;
                            }
                            break;
                        case SvgNames.strokeDashOffset:
                            if (SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.strokeDashOffset] = (double)reusableDefinitions.GetElement<double>(refId);
                            }
                            else
                            {
                                if (double.TryParse(parameters, out double dashOffset))
                                    style[SvgNames.strokeDashOffset] = dashOffset;
                            }
                            break;
                        case SvgNames.strokeOpacity:
                            if (SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.strokeOpacity] = (double)reusableDefinitions.GetElement<double>(refId);
                            }
                            else
                            {
                                if (double.TryParse(parameters, out double strokeOpacity))
                                    style[SvgNames.strokeOpacity] = strokeOpacity;
                            }
                            break;
                        case SvgNames.transform:
                            if(SvgHelpers.IsUseReference(parameters, out refId))
                            {
                                style[SvgNames.transform] = reusableDefinitions.GetElement<Transform>(refId) as Transform ?? Transform.Identity;
                            }
                            else
                            {
                                style[SvgNames.transform] = ParseTransform(parameters);
                            }
                            break;
                    }
                }
            }

            return style;
        }

        public static bool IsUseReference(XAttribute attribute, out string refId)
        {
            return IsUseReference(attribute.Value, out refId);
        }

        public static bool IsUseReference(object? attribute, out string refId)
        {
            if(attribute == null || !(attribute is string attributeValue))
            {
                refId = "";
                return false;
            }
            return IsUseReference(attributeValue, out refId);
        }

        public static bool IsUseReference(string attribute, out string refId)
        {
            refId = "";

            if (attribute.Trim().StartsWith(SvgNames.url, StringComparison.OrdinalIgnoreCase))
            {
                // Handle references to reusable definitions.
                int startIndex = attribute.IndexOf('(');
                int endIndex = attribute.IndexOf(')', startIndex);
                if (startIndex > 0 && endIndex > startIndex)
                {
                    // Extract the id from url() or href() syntax.
                    string id = attribute.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
                    if (id.StartsWith("#"))
                    {
                        id = id.Substring(1); // Remove leading '#' if present.
                    }
                    refId = id;
                    return true;
                }
            }

            return false;
        }
    }
}
