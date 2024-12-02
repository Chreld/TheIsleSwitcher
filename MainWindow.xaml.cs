using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;
using Microsoft.VisualBasic.FileIO;

namespace TheIsleSwitcher
{
    public partial class MainWindow : Window
    {
        private const string TheIsle = "The Isle";
        private const string Legacy = "_Legacy";
        private const string Evrima = "_Evrima";
        private const string SteamLibrary = "SteamLibrary";

        private string currentVersion = "new version";
        private int searchDepth = 3;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Something catastrophic happened on startup. Please see FAQ or update to the newest version. If the problem persists, contact support at https://github.com/Chreld/TheIsleSwitcherCode with this exception message:\n {ex.Message}");
            }
        }

        private void SwitchToLegacyEvent(object sender, RoutedEventArgs e)
        {
            SwitchVersion(Legacy);
        }

        private void SwitchToEvrimaEvent(object sender, RoutedEventArgs e)
        {
            SwitchVersion(Evrima);
        }

        private void SwitchVersion(string targetVersion)
        {
            try
            {
                var steamPaths = FindSteamLibraries();
                if (!steamPaths.Any())
                {
                    MessageBox.Show("Could not find the Steam Library on your PC.");
                    return;
                }

                var gamePaths = FindGameInstallations(steamPaths, TheIsle);
                if (!gamePaths.Any())
                {
                    MessageBox.Show("Could not find any installations of The Isle on your PC.");
                    return;
                }

                UpdateGameVersions(gamePaths);
                ActivateVersion(gamePaths, targetVersion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private List<string> FindSteamLibraries()
        {
            var steamPaths = new List<string>();
            foreach (var drive in Directory.GetLogicalDrives())
            {
                try
                {
                    SearchDirectories(drive, SteamLibrary, searchDepth, steamPaths);
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
            return steamPaths;
        }

        private void SearchDirectories(string root, string target, int depth, List<string> results)
        {
            if (depth < 0) return;

            try
            {
                foreach (var directory in Directory.GetDirectories(root))
                {
                    if (directory.Contains(target))
                    {
                        results.Add(directory);
                    }
                    else
                    {
                        SearchDirectories(directory, target, depth - 1, results);
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
        }

        private List<string> FindGameInstallations(IEnumerable<string> steamPaths, string gameName)
        {
            var gamePaths = new List<string>();
            foreach (var steamPath in steamPaths)
            {
                try
                {
                    var commonFolder = Path.Combine(steamPath, "steamapps", "common");
                    if (Directory.Exists(commonFolder))
                    {
                        foreach (var gameFolder in Directory.GetDirectories(commonFolder))
                        {
                            if (Directory.GetFiles(gameFolder).Any(x => x.Contains(gameName)))
                            {
                                gamePaths.Add(gameFolder);
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException) { }
            }
            return gamePaths;
        }

        private List<string> UpdateGameVersions(IEnumerable<string> gamePaths)
        {
            var updatedPaths = new List<string>();

            foreach (var gamePath in gamePaths)
            {
                try
                {
                    // Only Evrima has antiCheat, so we can use this to differentiate between the Legacy and Evrima version.
                    var files = Directory.GetFiles(gamePath);
                    var isEvrima = files.Any(x => x.Contains("InstallAntiCheat.bat"));
                    var version = isEvrima ? Evrima : Legacy;
                    var expectedPath = TheIsle + version;

                    if (!Path.GetFileName(gamePath).Equals(expectedPath, StringComparison.OrdinalIgnoreCase))
                    {
                        FileSystem.RenameDirectory(gamePath, expectedPath);
                    }

                    updatedPaths.Add(expectedPath);
                }
                catch (UnauthorizedAccessException) { }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating game version: {ex.Message}");
                }
            }

            return updatedPaths;
        }

        private void ActivateVersion(IEnumerable<string> gamePaths, string targetVersion)
        {
            var targetPath = gamePaths.FirstOrDefault(path => path.Contains(targetVersion));
            if (targetPath == null)
            {
                MessageBox.Show($"The specified version '{targetVersion}' could not be activated.");
                return;
            }

            try
            {
                FileSystem.RenameDirectory(targetPath, TheIsle);
                currentVersion = targetVersion.TrimStart('_');
                MessageBox.Show($"The Isle has been switched to {currentVersion}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error activating version '{targetVersion}': {ex.Message}");
            }
        }
    }
}
