using System.Runtime.CompilerServices;

namespace WpfSvgImage
{
    /// <summary>
    /// Contains constant string values for common SVG element and attribute names.
    /// Used to avoid magic strings and typos in SVG parsing and rendering.
    /// </summary>
    internal class SvgNames
    {
        // SVG element names
        /// <summary>SVG root element: &lt;svg&gt;</summary>
        public const string svg = "svg";
        /// <summary>SVG group element: &lt;g&gt;</summary>
        public const string g = "g";
        /// <summary>SVG path element: &lt;path&gt;</summary>
        public const string path = "path";
        /// <summary>SVG rectangle element: &lt;rect&gt;</summary>
        public const string rect = "rect";
        /// <summary>SVG circle element: &lt;circle&gt;</summary>
        public const string circle = "circle";
        /// <summary>SVG ellipse element: &lt;ellipse&gt;</summary>
        public const string ellipse = "ellipse";
        /// <summary>SVG line element: &lt;line&gt;</summary>
        public const string line = "line";
        /// <summary>SVG polygon element: &lt;polygon&gt;</summary>
        public const string polygon = "polygon";
        /// <summary>SVG polyline element: &lt;polyline&gt;</summary>
        public const string polyline = "polyline";
        /// <summary>SVG text element: &lt;text&gt;</summary>
        public const string text = "text";

        // Common SVG attribute names
        /// <summary>Stroke color attribute.</summary>
        public const string stroke = "stroke";
        /// <summary>Stroke width attribute.</summary>
        public const string strokeWidth = "stroke-width";
        /// <summary>Fill color attribute.</summary>
        public const string fill = "fill";
        /// <summary>Fill rule attribute (e.g., "evenodd", "nonzero").</summary>
        public const string fillRule = "fill-rule";
        /// <summary>Fill rule value: "evenodd".</summary>
        public const string evenodd = "evenodd";
        /// <summary>Fill rule value: "nonzero".</summary>
        public const string nonzero = "nonzero";
        /// <summary>Special value for no paint: "none".</summary>
        public const string none = "none";
        /// <summary>Path data attribute (for &lt;path&gt; elements).</summary>
        public const string d = "d";

        // Position and size attributes
        /// <summary>X position attribute.</summary>
        public const string x = "x";
        /// <summary>Y position attribute.</summary>
        public const string y = "y";
        /// <summary>Width attribute.</summary>
        public const string width = "width";
        /// <summary>Height attribute.</summary>
        public const string height = "height";
        /// <summary>Center X attribute (for circles/ellipses).</summary>
        public const string cx = "cx";
        /// <summary>Center Y attribute (for circles/ellipses).</summary>
        public const string cy = "cy";
        /// <summary>Radius X attribute (for ellipses/rounded rects).</summary>
        public const string rx = "rx";
        /// <summary>Radius Y attribute (for ellipses/rounded rects).</summary>
        public const string ry = "ry";
        /// <summary>Radius attribute (for circles).</summary>
        public const string r = "r";
        /// <summary>X1 attribute (for lines).</summary>
        public const string x1 = "x1";
        /// <summary>Y1 attribute (for lines).</summary>
        public const string y1 = "y1";
        /// <summary>X2 attribute (for lines).</summary>
        public const string x2 = "x2";
        /// <summary>Y2 attribute (for lines).</summary>
        public const string y2 = "y2";
        /// <summary>Points attribute (for polygons and polylines).</summary>
        public const string points = "points";

        // Text attributes
        /// <summary>Font family attribute (for text).</summary>
        public const string fontFamily = "font-family";
        /// <summary>Font size attribute (for text).</summary>
        public const string fontSize = "font-size";
        /// <summary>Font weight attribute (for text).</summary>
        public const string fontWeight = "font-weight";
        /// <summary>Font style attribute (for text).</summary>
        public const string fontStyle = "font-style";
        /// <summary>Text anchor attribute (for text alignment).</summary>
        public const string textAnchor = "text-anchor";

        // Transform attributes and types
        /// <summary>Transform attribute (for geometric transforms).</summary>
        public const string transform = "transform";
        /// <summary>Translate transform type.</summary>
        public const string translate = "translate";
        /// <summary>Scale transform type.</summary>
        public const string scale = "scale";
        /// <summary>Rotate transform type.</summary>
        public const string rotate = "rotate";
        /// <summary>Matrix transform type.</summary>
        public const string matrix = "matrix";
        /// <summary>SkewX transform type.</summary>
        public const string skewX = "skewX";
        /// <summary>SkewY transform type.</summary>
        public const string skewY = "skewY";
    }
}
