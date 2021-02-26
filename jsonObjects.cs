using System;
using System.Net;
using System.Text.Json;
using System.Reflection;
using System.Windows.Forms;

namespace BakkesModInjectorCs {
    // https://json2csharp.com/

    public static class JsonObjects
    {
        public static bool ConfigsLoaded = false;
        public static bool UsingOutdated = false;
        public static OutdatedUpdater.Root OutdatedConfig; // This is the json object bakkes' sends when the user is out of date
        public static CurrentUpdater.Root CurrentConfig; // This is the json object bakkes' sends when the user is up to date
        public static BranksUpdater BranksConfig; // This is the json object that this project uses for itself

        public class BranksUpdater
        {
            public string InjectorUrl { get; set; }
            public string BakkesModUrl { get; set; }
            public string InjectorVersion { get; set; }
        }

        public class OutdatedUpdater
        {
            public class Gameinfo
            {
                public string buildid { get; set; }
                public string egsbuildids { get; set; }
                public string buildids { get; set; }
            }

            public class UpdateInfo
            {
                public string github_download_url { get; set; }
                public int trainer_version { get; set; }
                public string download_url { get; set; }
                public string rocketleague_version { get; set; }
                public string message { get; set; }
                public DateTime pub_date { get; set; }
            }

            public class Injector
            {
                public string injectorurl { get; set; }
                public string newinjectorversion { get; set; }
                public string injectorsetupurl { get; set; }
                public string injectorversion { get; set; }
            }

            public class Root
            {
                public bool err { get; set; }
                public Gameinfo gameinfo { get; set; }
                public UpdateInfo update_info { get; set; }
                public bool update_required { get; set; }
                public int backoff_seconds { get; set; }
                public string user_agent { get; set; }
                public Injector injector { get; set; }
            }
        }

        public class CurrentUpdater
        {
            public class Gameinfo
            {
                public string buildid { get; set; }
                public string egsbuildids { get; set; }
                public string buildids { get; set; }
            }

            public class Injector
            {
                public string injectorurl { get; set; }
                public string newinjectorversion { get; set; }
                public string injectorsetupurl { get; set; }
                public string injectorversion { get; set; }
            }

            public class Root
            {
                public Gameinfo gameinfo { get; set; }
                public bool err { get; set; }
                public bool update_required { get; set; }
                public int backoff_seconds { get; set; }
                public string user_agent { get; set; }
                public Injector injector { get; set; }
            }
        }

        public static void GetJsonObjects(out bool succeeded)
        {
            // Downloads the json data and serializes them into objects
            succeeded = false;

            using (WebClient client = new WebClient())
            {
                string branksJson = client.DownloadString("https://pastebin.com/raw/iyFGZYnw");
                BranksConfig = JsonSerializer.Deserialize<BranksUpdater>(branksJson);
            }

            // Simple check to verify that `branksConfig` did actually get populated
            if (!BranksConfig.InjectorUrl.Contains("https"))
            {
                return;
            }

            // Since bakkes uses two different json formats (one if the user is outdated and another if the user is up to date) we have to figure out which is which before trying to deserialize it into an object
            // We can do that just by checking if it contains "update_info" or not, which only the oudated format has. This is a lazy but effective check as long as bakkes doesn't change it anytime soon

            using (WebClient client = new WebClient())
            {
                string updaterJson = client.DownloadString(BranksConfig.BakkesModUrl + Properties.Settings.Default.BM_VERSION);

                if (updaterJson.Contains("update_info"))
                {
                    OutdatedConfig = JsonSerializer.Deserialize<OutdatedUpdater.Root>(updaterJson);
                    UsingOutdated = true;
                }
                else
                {
                    CurrentConfig = JsonSerializer.Deserialize<CurrentUpdater.Root>(updaterJson);
                    UsingOutdated = false;

                    int previousVerison = int.Parse(Properties.Settings.Default.BM_VERSION) - 1;
                    string previousJson = client.DownloadString(BranksConfig.BakkesModUrl + previousVerison.ToString());

                    OutdatedConfig = JsonSerializer.Deserialize<OutdatedUpdater.Root>(previousJson);
                }
            }

            // Another simple check to verify the objects got populated, if not just return as `succeeded` is false by default

            if (UsingOutdated)
            {
                if (!OutdatedConfig.injector.injectorurl.Contains("bakkesmod"))
                {
                    return;
                }
            }
            else
            {
                if (!CurrentConfig.injector.injectorurl.Contains("bakkesmod"))
                {
                    return;
                }
            }

            succeeded = true;
            ConfigsLoaded = true;
        }

        public static string GetChangelog()
        {
            if (!string.IsNullOrEmpty(OutdatedConfig.update_info.message))
            {
                return OutdatedConfig.update_info.message;
            }

            return "No changelog provided for the most recent update.";
        }

        public static string GetBuildIds()
        {
            if (UsingOutdated)
            {
                return OutdatedConfig.gameinfo.buildids;
            }

            return CurrentConfig.gameinfo.buildids;
        }

        public static bool IsUpdateRequired()
        {
            if (UsingOutdated)
            {
                return OutdatedConfig.update_required;
            }

            return CurrentConfig.update_required;
        }
    }
}
