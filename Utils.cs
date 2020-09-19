using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Text.RegularExpressions;

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
                File.AppendAllText(logFile, DateTime.Now.ToString("[HH:mm:ss] ") + "[" + method.Name + "] " + s);
            } else {
                File.AppendAllText(logFile, Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss] ") + "[" + method.Name + "] " + s);
            }
        } else {
            createLogFile();
            log(method, s);
        }
    }

    public static string getDirectory() {
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

    public static string getRocketLeagueVersion(String path) {
        string manifestFile = path + "\\appmanifest_252950.acf";
        string pattern = "(\"([^ \"]|\"\")*\")";
        string version = null;
        if (File.Exists(manifestFile)) {
            string line;
            using (FileStream fs = File.Open(manifestFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                StreamReader sr = new StreamReader(fs);
                while ((line = sr.ReadLine()) != null) {
                    if (line.Contains("buildid")) {
                        version = Regex.Match(line, pattern, RegexOptions.IgnoreCase | RegexOptions.RightToLeft).Groups[1].Value.Replace("\"", "");
                        break;
                    }
                }
                if (version == null)
                    version = "FILE_BLANK";
            }
        } else {
            version = "FILE_NOT_FOUND";
        }
        return version;
    }

    public static string getBakkesModVersion(String path) {
        string versionFile = path + "\\bakkesmod\\version.txt";
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