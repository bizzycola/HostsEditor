using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostsEditor.Parsing
{
    /// <summary>
    /// Defines a single line entry in a host file
    /// </summary>
    public class HostListItem
    {
        /// <summary>
        /// Whether this line is enabled(not commented out)
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Host IP Entry
        /// </summary>
        public string IP { get; set; } = string.Empty;

        /// <summary>
        /// Host entry
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// User comment for this line
        /// </summary>
        public string Comment { get; set; } = string.Empty;
    }
}
