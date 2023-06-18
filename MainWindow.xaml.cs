using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;
using Microsoft.VisualBasic.FileIO;

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
        string currentVersion = "new version";
        string steamLibrary = "SteamLibrary";

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
            string[] drives = Directory.GetLogicalDrives();

            // Lists of the URIs for the versions we found.
            List<string> steamPaths = new List<string>();
            List<string> gameFound = new List<string>();
            List<string> newTheIslesContent = new List<string>();

            // Search for the Steamlibrary folder on each drive.
            foreach (string drive in drives)
            {

                var driveContents = Directory.GetDirectories(drive);

                /*
                 * Search through the contents of the driver.
                 * 
                 * Note: A recursive method would be easier to maintain (and more efficent), but I couldn't quickly come up with a limited scope for the search,
                 * where it didn't spend way too long (hours maybe) searching all folders on the entire PC.
                 * So we limit it to 3 subfolders with less efficent but limited search scope.
                 * It is assumed that the user has their SteamLibary installed at a default place, but we widen the scope a little deeper.
                 */
                foreach (var driveContent in driveContents)
                {
                    try
                    {
                        if (driveContent.Contains(steamLibrary))
                        {
                            steamPaths.Add(driveContent.ToString());
                        }
                        else
                        {
                            var driveContentOne = Directory.GetDirectories(driveContent);

                            foreach (var depthOne in driveContentOne)
                            {
                                if (depthOne.Contains(steamLibrary))
                                {
                                    steamPaths.Add(depthOne.ToString());
                                }
                                else
                                {
                                    var driveContentTwo = Directory.GetDirectories(depthOne);

                                    foreach (var depthTwo in driveContentTwo)
                                    {
                                        if (depthTwo.Contains(steamLibrary))
                                        {
                                            steamPaths.Add(depthTwo.ToString());
                                        }
                                        else
                                        {
                                            var driveContentThree = Directory.GetDirectories(depthTwo);

                                            foreach (var depthThree in driveContentThree)
                                            {
                                                if (depthThree.Contains(steamLibrary))
                                                {
                                                    steamPaths.Add(depthThree.ToString());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not find any Steam Library on your PC.\nException: {ex}");
                    }

                }
            }

            // Search through the steamLibraries for the game, The Isle.
            try
            {
                // If Steam Library was found.
                foreach (var steamPath in steamPaths)
                {
                    // Games are stored in the 'Common' folder, combite its adress with where we found the Steam Library.
                    string steamAppFolder = Path.Combine(steamPath, "steamapps");
                    string CommonFolder = Path.Combine(steamAppFolder, "common");

                    // This is the address where all the games are stored.
                    string[] files = Directory.GetDirectories(CommonFolder);

                    // Search through each folder in 'Common' and save to list steamGame
                    foreach (var steamGame in files)
                    {
                        var steamGameContent = Directory.GetFiles(steamGame);

                        if (steamGameContent.Any(x => x.Contains(theIsle)))
                        {
                            gameFound.Add(steamGame);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not find 'The Isle' installed on your PC.\nException: {ex}.");
            }

            // Go through the list steamGame, and determine which is Legacy and which is Evrima based on the presence of EasyAntiCheat.
            try
            {
                foreach (var steamGame in gameFound)
                {
                    string[] theIslesContent = Directory.GetFiles(steamGame);

                    // Only the Evrima version has EasyAntiCheat.If it's not present then we have found the Legacy version.
                    if (!theIslesContent.Any(x => x.Contains("InstallAntiCheat.bat")))
                    {
                        //if (steamGame != CommonFolder + "\\" + theIsle + legacy)
                        //TODO: Fix CommonFolder so it uses the correct URI path.
                        if (!steamGame.Contains(theIsle + legacy))
                        {
                            FileSystem.RenameDirectory(steamGame, theIsle + legacy);
                        }
                        var testPath = steamGame.Substring(0, steamGame.LastIndexOf("\\") + 1);
                        newTheIslesContent.Add(testPath + theIsle + legacy);
                    }

                    else
                    {
                        //if (steamGame != CommonFolder + "\\" + theIsle + evrima)
                        if (!steamGame.Contains(theIsle + evrima))
                        {
                            FileSystem.RenameDirectory(steamGame, theIsle + evrima);
                        }
                        var testPath = steamGame.Substring(0, steamGame.LastIndexOf("\\") + 1);
                        newTheIslesContent.Add(testPath + theIsle + evrima);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong while renaming the Legacy and Evrima version.\nException: {ex}.");
            }

            // Activate the version requested.
            if (newTheIslesContent.Count > 0)
            {
                string requestedVersion = newTheIslesContent.Where(x => x.Contains(newerVersion)).FirstOrDefault();

                if (requestedVersion.Contains(newerVersion))
                {
                    FileSystem.RenameDirectory(requestedVersion, theIsle);
                }

                // Remove the underscore from the _Version for the user message.
                currentVersion = newerVersion.Substring(1);

                // Pop-up window to notify success to user.
                MessageBox.Show($"The Isle has been switched to {currentVersion}.");
            }
            else
            {
                MessageBox.Show($"Something went wrong during activation of {currentVersion}.");
            }
        }
    }
}