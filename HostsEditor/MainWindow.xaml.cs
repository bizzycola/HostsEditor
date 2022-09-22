using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using HostsEditor.Parsing;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;

namespace HostsEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisUI.Controls.AdonisWindow
    {
        private readonly HostFileParser _parser;

        private void AdminRelauncher()
        {
            if (!IsRunAsAdmin())
            {
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = Environment.ProcessPath;

                proc.Verb = "runas";

                try
                {
                    Process.Start(proc);
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"This program must be run as an administrator.\n\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }
            }
        }

        private bool IsRunAsAdmin()
        {
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"This program must be run as an administrator.\n\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                return false;
            }
        }

        public MainWindow()
        {
            _parser = new HostFileParser();

            InitializeComponent();
            AdminRelauncher();
            LoadHosts();
        }


        private void SetList()
        {
            try
            {
                hostList.Items.Clear();

                var entries = _parser.GetEntries();
                foreach (var entry in entries)
                    hostList.Items.Add(entry);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadHosts()
        {
            try
            {
                _parser.LoadFile();
                SetList();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            var newEntry = HostDialogWindow.ShowHostBox();
            if (newEntry == null || string.IsNullOrEmpty(newEntry.Host) || string.IsNullOrEmpty(newEntry.IP)) return;

            _parser.AddEntry(newEntry.IP, newEntry.Host, newEntry.Comment, newEntry.Enabled);
            SetList();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var hostEntry = btn.DataContext as HostListItem;
            if (hostEntry == null) return;

            // MessageBox.Show($"Editing \"    {hostEntry.IP}   {hostEntry.Host}\"", "Debug");
            var newEntry = HostDialogWindow.ShowHostBox(hostEntry.IP, hostEntry.Host, hostEntry.Comment);
            if (newEntry == null || string.IsNullOrEmpty(newEntry.Host) || string.IsNullOrEmpty(newEntry.IP)) return;

            _parser.ModifyEntry(hostEntry, newEntry);
            SetList();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var hostEntry = btn.DataContext as HostListItem;
            if (hostEntry == null) return;

            //MessageBox.Show($"Deleting \"    {hostEntry.IP}   {hostEntry.Host}\"", "Debug");
            _parser.RemoveEntry(hostEntry.IP, hostEntry.Host);
            _parser.SaveFile();

            SetList();
        }

        private void checkEnabled_Checked(object sender, RoutedEventArgs e)
        {
            var box = sender as CheckBox;
            if (box == null) return;

            var hostEntry = box.DataContext as HostListItem;
            if (hostEntry == null) return;

            hostEntry.Enabled = true;

            _parser.ModifyEntry(hostEntry, hostEntry);
            SetList();
        }

        private void checkEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            var box = sender as CheckBox;
            if (box == null) return;

            var hostEntry = box.DataContext as HostListItem;
            if (hostEntry == null) return;

            hostEntry.Enabled = false;

            _parser.ModifyEntry(hostEntry, hostEntry);
            SetList();
        }
    }

    
}
