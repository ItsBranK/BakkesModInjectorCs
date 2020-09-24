using System;
using System.Net;
using System.Text.Json;
using System.Reflection;

namespace BakkesModInjectorCs {
    // https://json2csharp.com/

    public static class jsonObjects {
        public static bool configsLoaded = false;
        public static bool usingOutdated = false;
        public static outdatedUpdater.Root outdatedConfig; // This is the json object bakkes' sends when the user is out of date
        public static currentUpdater.Root currentConfig; // This is the json object bakkes' sends when the user is up to date
        public static branksUpdater branksConfig; // This is the json object that this project uses for itself

        public class branksUpdater {
            public string injectorUrl { get; set; }
            public string bakkesmodUrl { get; set; }
            public string injectorVersion { get; set; }
        }

        public class outdatedUpdater {
            public class Gameinfo {
                public string buildid { get; set; }
                public string buildids { get; set; }
            }

            public class UpdateInfo {
                public int trainer_version { get; set; }
                public string rocketleague_version { get; set; }
                public DateTime pub_date { get; set; }
                public string message { get; set; }
                public string download_url { get; set; }
            }

            public class Injector {
                public string injectorurl { get; set; }
                public string injectorversion { get; set; }
            }

            public class Root {
                public bool err { get; set; }
                public Gameinfo gameinfo { get; set; }
                public UpdateInfo update_info { get; set; }
                public bool update_required { get; set; }
                public string user_agent { get; set; }
                public Injector injector { get; set; }
            }
        }

        public class currentUpdater  {
            public class Injector {
                public string injectorurl { get; set; }
                public string injectorversion { get; set; }
            }

            public class Gameinfo {
                public string buildid { get; set; }
                public string buildids { get; set; }
            }

            public class Root {
                public Injector injector { get; set; }
                public bool err { get; set; }
                public Gameinfo gameinfo { get; set; }
                public bool update_required { get; set; }
                public string user_agent { get; set; }
            }
        }

        public static void getJsonObjects(out bool succeeded) {
            // Downloads the json data and serializes them into objects
            succeeded = false;
            using (WebClient client = new WebClient()) {
                string branksJson = client.DownloadString("https://pastebin.com/raw/iyFGZYnw");
                branksConfig = JsonSerializer.Deserialize<branksUpdater>(branksJson);
            }

            // Simple check to verify that `branksConfig` did actually get populated
            if (!branksConfig.injectorUrl.Contains("https"))
                return;

            // Since bakkes uses two different json formats (one if the user is outdated and another if the user is up to date) we have to figure out which is which before trying to deserialize it into an object
            // We can do that just by checking if it contains "update_info" or not, which only the oudated format has. This is a lazy but effective check as long as bakkes doesn't change it anytime soon
            using (WebClient client = new WebClient()) {
                string updaterJson = client.DownloadString(branksConfig.bakkesmodUrl + Properties.Settings.Default.BM_VERSION);
                utils.log(MethodBase.GetCurrentMethod(), updaterJson);
                if (updaterJson.Contains("update_info")) {
                    outdatedConfig = JsonSerializer.Deserialize<outdatedUpdater.Root>(updaterJson);
                    usingOutdated = true;
                } else {
                    currentConfig = JsonSerializer.Deserialize<currentUpdater.Root>(updaterJson);
                    usingOutdated = false;
                }
            }

            // Another simple check to verify the objects got populated, if not just return as `succeeded` is false by default
            if (usingOutdated) {
                if (!outdatedConfig.injector.injectorurl.Contains("bakkesmod"))
                    return;
            } else {
                if (!currentConfig.injector.injectorurl.Contains("bakkesmod"))
                    return;
            }

            succeeded = true;
            configsLoaded = true;
        }

        public static string getChangelog() {
            if (usingOutdated)
                return outdatedConfig.update_info.message;

            return "No changelog provided for the most recent update.";
        }

        public static string getBuildIds() {
            if (usingOutdated)
                return outdatedConfig.gameinfo.buildids;

            return currentConfig.gameinfo.buildids;
        }

        public static bool isUpdateRequired() {
            if (usingOutdated)
                return outdatedConfig.update_required;

            return currentConfig.update_required;
        }
    }
}
