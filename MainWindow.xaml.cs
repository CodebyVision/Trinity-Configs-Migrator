using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Trinity_Configs_Migrator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //TCMNav.SelectedItem = tcm_tab_about;
        }

        private void titleBarBtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void titleBarBtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnOldPath_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new OpenFileDialog();
            dialog.FileName = "Authserver or Worldserver Config"; // Default file name
            dialog.DefaultExt = ".conf"; // Default file extension
            dialog.Filter = "Text documents (.conf)|*.conf"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dialog.FileName;
                info_old_path.Text = filename;
            }
        }

        private void BrtnNewPath_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new OpenFileDialog();
            dialog.FileName = "Authserver or Worldserver Config"; // Default file name
            dialog.DefaultExt = ".conf"; // Default file extension
            dialog.Filter = "Text documents (.conf)|*.conf"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dialog.FileName;
                info_new_path.Text = filename;
            }
        }

        private void BtnMigrate_Click(object sender, RoutedEventArgs e)
        {
            migration_info.Text = "Working on migration documents..";

            Task.Delay(1000);

            // get whole text from old file
            string old_config_content = string.Empty;
            try
            {
                // Open the text file using a stream reader.
                using (var sr = new StreamReader(info_old_path.Text))
                {
                    // Read the stream as a string, and write the string to the console.
                    old_config_content = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //get whole text from new file
            string new_config_content = string.Empty;
            try
            {
                // Open the text file using a stream reader.
                using (var sr = new StreamReader(info_new_path.Text))
                {
                    // Read the stream as a string, and write the string to the console.
                    new_config_content = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            int counter = 0;

            // get old config file reference
            string old_config_ref = string.Empty;
            foreach (string line in old_config_content.Split('\n'))
            {
                if (line.StartsWith("["))
                {
                    old_config_ref = line;
                }
            }

            // get new config file reference
            string new_config_ref = string.Empty;
            foreach (string line in new_config_content.Split('\n'))
            {
                if (line.StartsWith("["))
                {
                    new_config_ref = line;
                }
            }

            if (old_config_ref != new_config_ref)
            {
                migration_info.Foreground = new SolidColorBrush(Colors.DarkRed);
                migration_info.Text = "You are trying to migrate settings from 2 different config references, for example transfering authserver settings to worldserver.conf";
            }
            else
            {
                // Read the file and display it line by line.
                foreach (string oldline in old_config_content.Split('\n'))
                {
                    // if line doesn't start with "#", or not null or not empty then definetely found a variable
                    if (!oldline.StartsWith("[") && !oldline.StartsWith("#") && !string.IsNullOrEmpty(oldline) && !string.IsNullOrWhiteSpace(oldline))
                    {
                        // get varialbe name
                        string s_old_var_name = string.Empty;
                        int charLocation = oldline.IndexOf("=", StringComparison.Ordinal);
                        if (charLocation > 0)
                        {
                            s_old_var_name = oldline.Substring(0, charLocation);
                        }

                        string s_old_var_value = oldline.Substring(oldline.LastIndexOf('=') + 1);

                        // check if variable exists in the new config file
                        if (!new_config_content.Contains(s_old_var_name))
                            continue;

                        int lineNumber = 0;
                        foreach (string lineN in new_config_content.Split('\n'))
                        {
                            lineNumber++;
                            if (lineN.Contains(s_old_var_name) && !lineN.StartsWith("#") && lineN != oldline)
                            {
                                LineChanger(oldline.Replace("\n", "").Replace("\r", ""), info_new_path.Text, lineNumber);
                                //MessageBox.Show($"transfering value for {s_old_var_name} to new config at line number [{lineNumber}]");

                                counter++;
                                break;
                            }
                        }
                    }
                }

                if (counter > 0)
                {
                    migration_info.Foreground = new SolidColorBrush(Colors.DarkGreen);
                    migration_info.Text = $"{counter} settings transfered.";
                }
                else
                {
                    migration_info.Foreground = new SolidColorBrush(Colors.DarkOrange);
                    migration_info.Text = $"{counter} settings transfered, nothing different.";
                }
            }
        }

        static void LineChanger(string newText, string fileName, int line_to_edit)
        {
            int currentLineNumber = 0;
            string[] lines = File.ReadAllLines(fileName);
            foreach (string line in lines)
            {
                if (currentLineNumber == line_to_edit)
                {
                    lines[currentLineNumber -1] = newText;
                    break;
                }
                currentLineNumber++;
            }

            File.WriteAllLines(fileName, lines);
        }
    }
}
