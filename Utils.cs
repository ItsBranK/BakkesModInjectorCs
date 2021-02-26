using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace BakkesModInjectorCs
{
    public static class Utils
    {
        private static string LogFile = Path.GetTempPath() + "\\BakkesModInjectorCs.log";
        private static bool FirstLine = true;

        // Creates a new text file at the location provided by "logFile" and tries to write to it.
        // If it catches the program might not have permission to read/write which would be weird.
        private static void CreateLogFile()
        {
            try
            {
                StreamWriter sw = new StreamWriter(LogFile);
                sw.Close();
                Log(MethodBase.GetCurrentMethod(), "Initialized logging.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex, "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Logs the calling methods name, the current date/time, and then the provided string.
        public static void Log(MethodBase method, string s)
        {
            if (File.Exists(LogFile))
            {
                if (FirstLine)
                {
                    FirstLine = false;
                    File.WriteAllText(LogFile, String.Empty);
                    File.AppendAllText(LogFile, "[" + DateTime.Now.ToString() + "] [" + method.Name + "] " + s);
                }
                else
                {
                    File.AppendAllText(LogFile, Environment.NewLine + "[" + DateTime.Now.ToString() + "] [" + method.Name + "] " + s);
                }
            }
            else
            {
                CreateLogFile();
                Log(method, s);
            }
        }

        public static void LoadDirectories(out bool succeeded)
        {
            bool bmFolderError = false;
            bool bmVersionError = false;

            string bakkesModFolder = GetBakkesModFolder();
            string bakkesModVersion = GetBakkesModVersion();
            string rocketLeaguePlatform = GetRocketLeaguePlatform();
            string rocketLeagueBuild = GetRocketLeagueBuild();
            string rocketLeagueVersion = GetRocketLeagueVersion();

            if (bakkesModFolder != "FOLDER_NOT_FOUND")
            {
                Log(MethodBase.GetCurrentMethod(), "Found BakkesMod folder: " + bakkesModFolder);

                if (bakkesModVersion != "FILE_NOT_FOUND" && bakkesModVersion != "NULL")
                {
                    Log(MethodBase.GetCurrentMethod(), "Found BakkesMod folder: " + bakkesModFolder);
                }
                else
                {
                    Log(MethodBase.GetCurrentMethod(), "Failed to find the installed BakkesMod version.");
                    bmVersionError = true;
                }
            }
            else
            {
                Log(MethodBase.GetCurrentMethod(), "Failed to locate the BakkesMod folder.");
                bmFolderError = true;
            }

            if (!bmFolderError && !bmVersionError)
            {
                Properties.Settings.Default.BAKKESMOD_FOLDER = bakkesModFolder;
                Properties.Settings.Default.BM_VERSION = bakkesModVersion;
                Properties.Settings.Default.PLATFORM = rocketLeaguePlatform;
                Properties.Settings.Default.RL_BUILD = rocketLeagueBuild;
                Properties.Settings.Default.RL_VERSION = rocketLeagueVersion;

                succeeded = true;
            }
            else
            {
                succeeded = false;
            }

            Properties.Settings.Default.Save();
        }

        public static string GetBakkesModFolder()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\bakkesmod\\bakkesmod";

            if (Directory.Exists(directory))
            {
                return directory;
            }
            else
            {
                return "FOLDER_NOT_FOUND";
            }
        }

        public static string GetBakkesModVersion()
        {
            string bakkesModFolder = GetBakkesModFolder();

            if (bakkesModFolder != "FOLDER_NOT_FOUND")
            {
                string versionFile = bakkesModFolder + "\\version.txt";

                if (File.Exists(versionFile))
                {
                    string line = "NULL";

                    using (FileStream fs = File.Open(versionFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        StreamReader sr = new StreamReader(fs);

                        while ((line = sr.ReadLine()) != null)
                        {
                            break;
                        }

                        return line;
                    }
                }
                else
                {
                    return "FILE_NOT_FOUND";
                }
            }
            else
            {
                return "FOLDER_NOT_FOUND";
            }
        }

        // Uses the Launch.log file created by the game to get the directory that's currently being used.
        public static string GetRocketLeagueFolder()
        {
            string documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string logFile = documentsDir + "\\My Games\\Rocket League\\TAGame\\Logs\\launch.log";

            if (File.Exists(logFile))
            {
                string logContents;
                FileStream fs = File.Open(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                using (StreamReader sr = new StreamReader(fs))
                {
                    logContents = sr.ReadToEnd();
                }

                Match match = Regex.Match(logContents, "Init: Base directory: (.*)", RegexOptions.RightToLeft);

                if (match.Groups[1].Success)
                {
                    string directory = match.Groups[1].Value;
                    return directory.Remove(directory.LastIndexOf("\\"), 2);
                }
                else
                {
                    return "FILE_BLANK";
                }
            }
            else
            {
                return "FILE_NOT_FOUND";
            }
        }

        // Checks the games full directory path for keywords to determine the platform.
        public static string GetRocketLeaguePlatform()
        {
            string rocketLeagueDir = GetRocketLeagueFolder();

            if (rocketLeagueDir != "FOLDER_NOT_FOUND")
            {
                if (rocketLeagueDir.Contains("Epic Games"))
                {
                    return "EPIC";
                }
                else if (rocketLeagueDir.Contains("steamapps"))
                {
                    return "STEAM";
                }
                else
                {
                    return "UNKNOWN";
                }
            }
            else
            {
                return "FOLDER_NOT_FOUND";
            }
        }

        public static string GetRocketLeagueBuild()
        {
            string platform = GetRocketLeaguePlatform();

            if (platform == "STEAM")
            {
                string rocketLeagueDir = GetRocketLeagueFolder();

                if (rocketLeagueDir != "FOLDER_NOT_FOUND")
                {
                    string manifestFile = rocketLeagueDir.Replace("\\common\\rocketleague\\Binaries\\Win64", "\\appmanifest_252950.acf");

                    if (File.Exists(manifestFile))
                    {
                        string manifestContents = File.ReadAllText(manifestFile);
                        Match match = Regex.Match(manifestContents, "(\"buildid\"\t\t\"(.*?)\")", RegexOptions.IgnoreCase);

                        if (match.Groups[2].Success)
                        {
                            return match.Groups[2].Value;
                        }
                        else
                        {
                            return "FILE_BLANK";
                        }
                    }
                    else
                    {
                        return "FILE_NOT_FOUND";
                    }
                }
                else
                {
                    return "FOLDER_NOT_FOUND";
                }
            }
            else if (platform == "EPIC")
            {
                return "Not available on the EGS.";
            }
            else
            {
                return "Not available.";
            }
        }

        public static string GetRocketLeagueVersion()
        {
            string platform = GetRocketLeaguePlatform();

            if (platform == "STEAM")
            {
                string rocketLeagueDir = GetRocketLeagueFolder();
                string version = "NULL";

                if (rocketLeagueDir != "FOLDER_NOT_FOUND")
                {
                    string appinfoFile = rocketLeagueDir.Replace("\\Binaries\\Win64", "\\appinfo.vdf");

                    if (File.Exists(appinfoFile))
                    {
                        string appinfoContents = File.ReadAllText(appinfoFile);
                        Match match = Regex.Match(appinfoContents, "(\"DisplayVersion\"\t\t\"(.*?)\")", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);

                        if (match.Groups[2].Success)
                        {
                            version = match.Groups[2].Value;
                        }
                        else
                        {
                            return  "FILE_BLANK";
                        }
                    }
                    else
                    {
                        return "FILE_NOT_FOUND";
                    }
                }
                else
                {
                    return "FOLDER_NOT_FOUND";
                }

                return version;
            }
            else if (platform == "EPIC")
            {
                return "Not available on the EGS.";
            }
            else
            {
                return "Not available.";
            }
        }
    }
}