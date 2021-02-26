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
            string url = "";

            for (int i = 0; i < args.Length; i++)
            {
                url += args[i].ToString();
            }

            Install(url);
        }

        static void StartProcess()
        {
            Console.WriteLine("[" + DateTime.Now.ToString() + "] Attemping to Start BakkesModInjectorCs.exe");

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

        static void CheckProcess()
        {
            Console.WriteLine("[" + DateTime.Now.ToString() + "] Checking if BakkesModInjectorCs.exe is running.");

            Process[] proc = Process.GetProcessesByName("BakkesModInjectorCs");

            if (proc.Length != 0)
            {
                Console.WriteLine("[" + DateTime.Now.ToString() + "] Process found, attempting to close BakkesModInjectorCs.exe");

                try
                {
                    Console.WriteLine("[" + DateTime.Now.ToString() + "] Closing BakkesModInjectorCs.exe");
                    proc[0].Kill();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[" + DateTime.Now.ToString() + "] Failed to close process: " + ex.ToString());
                    SelfDestruct();
                }
            }
        }

        static void SelfDestruct()
        {
            Console.WriteLine("[" + DateTime.Now.ToString() + "] Update canceled.");

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip"))
            {
                Thread.Sleep(250);
                Console.WriteLine("[" + DateTime.Now.ToString() + "] Deleting BakkesModInjectorCs.zip");
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip");
            }

            Console.ReadKey(true);
        }

        static void Install(string url)
        {
            //string currentVersion = httpDownloader("https://pastebin.com/raw/BVMKZ4TZ", "(\"([^ \"]|\"\")*\")", "INJECTOR_VERSION");
            //string url = "https://github.com/ItsBranK/BakkesModInjectorCs/releases/download/" + currentVersion + "/BakkesModInjectorCs.zip";

            Console.WriteLine("[" + DateTime.Now.ToString() + "] Download url: " + url);

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.exe"))
            {
                Console.WriteLine("[" + DateTime.Now.ToString() + "] Could not locate BakkesModInjectorCs.exe, canceling update.");
                SelfDestruct();

                return;
            }

            CheckProcess();

            using (WebClient Client = new WebClient())
            {
                try
                {
                    Console.WriteLine("[" + DateTime.Now.ToString() + "] Downloading file.");
                    Client.DownloadFile(url, AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip");
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[" + DateTime.Now.ToString() + "] Failed to download file: " + ex.ToString());
                    Console.ReadKey(true);
                    return;
                }
            }

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip"))
            {
                Console.WriteLine("[" + DateTime.Now.ToString() + "] Could not locate BakkesModInjectorCs.zip, canceling update.");
                SelfDestruct();

                return;
            }
            else
            {
                Console.WriteLine("[" + DateTime.Now.ToString() + "] BakkesModInjectorCs.exe Located.");
                Console.WriteLine("[" + DateTime.Now.ToString() + "] BakkesModInjectorCs.zip Located.");

                try
                {
                    Console.WriteLine("[" + DateTime.Now.ToString() + "] Deleting BakkesModInjectorCs.exe");
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.exe");
                    Thread.Sleep(250);
                    Console.WriteLine("[" + DateTime.Now.ToString() + "] Extracting BakkesModInjectorCs.zip");
                    ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip", AppDomain.CurrentDomain.BaseDirectory);
                    Console.WriteLine("[" + DateTime.Now.ToString() + "] Deleting BakkesModInjectorCs.zip");
                    Thread.Sleep(250);
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModInjectorCs.zip");
                    StartProcess();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[" + DateTime.Now.ToString() + "] Fatal error: " + ex.ToString());
                    Console.ReadKey(true);
                    return;
                }
            }
        }
    }
}
