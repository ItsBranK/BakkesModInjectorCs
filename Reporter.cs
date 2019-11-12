using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

class reporter
{
    public static void writeToLog(String path, String data)
    {
        if (File.Exists(path))
        {
            File.AppendAllText(path, Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss] ") + data);
        }
    }

    public static string getDirFromLog()
    {
        string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string launchFile = myDocuments + "\\My Games\\Rocket League\\TAGame\\Logs\\launch.log";
        string directory = null;

        if (File.Exists(launchFile))
        {
            string line;

            using (FileStream stream = File.Open(launchFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader file = new StreamReader(stream);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("Init: Base directory: "))
                    {
                        line = line.Replace("Init: Base directory: ", "");
                        directory = line;
                        break;
                    }
                }
                if (directory == null)
                {
                    directory = "FILE_BLANK";
                }
            }
        }
        else
        {
            directory = "FILE_NOT_FOUND";
        }

        return directory;
    }

    public static string getRLVersion(String Path)
    {
        string manifestFile = Path + "\\appmanifest_252950.acf";
        string version = "0";
        string pattern = "(\"([^ \"]|\"\")*\")";

        if (File.Exists(manifestFile))
        {
            string line;

            using (FileStream Stream = File.Open(manifestFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader file = new StreamReader(Stream);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("buildid"))
                    {
                        version = Regex.Match(line, pattern, RegexOptions.IgnoreCase | RegexOptions.RightToLeft).Groups[1].Value.Replace("\"", "");
                        break;
                    }
                    else
                    {
                        version = null;
                    }
                }
            }
        }

        return version;
    }

    public static string getBMVersion(String Path)
    {
        string versionFile = Path + "\\bakkesmod\\version.txt";
        string version = "0";

        if (File.Exists(versionFile))
        {
            string line;

            using (FileStream Stream = File.Open(versionFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader file = new StreamReader(Stream);
                while ((line = file.ReadLine()) != null)
                {
                    version = line;
                    break;
                }
            }
        }

        return version;
    }
}