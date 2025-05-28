using System.Windows.Media;

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
            // Otherwise, parse the color string (e.g., "#FF0000", "red").
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        }

        /// <summary>
        /// Parses an SVG 'transform' attribute value into a WPF <see cref="Matrix"/>.
        /// Supports translate, scale, rotate, skewX, skewY, and matrix commands.
        /// </summary>
        /// <param name="transform">The SVG transform string.</param>
        /// <returns>A <see cref="Matrix"/> representing the combined transformation.</returns>
        /// <exception cref="FormatException">Thrown if an invalid transform command is encountered.</exception>
        public static Matrix ParseTransform(string transform)
        {
            Matrix matrix = Matrix.Identity;
            int index = 0;
            char[] parameterSeparators = new char[] { ' ', ',' };

            // Handle 'none' as identity matrix.
            if (transform == SvgNames.none)
            {
                return Matrix.Identity;
            }

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
                        return Matrix.Identity;
                    case SvgNames.translate:
                        {
                            // translate(x [, y])
                            var parts = parameters.Split(parameterSeparators).Where(p => !string.IsNullOrEmpty(p.Trim())).ToArray();
                            if (parts.Length == 1)
                            {
                                double x = double.Parse(parts[0]);
                                matrix.Translate(x, 0);
                            }
                            else if (parts.Length == 2)
                            {
                                double x = double.Parse(parts[0]);
                                double y = double.Parse(parts[1]);
                                matrix.Translate(x, y);
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
                                matrix.Scale(scale, scale);
                            }
                            else if (parts.Length == 2)
                            {
                                double scaleX = double.Parse(parts[0]);
                                double scaleY = double.Parse(parts[1]);
                                matrix.Scale(scaleX, scaleY);
                            }
                        }
                        break;
                    case SvgNames.rotate:
                        {
                            // rotate(angle)
                            double angle = double.Parse(parameters);
                            matrix.Rotate(angle);
                            break;
                        }
                    case SvgNames.skewX:
                        {
                            // skewX(angle)
                            double angle = double.Parse(parameters);
                            matrix.Skew(angle, 0);
                            break;
                        }
                    case SvgNames.skewY:
                        {
                            // skewY(angle)
                            double angle = double.Parse(parameters);
                            matrix.Skew(0, angle);
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
                                matrix = new Matrix(m11, m12, m21, m22, dx, dy);
                            }
                        }
                        break;
                    default:
                        // Unknown or unsupported transform command.
                        throw new FormatException($"Invalid SVG transform command: {command}");
                }
            }
            return matrix;
        }
    }
}
