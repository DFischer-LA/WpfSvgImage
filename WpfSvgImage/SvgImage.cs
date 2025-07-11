
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace WpfSvgImage
{

    /// <summary>
    /// A WPF Image control that displays SVG images.
    /// Provides static factory methods for creating instances from various SVG sources,
    /// and a dependency property for binding to an SVG resource URI.
    /// </summary>
    public class SvgImage : Image
    {
        private DrawingImage? _editInProgressImage = null;
        private XDocument? _originalSVG = null;

        /// <summary>
        /// Begins an edit session for the current SVG image.
        /// Clones the current <see cref="DrawingImage"/> and stores it internally,
        /// allowing modifications to be made without affecting the original image until <see cref="EndEdit"/> is called.
        /// Throws an exception if an edit session is already in progress.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an edit is already in progress. Call <see cref="EndEdit"/> before starting a new edit.
        /// </exception>
        public void BeginEdit()
        {
            if (_editInProgressImage != null)
            {
                throw new InvalidOperationException("An edit is already in progress. Call EndEdit to finish the current edit before starting a new one.");
            }
            if (Source is DrawingImage drawingImage)
            {
                _editInProgressImage = drawingImage.Clone();
            }
        }

        /// <summary>
        /// Ends the current edit session and applies the changes made to the SVG image.
        /// Sets the <see cref="Source"/> to the edited <see cref="DrawingImage"/> and clears the internal edit state.
        /// Throws an exception if no edit session is in progress.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no edit is in progress. Call <see cref="BeginEdit"/> before calling <see cref="EndEdit"/>.
        /// </exception>
        public void EndEdit()
        {
            if (_editInProgressImage == null)
            {
                throw new InvalidOperationException("No edit is in progress. Call BeginEdit to start an edit before calling EndEdit.");
            }
            Source = _editInProgressImage;
            _editInProgressImage = null;
        }

        /// <summary>
        /// Resets the SVG image to its original state by re-parsing and rendering the initially loaded SVG document.
        /// This discards any runtime modifications made to the image, restoring it to how it was when first loaded
        /// from a file, stream, or resource. Throws an exception if no original SVG document is available.
        /// Resets any ongoing edits, do not call EndEdit after calling Reset
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no original SVG document is available to reset to. Ensure the SVG was loaded from a file, stream, or resource before calling <see cref="Reset"/>.
        /// </exception>
        public void Reset()
        {
            if (_originalSVG == null)
            {
                throw new InvalidOperationException("No original SVG document is available to reset to. Ensure the SVG was loaded from a file or stream before calling Reset.");
            }
            Source = new SvgParser().SvgToImage(_originalSVG);
        }

        /// Replaces all occurrences of a specified <see cref="Brush"/> with another <see cref="Brush"/> 
        /// in the rendered SVG image. This method traverses the drawing structure of the SVG and 
        /// updates any matching brushes, allowing for dynamic color or style changes at runtime.
        /// </summary>
        /// <param name="existing">The brush to be replaced.</param>
        /// <param name="replacement">The new brush to use as a replacement.</param>
        public void ReplaceFillBrush(Brush existing, Brush replacement)
        {
            if (Source is DrawingImage drawingImage)
            {
                var clonedImage = _editInProgressImage ?? drawingImage.Clone();
                var drawing = clonedImage.Drawing;
                if (drawing is DrawingGroup drawingGroup)
                {
                    ReplaceFillBrush(drawingGroup, existing, replacement);
                    if(_editInProgressImage == null)
                        Source = clonedImage;
                }
            }
        }

        /// <summary>
        /// Recursively replaces all occurrences of the specified stroke <see cref="Brush"/> in the given <see cref="DrawingGroup"/> 
        /// with a new brush. This method traverses the drawing hierarchy and updates the <see cref="Pen.Brush"/> property 
        /// of any <see cref="GeometryDrawing"/> whose stroke brush matches the specified <paramref name="existing"/> brush,
        /// preserving the original pen thickness and line caps. The replacement brush's opacity is adjusted to match the original.
        /// </summary>
        /// <param name="group">The <see cref="DrawingGroup"/> to search and update.</param>
        /// <param name="existing">The stroke brush to be replaced.</param>
        /// <param name="replacement">The new brush to use as a replacement.</param>
        public void ReplaceStrokeBrush(Brush existing, Brush replacement)
        {
            if (Source is DrawingImage drawingImage)
            {
                var clonedImage = _editInProgressImage ?? drawingImage.Clone();
                var drawing = clonedImage.Drawing;
                if (drawing is DrawingGroup drawingGroup)
                {
                    ReplaceStrokeBrush(drawingGroup, existing, replacement);
                    if(_editInProgressImage == null)
                        Source = clonedImage;
                }
            }
        }

        /// <summary>
        /// Recursively replaces all occurrences of the specified brush in the drawing group with a new brush.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="existing"></param>
        /// <param name="replacement"></param>
        private void ReplaceFillBrush(DrawingGroup group, Brush existing, Brush replacement)
        {
            foreach (var child in group.Children)
            {
                if (child is GeometryDrawing geometryDrawing && SvgHelpers.CompareBrushesByColor(geometryDrawing.Brush, existing))
                {
                    // Apply the replacement brush, adjusting for opacity if necessary
                    var opacity = existing.Opacity;
                    var adjustedReplacement = SvgHelpers.ApplyOpacityToBrush(replacement.Clone(), opacity);
                    geometryDrawing.Brush = adjustedReplacement;
                }
                else if (child is DrawingGroup childGroup)
                {
                    ReplaceFillBrush(childGroup, existing, replacement);
                }
            }
        }

        /// <summary>
        /// Recursively replaces all occurrences of the specified pen brush in the drawing group with a new brush.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="existing"></param>
        /// <param name="replacement"></param>
        private void ReplaceStrokeBrush(DrawingGroup group, Brush existing, Brush replacement)
        {
            foreach (var child in group.Children)
            {
                if (child is GeometryDrawing geometryDrawing && geometryDrawing.Pen != null && SvgHelpers.CompareBrushesByColor(geometryDrawing.Pen.Brush, existing))
                {
                    // Apply the replacement brush, adjusting for opacity if necessary
                    var opacity = existing.Opacity;
                    var adjustedReplacement = SvgHelpers.ApplyOpacityToBrush(replacement.Clone(), opacity);
                    var pen = new Pen(adjustedReplacement, geometryDrawing.Pen.Thickness);
                    pen.StartLineCap = geometryDrawing.Pen.StartLineCap;
                    pen.EndLineCap = geometryDrawing.Pen.EndLineCap;
                    geometryDrawing.Pen = pen;
                }
                else if (child is DrawingGroup childGroup)
                {
                    ReplaceStrokeBrush(childGroup, existing, replacement);
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="SvgImage"/> and sets its <see cref="SvgSourceUri"/> property.
        /// The SVG will be loaded and rendered from the specified URI.
        /// </summary>
        /// <param name="svgSourceUri">The URI of the SVG resource.</param>
        /// <returns>A new <see cref="SvgImage"/> instance.</returns>
        public static SvgImage CreateSvgImage(Uri svgSourceUri)
        {
            var svgImage = new SvgImage();
            svgImage.SvgSourceUri = svgSourceUri;
            return svgImage;
        }

        /// <summary>
        /// Creates a new <see cref="SvgImage"/> from a file path.
        /// Loads and parses the SVG file, then renders it as an image.
        /// </summary>
        /// <param name="filePath">The file path to the SVG file.</param>
        /// <returns>A new <see cref="SvgImage"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the file does not contain a valid SVG document.</exception>
        public static SvgImage CreateSvgImage(string filePath)
        {
            var svgImage = new SvgImage();
            XDocument svgXDoc = XDocument.Load(filePath);
            if (svgXDoc.Root == null || svgXDoc.Root.Name.LocalName != SvgNames.svg)
            {
                throw new InvalidOperationException("The provided file does not contain a valid SVG document.");
            }
            SvgParser parser = new SvgParser();
            svgImage._originalSVG = svgXDoc;  // Store the original SVG document for reset functionality
            svgImage.Source = parser.SvgToImage(svgXDoc);
            return svgImage;
        }

        /// <summary>
        /// Creates a new <see cref="SvgImage"/> from a stream.
        /// Loads and parses the SVG data from the stream, then renders it as an image.
        /// </summary>
        /// <param name="svgStream">A stream containing SVG data.</param>
        /// <returns>A new <see cref="SvgImage"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the stream is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the stream does not contain a valid SVG document.</exception>
        public static SvgImage CreateSvgImage(Stream svgStream)
        {
            var svgImage = new SvgImage();
            if (svgStream == null || svgStream.Length == 0)
            {
                throw new ArgumentException("The provided stream is null or empty.", nameof(svgStream));
            }
            XDocument svgXDoc = XDocument.Load(svgStream);
            if (svgXDoc.Root == null || svgXDoc.Root.Name.LocalName != SvgNames.svg)
            {
                throw new InvalidOperationException("The provided stream does not contain a valid SVG document.");
            }
            SvgParser parser = new SvgParser();
            svgImage._originalSVG = svgXDoc;  // Store the original SVG document for reset functionality
            svgImage.Source = parser.SvgToImage(svgXDoc);
            return svgImage;
        }

        /// <summary>
        /// Gets or sets the URI of the SVG resource to display.
        /// When set, the control loads, parses, and renders the SVG from the specified URI.
        /// </summary>
        public Uri SvgSourceUri
        {
            get { return (Uri)GetValue(SvgSourceUriProperty); }
            set { SetValue(SvgSourceUriProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="SvgSourceUri"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SvgSourceUriProperty =
            DependencyProperty.Register(
                "SvgSourceUri",
                typeof(Uri),
                typeof(SvgImage),
                new PropertyMetadata(null, SvgSourceChanged));

        /// <summary>
        /// Called when the <see cref="SvgSourceUri"/> property changes.
        /// Loads the SVG resource from the specified URI, parses it, and sets the image source.
        /// </summary>
        /// <param name="d">The dependency object (should be <see cref="SvgImage"/>).</param>
        /// <param name="args">The event arguments.</param>
        private static void SvgSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is SvgImage svgImage)
            {
                XDocument? svgXDoc = null;
                var sourceUri = args.NewValue as Uri;

                // Try to get the SVG resource stream from the application resources.
                var stream = Application.GetResourceStream(sourceUri);
                if (stream != null)
                {
                    // Load the SVG XML from the stream.
                    using (var xmlStream = stream.Stream)
                    {
                        svgXDoc = XDocument.Load(xmlStream);
                    }
                }

                // If the SVG was loaded successfully, parse and render it.
                if (svgXDoc != null)
                {
                    SvgParser parser = new();
                    svgImage._originalSVG = svgXDoc; // Store the original SVG document for reset functionality
                    svgImage.Source = parser.SvgToImage(svgXDoc);
                }
            }
        }
    }

    

}
