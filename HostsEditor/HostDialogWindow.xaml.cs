using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HostsEditor.Parsing;

namespace HostsEditor
{
    public class HostDialogWindowModel : IDataErrorInfo
    {
        private Regex _domainRegex = new Regex(@"^[\w.]+$");

        public string Host { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;

                if(columnName == "Host")
                {
                    if (!string.IsNullOrEmpty(Host) && !_domainRegex.IsMatch(Host))
                        error = "Invalid hostname.";
                }
                else if(columnName == "IP")
                {
                    if (!string.IsNullOrEmpty(IP) && !IPAddress.TryParse(IP, out _))
                        error = "Invalid IP Address.";
                }

                return error;
            }
        }

        public string Error => string.Empty;
    }

    /// <summary>
    /// Interaction logic for HostDialogWindow.xaml
    /// </summary>
    public partial class HostDialogWindow : AdonisUI.Controls.AdonisWindow
    {
        private Regex _domainRegex = new(@"^[\w.]+$");
        private bool editMode = false;

        /// <summary>
        /// Initalise a new dialog window object with empty values
        /// </summary>
        public HostDialogWindow()
        {
            InitializeComponent();
            DataContext = new HostDialogWindowModel();
        }

        /// <summary>
        /// Initalise a new dialog window object with prefilled values
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="host"></param>
        /// <param name="comment"></param>
        public HostDialogWindow(string ip, string host, string comment = "")
        {
            InitializeComponent();

            var model = new HostDialogWindowModel()
            {
                IP = ip,
                Host = host
            };
            DataContext = model;

            commentBox.Text = comment;
        }

        /// <summary>
        /// Checks validity of host and IP fields
        /// </summary>
        /// <returns></returns>
        private bool IsValid()
        {
            var context = DataContext as HostDialogWindowModel;
            if (context == null) return false;

            var ip = context.IP;
            var host = context.Host;

            if (string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out _))
                return false;
            if (string.IsNullOrEmpty(host) || !_domainRegex.IsMatch(host))
                return false;

            return true;
        }

        /// <summary>
        /// Validates the input fields and completes the input dialog flow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid()) return;

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Cancels the input dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Creates a host item from the input field values and returns it
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public HostListItem GetHostItem()
        {
            var context = DataContext as HostDialogWindowModel;
            if (context == null) throw new Exception("Invalid model context.");

            var ip = context.IP;
            var host = context.Host;

            return new HostListItem()
            {
                IP = Regex.Replace(ip, @"\s+", "").Trim(),
                Host = Regex.Replace(host, @"\s+", "").Trim(),
                Comment = commentBox.Text.Replace("\n", ""),
                Enabled = true
            };
        }

        /// <summary>
        /// Shows the host entry dialog box with empty input fields
        /// </summary>
        /// <returns></returns>
        public static HostListItem? ShowHostBox()
        {
            var window = new HostDialogWindow();
            var accept = window.ShowDialog();
            if (!accept ?? false) return null;

            return window.GetHostItem();
        }

        /// <summary>
        /// Shows the host entry dialog box with input fields prefilled
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="host"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static HostListItem? ShowHostBox(string ip, string host, string comment = "")
        {
            var window = new HostDialogWindow(ip, host, comment);
            var accept = window.ShowDialog();
            if (!accept ?? false) return null;

            return window.GetHostItem();
        }
    }
}
