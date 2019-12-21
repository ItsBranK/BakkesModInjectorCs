using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace AutoUpdaterCs
{
    class Program
    {
        static void Main(string[] args)
        {
            install();
        }

        public static string httpDownloader(String url, String pattern, String contents)
        {
            string match = "";
            string download = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            download = sr.ReadToEnd();
            sr.Close();

            if (download.Contains(contents))
            {
                match = Regex.Match(download, pattern, RegexOptions.IgnoreCase | RegexOptions.RightToLeft).Groups[1].Value.Replace("\"", "");
            }

            return match;
        }

        static void startProcess()
        {
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Attemping to Start BakkesModInjectorCs.exe");

            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.exe";
                proc.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] Failed to start process: ") + ex.ToString());
                Console.ReadKey(true);
                return;
            }
        }

        static void checkProcess()
        {
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Checking if BakkesModInjectorCs.exe is running.");

            Process[] proc = Process.GetProcessesByName("BakkesModInjectorCs");

            if (proc.Length != 0)
            {
                Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Process found, attempting to close BakkesModInjectorCs.exe");
                try
                {
                    Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Closing BakkesModInjectorCs.exe");
                    proc[0].Kill();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Failed to close process: " + ex.ToString());
                    selfDestruct();
                }
            }
        }

        static void selfDestruct()
        {
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Update canceled.");

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip"))
            {
                Thread.Sleep(250);
                Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Deleting BakkesModInjectorCs.zip");
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip");
            }

            Console.ReadKey(true);
        }

        static void install()
        {
            string currentVersion = httpDownloader("https://pastebin.com/raw/BVMKZ4TZ", "(\"([^ \"]|\"\")*\")", "INJECTOR_VERSION");
            string url = "https://github.com/ItsBranK/BakkesModInjectorCs/releases/download/" + currentVersion + "/BakkesModInjectorCs.zip";

            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Current version: " + currentVersion);
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Download url: " + url);

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.exe"))
            {
                Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Could not locate BakkesModInjectorCs.exe, canceling update.");
                selfDestruct();
                return;
            }

            checkProcess();

            using (WebClient Client = new WebClient())
            {
                try
                {
                    Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Downloading file.");
                    Client.DownloadFile(url, AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip");
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Failed to download file: " + ex.ToString());
                    Console.ReadKey(true);
                    return;
                }
            }

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip"))
            {
                Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Could not locate BakkesModInjectorCs.zip, canceling update.");
                selfDestruct();
                return;
            }
            else
            {
                Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "BakkesModInjectorCs.exe Located.");
                Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "BakkesModInjectorCs.zip Located.");

                try
                {
                    Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Deleting BakkesModInjectorCs.exe");
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.exe");
                    Thread.Sleep(250);
                    Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Extracting BakkesModInjectorCs.zip");
                    ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip", AppDomain.CurrentDomain.BaseDirectory);
                    Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + "Deleting BakkesModInjectorCs.zip");
                    Thread.Sleep(250);
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip");
                    startProcess();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] Fatal error: ") + ex.ToString());
                    Console.ReadKey(true);
                    return;
                }
            }
        }
    }
}
