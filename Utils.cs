using System;
using System.IO;
using System.Text.RegularExpressions;

class utils {
    public static bool firstWrite = true;
    public static void writeToLog(String path, String data) {
        if (File.Exists(path)) {
            if (firstWrite == true) {
                firstWrite = false;
                File.AppendAllText(path, DateTime.Now.ToString("[HH:mm:ss] ") + data);
                return;
            }
            File.AppendAllText(path, Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss] ") + data);
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