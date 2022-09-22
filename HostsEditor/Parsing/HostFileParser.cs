using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
 
namespace HostsEditor.Parsing
{
    /// <summary>
    /// Class for parsing and managing hostfile entries within a specific section
    /// </summary>
    public class HostFileParser
    {
        private const string SECTION_TOP = "#### START HOSTFILEPARSER ####";
        private const string SECTION_BOTTOM = "#### END HOSTFILEPARSER ####";
        private readonly string HOST_PATH;
        private readonly Regex _domainRegex;
        private List<string> _hostData;
        private List<HostListItem> _newData;
        
        private int _sectionStart = 0;
        private int _sectionStop = 0;
        
        public HostFileParser()
        {
            _domainRegex = new(@"^[\w.]+$");
            _hostData = new();
            _newData = new();

            HOST_PATH = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "drivers",
                "etc",
                "hosts"
            );
        }

        /// <summary>
        /// Replaces values in an existing entry with values from the provided model
        /// </summary>
        /// <param name="current"></param>
        /// <param name="newItem"></param>
        public void ModifyEntry(HostListItem current, HostListItem newItem)
        {
            var itm = _newData.FirstOrDefault(p => p.IP == current.IP && p.Host.ToLower() == current.Host.ToLower());
            if (itm == null) return;

            var index = _newData.IndexOf(itm);
            _newData[index].IP = newItem.IP;
            _newData[index].Host = newItem.Host;
            _newData[index].Comment = newItem.Comment;
            _newData[index].Enabled = newItem.Enabled;

            SaveFile();
        }

        /// <summary>
        /// Returns a list of all entries
        /// </summary>
        /// <returns></returns>
        public List<HostListItem> GetEntries()
            => _newData;


        /// <summary>
        /// Adds a new entry to the list and saves the file
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="host"></param>
        /// <param name="comment"></param>
        /// <param name="enabled"></param>
        public void AddEntry(string ip, string host, string comment, bool enabled = true)
        {
            _newData.Add(new HostListItem()
            {
                IP = ip,
                Host = host,
                Comment = comment,
                Enabled = enabled
            });

            SaveFile();
        }

        /// <summary>
        /// Removes an entry from the list by IP and host and saves the file
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="host"></param>
        public void RemoveEntry(string ip, string host)
        {
            var entry = _newData
                .FirstOrDefault(p => p.IP == ip && p.Host.ToLower() == host.ToLower().Trim());

            if (entry != null)
                _newData.Remove(entry);

            SaveFile();
        }

        /// <summary>
        /// Load and parse the host file into a list of models we can work with
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void LoadFile()
        {
            _hostData = new List<string>();
            _newData = new List<HostListItem>();

            var data = File.ReadAllText(HOST_PATH);

            // find our section or add it to the file if it doesn't exist
            if (!data.Contains(SECTION_TOP))
                data += "\n" + SECTION_TOP + "\n\n" + SECTION_BOTTOM;

            // Split data
            _hostData = data.Split('\n').ToList();

            // Locate line numbers for our section
            var start = _hostData.FirstOrDefault(p => p.Contains(SECTION_TOP));
            if (string.IsNullOrWhiteSpace(start))
                throw new Exception("Failed to load HOST file: couldn't locate or create our section header");

            var end = _hostData.FirstOrDefault(p => p.Contains(SECTION_BOTTOM));
            if (string.IsNullOrWhiteSpace(end))
                throw new Exception("Failed to load HOST file: couldn't locate or create our section footer");

            _sectionStart = _hostData.IndexOf(start);
            _sectionStop = _hostData.IndexOf(end);

            // Load our section data
            var sind = _sectionStart + 1;
            for (var i = sind; i < _sectionStop; i++)
            {
                var parsed = ParseFromString(_hostData[i]);
                if (parsed == null) continue;

                _newData.Add(parsed);
            }
        }

        /// <summary>
        /// Parses a single line into a line model
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private HostListItem? ParseFromString(string line)
        {
            // Remove excess spaces
            line.Trim();
            line = Regex.Replace(line, @"\s+", " ").Trim();

            // Check for a comment at the beginning of the line
            // Record enabled state and remove comment
            bool enabled = !line.StartsWith('#');
            line = line.Replace("#", "");

            // Remove excess spaces created by lack of comment
            line.Trim();
            line = Regex.Replace(line, @"\s+", " ").Trim();

            // Split line into sections
            var split = line.Split(' ');

            // Minimum of 2 sections required (IP and host)
            if (split.Length < 2)
                return null;

            // Extract section values
            var ip = split[0];
            var host = split[1];

            // Validate IP and host are valid
            if (!IPAddress.TryParse(ip, out _))
                return null;

            if (!_domainRegex.IsMatch(host))
                return null;

            // Extract comment from the end of the line
            string comment = "";
            if (split.Length > 2)
                comment = string.Join(" ", split.Skip(2));

            if (comment.StartsWith("#"))
                comment = comment.Substring(1, comment.Length);

            // Construct and return line model object
            return new HostListItem()
            {
                IP = ip,
                Host = host,
                Enabled = enabled,
                Comment = comment
            };
        }

        /// <summary>
        /// Saves list of line models to host file
        /// </summary>
        public void SaveFile()
        {
            // Get new host OG host data with lines between our sections removed
            var newHostData = new List<string>();
            newHostData.AddRange(_hostData.GetRange(0, _sectionStart + 1));

            // Add our entries
            foreach (var entry in _newData)
                newHostData.Add($"{(entry.Enabled ? " " : "#")}   {entry.IP}   {entry.Host} #{entry.Comment}");

            // Add the rest of the entries
            newHostData.AddRange(_hostData.GetRange(_sectionStop, _hostData.Count - _sectionStop));

            // Save hosts file
            var newFileData = string.Join('\n', newHostData);
            File.WriteAllText(HOST_PATH, newFileData);
        }
    }
}
