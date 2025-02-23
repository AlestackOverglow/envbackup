using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using System.Text.Json;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace EnvBackup
{
    public partial class MainWindow : Window
    {
        private readonly string backupFilePath;
        private ObservableCollection<BackupEntry> backups;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize backup file path in AppData
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EnvBackup"
            );
            Directory.CreateDirectory(appDataPath);
            backupFilePath = Path.Combine(appDataPath, "backups.json");

            backups = new ObservableCollection<BackupEntry>();
            LoadBackups();
            BackupListView.ItemsSource = backups;
        }

        private void LoadBackups()
        {
            try
            {
                if (File.Exists(backupFilePath))
                {
                    string json = File.ReadAllText(backupFilePath);
                    var loadedBackups = JsonSerializer.Deserialize<List<BackupEntry>>(json);
                    if (loadedBackups != null)
                    {
                        backups.Clear();
                        foreach (var backup in loadedBackups)
                        {
                            backups.Add(backup);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading backups: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                backups.Clear();
            }
        }

        private void SaveBackups()
        {
            try
            {
                string json = JsonSerializer.Serialize(backups, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(backupFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving backups: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateBackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var environmentVariables = new Dictionary<string, Dictionary<string, string>>();

                // Get system environment variables
                using (var systemEnv = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment"))
                {
                    if (systemEnv != null)
                    {
                        var systemVars = new Dictionary<string, string>();
                        foreach (var name in systemEnv.GetValueNames())
                        {
                            systemVars[name] = systemEnv.GetValue(name)?.ToString() ?? "";
                        }
                        environmentVariables["System"] = systemVars;
                    }
                }

                // Get user environment variables
                using (var userEnv = Registry.CurrentUser.OpenSubKey("Environment"))
                {
                    if (userEnv != null)
                    {
                        var userVars = new Dictionary<string, string>();
                        foreach (var name in userEnv.GetValueNames())
                        {
                            userVars[name] = userEnv.GetValue(name)?.ToString() ?? "";
                        }
                        environmentVariables["User"] = userVars;
                    }
                }

                var backup = new BackupEntry
                {
                    Date = DateTime.Now,
                    Description = $"Backup created on {DateTime.Now:g}",
                    Variables = environmentVariables
                };

                backups.Add(backup);
                SaveBackups();
                MessageBox.Show("Backup created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreBackupButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBackup = BackupListView.SelectedItem as BackupEntry;
            if (selectedBackup == null)
            {
                MessageBox.Show("Please select a backup to restore.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to restore this backup? This will overwrite your current environment variables.",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Restore system variables
                    using (var systemEnv = Registry.LocalMachine.OpenSubKey(
                        @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true))
                    {
                        if (systemEnv != null && selectedBackup.Variables.ContainsKey("System"))
                        {
                            foreach (var variable in selectedBackup.Variables["System"])
                            {
                                systemEnv.SetValue(variable.Key, variable.Value);
                            }
                        }
                    }

                    // Restore user variables
                    using (var userEnv = Registry.CurrentUser.OpenSubKey("Environment", true))
                    {
                        if (userEnv != null && selectedBackup.Variables.ContainsKey("User"))
                        {
                            foreach (var variable in selectedBackup.Variables["User"])
                            {
                                userEnv.SetValue(variable.Key, variable.Value);
                            }
                        }
                    }

                    // Notify shell of environment change
                    SendMessageTimeout(
                        HWND_BROADCAST,
                        WM_SETTINGCHANGE,
                        IntPtr.Zero,
                        "Environment",
                        SMTO_ABORTIFHUNG,
                        5000,
                        out _
                    );

                    MessageBox.Show("Backup restored successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error restoring backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewBackupButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBackup = BackupListView.SelectedItem as BackupEntry;
            if (selectedBackup == null)
            {
                MessageBox.Show("Please select a backup to view.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var backupWindow = new BackupViewWindow(selectedBackup);
            backupWindow.Owner = this;
            backupWindow.ShowDialog();
        }

        private void DeleteBackupButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBackup = BackupListView.SelectedItem as BackupEntry;
            if (selectedBackup == null)
            {
                MessageBox.Show("Please select a backup to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to delete this backup?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                backups.Remove(selectedBackup);
                SaveBackups();
            }
        }

        private void DeleteAllBackupsButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete all backups?",
                "Confirm Delete All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                backups.Clear();
                SaveBackups();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        // P/Invoke for environment change notification
        private const int HWND_BROADCAST = 0xffff;
        private const uint WM_SETTINGCHANGE = 0x001A;
        private const uint SMTO_ABORTIFHUNG = 0x0002;

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SendMessageTimeout(
            int hWnd,
            uint Msg,
            IntPtr wParam,
            string lParam,
            uint fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult
        );
    }

    public class BackupEntry
    {
        public DateTime Date { get; set; }
        public required string Description { get; set; }
        public required Dictionary<string, Dictionary<string, string>> Variables { get; set; }
    }
} 