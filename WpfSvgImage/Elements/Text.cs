using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace WpfSvgImage.Elements
{
    /// <summary>
    /// Represents an SVG &lt;text&gt; element, which defines a text string to be rendered.
    /// </summary>
    internal class Text : SvgElement
    {
        public Text(XElement xElement, Defs reusableDefinitions, SvgGroupProperties? groupProperties = null) : base(xElement, reusableDefinitions, groupProperties)
        {
        }

        /// <summary>
        /// Parses an SVG &lt;text&gt; element into a GlyphRunDrawing.
        /// </summary>
        /// <param name="element">The text XElement.</param>
        /// <param name="reusableDefinitions">Reusable definitions for referenced elements.</param>
        /// <param name="groupProperties">Inherited group properties.</param>
        /// <returns>A GlyphRunDrawing for the text.</returns>
        public override GlyphRunDrawing Parse()
        {
            // Create a new GlyphRunDrawing for the text.
            GlyphRunDrawing glyphRunDrawing = new GlyphRunDrawing();

            // Get system DPI for text rendering.
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            int dpiX = 120;
            if (dpiXProperty != null)
            {
                object? oDpiX = dpiXProperty.GetValue(null, null);
                if (oDpiX != null)
                {
                    dpiX = (int)oDpiX;
                }
            }
            float pixelsPerDip = dpiX / 96f;

            // Default text properties.
            FontFamily fontFamily = new FontFamily("Arial");
            FontWeight fontWeight = FontWeights.Normal;
            FontStyle fontStyle = FontStyles.Normal;
            double fontSize = 12;
            double x = 0;
            double y = 0;

            // Parse text attributes.
            foreach (var attribute in _xElement.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case SvgNames.fill:
                        glyphRunDrawing.ForegroundBrush = SvgHelpers.ParseBrush(attribute.Value);
                        break;
                    case SvgNames.fontFamily:
                        FontFamilyConverter fontFamilyConverter = new FontFamilyConverter();
                        if (fontFamilyConverter.ConvertFrom(attribute.Value) is FontFamily family)
                        {
                            fontFamily = family;
                        }
                        break;
                    case SvgNames.fontSize:
                        FontSizeConverter fontSizeConverter = new FontSizeConverter();
                        if (fontSizeConverter.ConvertFrom(attribute.Value) is double size)
                        {
                            fontSize = size;
                        }
                        break;
                    case SvgNames.fontWeight:
                        FontWeightConverter fontWeightConverter = new FontWeightConverter();
                        if (fontWeightConverter.ConvertFrom(attribute.Value) is FontWeight weight)
                        {
                            fontWeight = weight;
                        }
                        break;
                    case SvgNames.fontStyle:
                        FontStyleConverter fontStyleConverter = new FontStyleConverter();
                        if (fontStyleConverter.ConvertFrom(attribute.Value) is FontStyle style)
                        {
                            fontStyle = style;
                        }
                        break;
                    case SvgNames.x:
                        if (double.TryParse(attribute.Value, out double xValue))
                        {
                            x = xValue;
                        }
                        break;
                    case SvgNames.y:
                        if (double.TryParse(attribute.Value, out double yValue))
                        {
                            y = yValue;
                        }
                        break;
                }
            }

            // Build the GlyphRun for the text.
            Typeface typeface = new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal);
            GlyphTypeface typeFace;
            if (typeface.TryGetGlyphTypeface(out typeFace))
            {
                double textWidth = 0;
                var glyphIndices = new ushort[_xElement.Value.Length];
                var glyphAdvanceWidths = new double[_xElement.Value.Length];
                var glyphOffsets = new Point[_xElement.Value.Length];
                var characters = _xElement.Value.ToCharArray();
                for (int i = 0; i < _xElement.Value.Length; i++)
                {
                    char c = _xElement.Value[i];
                    ushort glyphIndex = typeFace.CharacterToGlyphMap[c];
                    glyphIndices[i] = glyphIndex;

                    double width = typeFace.AdvanceWidths[glyphIndex] * fontSize;
                    glyphAdvanceWidths[i] = width;

                    glyphOffsets[i] = new Point(0, 0); // No offset for now.

                    textWidth += width;
                }

                GlyphRun glyphRun = new GlyphRun(
                        typeFace,
                        0,
                        false, // isSideways
                        fontSize, // Font rendering size
                        pixelsPerDip, // pixels per dip
                        glyphIndices, // Glyph indices
                        new Point(x, y), // Baseline origin
                        glyphAdvanceWidths, // Advance widths
                        glyphOffsets, // Glyph offsets
                        characters, // Characters
                        null, // Device font name
                        null, // Cluster map
                        null, // Caret stops
                        null); // Language

                glyphRunDrawing.GlyphRun = glyphRun;
            }

            return glyphRunDrawing;
        }
    }
}
