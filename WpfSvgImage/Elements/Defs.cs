using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfSvgImage.Elements
{
    /// <summary>
    /// Stores reusable SVG elements defined in &lt;defs&gt; blocks, such as gradients, shapes, and brushes.
    /// Allows lookup by string id for later reference (e.g., via url(#id) in SVG attributes).
    /// </summary>
    internal class Defs
    {
        /// <summary>
        /// Internal dictionary holding reusable elements by their SVG id.
        /// </summary>
        private Dictionary<string, object> _definedReusableElements = new();

        /// <summary>
        /// Exposes the dictionary of reusable elements for direct access if needed.
        /// </summary>
        public Dictionary<string, object> Elements
        {
            get => _definedReusableElements;
        }

        /// <summary>
        /// Retrieves a reusable element by id and attempts to cast it to the specified type.
        /// Returns default if not found or if the type does not match.
        /// </summary>
        /// <typeparam name="T">The expected type of the reusable element.</typeparam>
        /// <param name="id">The SVG id of the reusable element.</param>
        /// <returns>The element cast to type T, or default if not found or type mismatch.</returns>
        public T? GetElement<T>(string id)
        {
            if (_definedReusableElements.TryGetValue(id, out var element))
            {
                return element is T value ? value : default;
            }
            return default;
        }
    }
}
