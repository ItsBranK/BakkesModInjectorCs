using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace BakkesModInjectorCs {
    public class utils {
        private static string logFile = Path.GetTempPath() + "\\BakkesModInjectorCs.log";
        private static bool firstLine = true;

        private static void createLogFile() {
            try {
                StreamWriter sw = new StreamWriter(logFile);
                sw.Close();
                log(MethodBase.GetCurrentMethod(), "Initialized logging.");
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex, "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static void log(MethodBase method, string s) {
            if (File.Exists(logFile)) {
                if (firstLine) {
                    firstLine = false;
                    File.WriteAllText(logFile, String.Empty);
                    File.AppendAllText(logFile, "[" + DateTime.Now.ToString() + "] [" + method.Name + "] " + s);
                } else {
                    File.AppendAllText(logFile, Environment.NewLine + "[" + DateTime.Now.ToString() + "] [" + method.Name + "] " + s);
                }
            } else {
                createLogFile();
                log(method, s);
            }
        }

        public static string getRocketLeagueFolder() {
            string documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string logFile = documentsDir + "\\My Games\\Rocket League\\TAGame\\Logs\\launch.log";
            string directory = null;
            if (File.Exists(logFile)) {
                string line;
                using (FileStream fs = File.Open(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    StreamReader sr = new StreamReader(fs);
                    while ((line = sr.ReadLine()) != null) {
                        if (line.Contains("Init: Base directory: ")) {
                            line = line.Replace("Init: Base directory: ", "");
                            directory = line;
                            break;
                        }
                    }
                    if (directory == null)
                        directory = "FILE_BLANK";
                }
            } else {
                directory = "FILE_NOT_FOUND";
            }
            return directory;
        }

        public static string getRocketLeagueBuild() {
            string manifestFile = Properties.Settings.Default.WIN64_FOLDER; // getRocketLeagueFolder();
            string build = "";
            if (manifestFile != "FILE_BLANK" && manifestFile != "FILE_NOT_FOUND") {
                manifestFile = manifestFile.Replace("\\common\\rocketleague\\Binaries\\Win64\\", "\\appmanifest_252950.acf");
                if (File.Exists(manifestFile)) {
                    string manifestContents = File.ReadAllText(manifestFile);
                    Match match = Regex.Match(manifestContents, "(\"buildid\"\t\t\"(.*?)\")", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
                    if (match.Groups[2].Success) {
                        build = match.Groups[2].Value;
                    } else {
                        build = "FILE_BLANK";
                    }
                } else {
                    build = "FILE_NOT_FOUND";
                }
            } else {
                build = "FILE_NOT_FOUND";
            }
            return build;
        }


        public static string getRocketLeagueVersion() {
            string appinfoFile = Properties.Settings.Default.WIN64_FOLDER; // getRocketLeagueFolder();
            string version = "";
            if (appinfoFile != "FILE_BLANK" && appinfoFile != "FILE_NOT_FOUND") {
                appinfoFile = appinfoFile.Replace("\\Binaries\\Win64\\", "\\appinfo.vdf");
                if (File.Exists(appinfoFile)) {
                    string appinfoContents = File.ReadAllText(appinfoFile);
                    Match match = Regex.Match(appinfoContents, "(\"DisplayVersion\"\t\t\"(.*?)\")", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
                    if (match.Groups[2].Success) {
                        version = match.Groups[2].Value;
                    } else {
                        version = "FILE_BLANK";
                    }
                } else {
                    version = "FILE_NOT_FOUND";
                }
            } else {
                version = "FILE_NOT_FOUND";
            }
            return version;
        }

        public static string getBakkesModVersion() {
            string versionFile = Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod\\version.txt"; //getRocketLeagueFolder() + "\\bakkesmod\\version.txt";
            string version = null;
            if (File.Exists(versionFile)) {
                string line;
                using (FileStream fs = File.Open(versionFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    StreamReader sr = new StreamReader(fs);
                    while ((line = sr.ReadLine()) != null) {
                        version = line;
                        break;
                    }
                    if (version == null)
                        version = "FILE_BLANK";
                }
            } else {
                version = "FILE_NOT_FOUND";
            }
            return version;
        }
    }
}