using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;

namespace TheIsleSwitcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string theIsle = "The Isle";
        string legacy = "_Legacy";
        string evrima = "_Evrima";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SwitchToLegacyEvent(object sender, RoutedEventArgs e)
        {
            SearchDrive(legacy);
        }

        private void SwitchToEvrimaEvent(object sender, RoutedEventArgs e)
        {
            SearchDrive(evrima);
        }

        private void SearchDrive(string newerVersion)
        {
            // Get all drive letters on the system.
            string[] drives = Environment.GetLogicalDrives();
            List<string> gameFound = new List<string>();

            // Search for the Steam library folder on each drive.
            foreach (string drive in drives)
            {
                try
                {
                    // The Steam library folder is typically located in the "Program Files (x86)" folder.
                    string steamPath = Path.Combine(drive, "SteamLibrary");

                    // If Steam Library was found.
                    if (Directory.Exists(steamPath))
                    {
                        // Games are store in the 'Common' folder, combite its adress with where we found the Steam Library.
                        string steamAppFolder = Path.Combine(steamPath, "steamapps");
                        string CommonFolder = Path.Combine(steamAppFolder, "common");

                        // This is the address where all the games are stored.
                        string[] files = Directory.GetDirectories(CommonFolder);

                        // Search through each folder in 'Common' and save to list steamGame
                        foreach (var steamGame in files)
                        {
                            if (steamGame.Contains(theIsle))
                            {
                                gameFound.Add(steamGame);
                            }
                        }

                        // Go through the list steamGame, and determine which is Legacy and which is Evrima based on the presence of EasyAntiCheat.
                        foreach (var steamGame in gameFound)
                        {
                            string[] theIsles = Directory.GetFiles(steamGame);

                            try
                            {
                                // Only the Evrima version has EasyAntiCheat.If it's not present then we have found the Legacy version.
                                if (!theIsles.Any(x => x.Contains("InstallAntiCheat.bat")))
                                {
                                    if (!steamGame.Contains(legacy))
                                    {
                                        Directory.Move(steamGame, steamGame + legacy);
                                    }
                                }

                                else
                                {
                                    if(!steamGame.Contains(evrima))
                                    {
                                        Directory.Move(steamGame, steamGame + evrima);
                                    }
                                }
                            }
                            catch (IOException)
                            {
                                Console.WriteLine("Something went wrong while renaming the Legacy and Evrima version.");
                            }
                        }

                        // Activate the version requested.
                        string requestedVersion = gameFound.Where(x => x.Contains(newerVersion)).FirstOrDefault() ?? gameFound.FirstOrDefault();

                        if (requestedVersion.Contains(newerVersion))
                        {
                            Directory.Move(requestedVersion, requestedVersion.Replace(newerVersion, ""));
                        }

                        // Remove the underscore from the _Version for the user message.
                        var currentVersion = newerVersion.Substring(1);

                        // Pop-up window to notify success to user.
                        MessageBox.Show($"The Isle has been switched to {currentVersion}");

                        break;
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Something went wrong while searching the drive.");
                }
            }
        }
    }
}
