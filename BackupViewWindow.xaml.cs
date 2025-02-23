using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace EnvBackup
{
    public partial class BackupViewWindow : Window
    {
        private readonly BackupEntry backup;

        public BackupViewWindow(BackupEntry backup)
        {
            InitializeComponent();
            this.backup = backup;

            BackupInfoTextBlock.Text = $"Backup from {backup.Date:g}";
            
            VariableTypeComboBox.Items.Add("System Variables");
            VariableTypeComboBox.Items.Add("User Variables");
            VariableTypeComboBox.SelectedIndex = 0;
        }

        private void VariableTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var variableType = VariableTypeComboBox.SelectedIndex == 0 ? "System" : "User";
            
            if (backup.Variables.TryGetValue(variableType, out var variables))
            {
                var variableList = variables.Select(v => new { Name = v.Key, Value = v.Value }).ToList();
                VariablesDataGrid.ItemsSource = variableList;
            }
            else
            {
                VariablesDataGrid.ItemsSource = new List<object>();
            }
        }
    }
} 