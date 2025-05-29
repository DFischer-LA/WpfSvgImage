
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
                    svgImage.Source = parser.SvgToImage(svgXDoc);
                }
            }
        }
    }

    

}
