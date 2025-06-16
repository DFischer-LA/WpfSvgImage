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
        /// <summary>SVG id element: &lt;use&gt;</summary>
        public const string id = "id";
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
        /// <summary>SVG defs element: &lt;defs&gt;</summary>
        public const string defs = "defs";
        /// <summary>SVG style element: &lt;style&gt;</summary>
        public const string style = "style";
        /// <summary>SVG reference attribute: &lt;xlink:href&gt;</summary>
        public const string xlinkhref = "xlink:href"; // SVG uses xlink namespace for href attributes
        /// <summary>SVGreference attribute: &lt;href&gt;</summary>
        public const string href = "href"; // SVG 2 uses href directly without xlink namespace
        /// <summary>SVG url attribute: &lt;url&gt;</summary>
        public const string url = "url"; // SVG uses url() for referencing elements

        // SVG brush definitions
        /// <summary>SVG linear gradient element: &lt;style&gt;</summary>
        public const string linearGradient = "linearGradient";
        /// <summary>SVG radial gradient element: &lt;radialGradient&gt;</summary>
        public const string radialGradient = "radialGradient";
        /// <summary>SVG gradient stop element: &lt;stop&gt;</summary>\
        public const string stop = "stop";
        /// <summary>SVG gradient offset attribute: &lt;offset&gt;</summary>
        public const string offset = "offset";
        /// <summary>SVG gradient stop color attribute: &lt;stop-color&gt;</summary>\
        public const string stopColor = "stop-color";
        /// <summary>SVG gradient stop opacity attribute: &lt;stop-opacity&gt;</summary>
        public const string stopOpacity = "stop-opacity";
        /// <summary>SVG gradient transform: &lt;gradientTransform&gt;</summary>
        public const string gradientTransform = "gradientTransform";
        /// <summary>SVG gradient units attribute: &lt;gradientUnits&gt;</summary>
        public const string gradientUnits = "gradientUnits";
        /// <summary>SVG gradient units value: "userSpaceOnUse".</summary>
        public const string userSpaceOnUse = "userSpaceOnUse";
        /// <summary>SVG gradient spread method"spreadMethod".</summary>
        public const string spreadMethod = "spreadMethod";
        /// <summary>Pad spread method</summary>
        public const string pad = "pad";
        /// <summary>Reflect spread method</summary>
        public const string reflect = "reflect";
        /// <summary>Repeat spread method</summary>
        public const string repeat = "repeat";

        // Common SVG attribute names
        /// <summary>Opacity attribute</summary>
        public const string opacity = "opacity";
        /// <summary>Stroke color attribute.</summary>
        public const string stroke = "stroke";
        /// <summary>Stroke width attribute.</summary>
        public const string strokeWidth = "stroke-width";
        /// <summary>Stroke line cap value: "butt".</summary>
        public const string strokeLinecap = "stroke-linecap";
        /// <summary>Stroke line cap value: "round".</summary>
        public const string butt = "butt";
        /// <summary>Stroke line cap value: "round".</summary>
        public const string round = "round";
        /// <summary>Stroke line cap value: "square".</summary>
        public const string square = "square";
        /// /// <summary>Stroke line join value: "miter".</summary>
        public const string strokeLineJoin = "stroke-linejoin";
        /// <summary>Stroke line join value: "miter".</summary>
        public const string miter = "miter";
        /// <summary>Stroke line join value: "arcs".</summary>
        public const string arcs = "arcs";
        /// <summary>Stroke line join value: "bevel".</summary>
        public const string bevel = "bevel";
        /// <summary>Stroke line join value: "miter-clip".</summary>
        public const string miterClip = "miter-clip";
        /// <summary>Stroke miter limit attribute.</summary>
        public const string strokeMiterLimit = "stroke-miterlimit";
        /// <summary>Stroke opacity attribute.</summary>
        public const string strokeOpacity = "stroke-opacity";
        /// <summary>Stroke dash offset attribute.</summary>
        public const string strokeDashOffset = "stroke-dashoffset";
        /// <summary>Stroke dash array attribute.</summary>
        public const string strokeDasharray = "stroke-dasharray";
        /// <summary>Fill color attribute.</summary>
        public const string fill = "fill";
        /// <summary>Fill opacity attribute.</summary>
        public const string fillOpacity = "fill-opacity";
        /// <summary>Fill rule attribute (e.g., "evenodd", "nonzero").</summary>
        public const string fillRule = "fill-rule";
        /// <summary>Fill rule value: "evenodd".</summary>
        public const string evenodd = "evenodd";
        /// <summary>Fill rule value: "nonzero".</summary>
        public const string nonzero = "nonzero";
        /// <summary>Special value for no paint: "none".</summary>
        public const string none = "none";
        /// <summary>rgb function</summary>
        public const string rgb = "rgb";
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
        /// <summary>Focus X for radial gradients</summary>
        public const string fx = "fx";
        /// <summary>Focus Y for radial gradients</summary>
        public const string fy = "fy";

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
