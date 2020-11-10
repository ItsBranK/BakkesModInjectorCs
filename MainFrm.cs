using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using Microsoft.Win32;
using System.Text.Json;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace BakkesModInjectorCs {

    /*
        TO DO:
        - Always injected mode (honestly at this point idk if I'll ever finish this)
        - Proper commenting for everything explaining what it does, I was told you're suppose to do that for github projects but I'm lazy
        - Redo UI (I've gained a lot of knowledge from working on other UI projects, eventually I want to redo a bunch of stuff here as a result)
    */

    public partial class mainFrm : Form {
        bool isInjected = false;
        bool offlineMode = false;

        #region "Form Events"
        public mainFrm() {
            InitializeComponent();
        }

        private void MainFrm_Load(object sender, EventArgs e) {
            this.Text = Properties.Settings.Default.WINDOW_TTILE;
            checkForUpdater();
            checkForUninstaller();
            getFolderDirectory();
            checkServer();
            getJsonSuccess();
            loadSettings();
            loadChangelog();
        }

        private void MainFrm_FormClosed(object sender, FormClosedEventArgs e) {
            Environment.Exit(1);
        }

        private void MainFrm_Resize(object sender, EventArgs e) {
            checkHideMinimize();
        }

        private void OpenTrayBtn_Click(object sender, EventArgs e) {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            TrayIcon.Visible = false;
        }

        private void ExitTrayBtn_Click(object sender, EventArgs e) {
            Environment.Exit(1);
        }

        private void websiteLnk_Click(object sender, EventArgs e) {
            Process.Start("http://bakkesmod.com/");
        }

        private void discordLnk_Click(object sender, EventArgs e) {
            Process.Start("https://discordapp.com/invite/HsM6kAR");
        }

        private void patreonLnk_Click(object sender, EventArgs e) {
            Process.Start("https://www.patreon.com/bakkesmod");
        }

        private void icons8Link_Click(object sender, EventArgs e) {
            Process.Start("https://icons8.com/");
        }
        #endregion

        #region "Loading Events"
        public bool getJsonSuccess() {
            if (!offlineMode) {
                bool succeeded;
                jsonObjects.getJsonObjects(out succeeded);
                return succeeded;
            }
            return false;
        }

        public void getFolderDirectory() {
            string directory = utils.getRocketLeagueFolder();
            if (directory == "FILE_NOT_FOUND") {
                utils.log(MethodBase.GetCurrentMethod(), "Could not locate your \"Launch.log\" file, calling getFolderManually.");
                getFolderManually("Error: Could not locate your \"Launch.log\" file, please manually select where your RocketLeague.exe is located.");
            } else if (directory == "FILE_BLANK") {
                utils.log(MethodBase.GetCurrentMethod(), "Launch.log was found but return empty, calling getFolderManually.");
                getFolderManually("Error: Your \"Launch.log\" file returned empty, usually restarting Rocket League and letting it load fixes this error. In the meantime please manually select where your RocketLeague.exe is located.");
            } else {
                utils.log(MethodBase.GetCurrentMethod(), "Return: " + directory);
                if (directory.Contains("Win32")) {
                    utils.log(MethodBase.GetCurrentMethod(), "Path contains Win32, automatically switching to Win64.");
                    directory.Replace("Win32", "Win64");
                } 
                Properties.Settings.Default.WIN64_FOLDER = directory;
                Properties.Settings.Default.Save();
                checkInstall();
                loadPlugins();
            }
        }

        public void getFolderManually(string msg) {
            MessageBox.Show(msg, "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            utils.log(MethodBase.GetCurrentMethod(), "Opening OpenFileDialog.");

            OpenFileDialog ofd = new OpenFileDialog {
                Title = "Select RocketLeague.exe",
                Filter = "EXE Files (*.exe)|*.exe"
            };

            if (ofd.ShowDialog() == DialogResult.OK) {
                string directory = ofd.FileName;
                directory = directory.Replace("\\RocketLeague.exe", "");
                if (directory.Contains("Win32")) {
                    utils.log(MethodBase.GetCurrentMethod(), "Selected path contains Win32, automatically switching to Win64.");
                    directory.Replace("Win32", "Win64");
                }
                Properties.Settings.Default.WIN64_FOLDER = directory;
                Properties.Settings.Default.Save();
                utils.log(MethodBase.GetCurrentMethod(),"Return: " + directory);
                checkInstall();
                loadPlugins();
            } else {
                MessageBox.Show("Error: Canceled by user, BakkesModInjectorCs cannot run without locating your Win64 folder. The program will now close.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                utils.log(MethodBase.GetCurrentMethod(), "Canceled by user.");
                Environment.Exit(1);
            }
        }

        public void getVersionInfo() {
            Properties.Settings.Default.RL_BUILD = utils.getRocketLeagueBuild();
            Properties.Settings.Default.RL_VERSION = utils.getRocketLeagueVersion();
            Properties.Settings.Default.BM_VERSION = utils.getBakkesModVersion();
            Properties.Settings.Default.Save();
            rlVersionLbl.Text = "Rocket League Version: " + Properties.Settings.Default.RL_VERSION;
            rlBuildLbl.Text = "Rocket League Build: " + Properties.Settings.Default.RL_BUILD;
            injectorVersionLbl.Text = "Injector Version: " + Properties.Settings.Default.INJECTOR_VERSION;
            bmVersionLbl.Text = "Mod Version: " + Properties.Settings.Default.BM_VERSION;
        }

        public void checkRocketLeagueVersion() {
            if (!offlineMode) {
                getVersionInfo();
                utils.log(MethodBase.GetCurrentMethod(), "Checking Build ID.");
                utils.log(MethodBase.GetCurrentMethod(), "Current Build ID: " + Properties.Settings.Default.RL_BUILD);
                utils.log(MethodBase.GetCurrentMethod(), "Latest Build ID(s): " + jsonObjects.getBuildIds());

                if (jsonObjects.isUpdateRequired()) {
                    if (Properties.Settings.Default.RL_BUILD == "FILE_BLANK" || Properties.Settings.Default.RL_BUILD == "FILE_NOT_FOUND") {
                        utils.log(MethodBase.GetCurrentMethod(), "Corrupted appmanifest detected.");
                    } else {
                        utils.log(MethodBase.GetCurrentMethod(), "Build ID mismatch, activating Safe Mode.");
                        if (Properties.Settings.Default.SAFE_MODE)
                            activateSafeMode();
                    }
                } else {
                    utils.log(MethodBase.GetCurrentMethod(), "Build ID match.");
                    processTmr.Start();
                }
            }
        }

        public void activateSafeMode() {
            utils.log(MethodBase.GetCurrentMethod(), "Safe mode has been activated.");
            processTmr.Stop();
            injectionTmr.Stop();
            rocketLeagueLbl.Text = "Safe Mode Enabled.";
            statusLbl.Text = "Mod out of date, please wait for an update.";
        }

        public void activateOfflineMode() {
            utils.log(MethodBase.GetCurrentMethod(), "Offline mode has been activated.");
            this.Text = Properties.Settings.Default.WINDOW_TTILE + " (Offline Mode)";
            offlineMode = true;
        }

        public void checkServer() {
            if (!serversOnline())
                activateOfflineMode();
        }

        public void checkAutoUpdate() {
            if (!offlineMode) {
                if (Properties.Settings.Default.AUTO_UPDATE)
                    checkForUpdates(false);
            }
        }

        public void checkSafeMode() {
            if (Properties.Settings.Default.SAFE_MODE) {
                checkRocketLeagueVersion();
            } else  {
                processTmr.Start();
            }
            getVersionInfo();
        }

        public void checkHideStartup() {
            if (this.WindowState != FormWindowState.Minimized) {
                if (Properties.Settings.Default.STARTUP_MINIMIZE) {
                    this.Hide();
                    TrayIcon.Visible = true;
                } else {
                    TrayIcon.Visible = false;
                }
            }
        }

        public void checkHideMinimize() {
            if (Properties.Settings.Default.MINIMIZE_HIDE) {
                if (this.WindowState == FormWindowState.Minimized) {
                    this.Hide();
                    TrayIcon.Visible = true;
                }
            }
        }

        public void checkTopMost() {
            if (Properties.Settings.Default.TOPMOST) {
                this.TopMost = true;
            } else {
                this.TopMost = false;
            }
        }

        public void loadChangelog() {
            string message = "";
            if (!offlineMode) {
                utils.log(MethodBase.GetCurrentMethod(), "Downloading latest changelog information.");
                message = jsonObjects.getChangelog();
            } else {
                utils.log(MethodBase.GetCurrentMethod(), "Offline mode activated, cannot download changelog information.");
                message = "Offline mode has been activated, cannot download changelog information.";
            }

            if (Properties.Settings.Default.JUST_UPDATED) {
                utils.log(MethodBase.GetCurrentMethod(), "Downloading latest changelog information.");
                changelogBox.Visible = true;
                changelogBtnBackground.Location = new Point(12, 74);
                changelogBox.Text = message;
                Properties.Settings.Default.JUST_UPDATED = false;
                Properties.Settings.Default.Save();
            } else {
                if (Properties.Settings.Default.CHANGELOG_COLLAPSED) {
                    changelogBox.Visible = false;
                    changelogBtnBackground.Location = new Point(12, 294);
                } else {
                    changelogBox.Visible = true;
                    changelogBtnBackground.Location = new Point(12, 74);
                }
                changelogBox.Text = message;
            }
        }
        #endregion

        #region "Home Events"
        private void injectBtn_Click(object sender, EventArgs e) {
            utils.log(MethodBase.GetCurrentMethod(), "Manually injecting dll.");
            injectInstance();
        }

        private void changelogBtn_Click(object sender, EventArgs e) {
            if (changelogBtnBackground.Top == 74) {
                changelogBackground.Visible = false;
                changelogBtn.Size = new Size(576, 25);
                changelogBtnBackground.Location = new Point(12, 294);
                Properties.Settings.Default.CHANGELOG_COLLAPSED = true;
            } else {
                changelogBackground.Visible = true;
                changelogBtn.Size = new Size(576, 26);
                changelogBtnBackground.Location = new Point(12, 74);
                Properties.Settings.Default.CHANGELOG_COLLAPSED = false;
            }

            Properties.Settings.Default.Save();
        }
        #endregion

        #region "Plugin Events"
        public void loadPlugins() {
            pluginsList.Clear();
            if (Directory.Exists(Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod\\plugins")) {
                // Returns an array of all files in the plugins folder (returns their path)
                string[] files = Directory.GetFiles(Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod\\plugins");
                foreach (string f in files) { // Adds each file in the array to the listbox
                    utils.log(MethodBase.GetCurrentMethod(), "Found plugin: " + Path.GetFileName(f));
                    pluginsList.Items.Add(Path.GetFileName(f));
                }
                utils.log(MethodBase.GetCurrentMethod(), "All plugins loaded.");
            } else {
                utils.log(MethodBase.GetCurrentMethod(), "Could not find plugins folder, no plugins loaded.");
            }
        }

        private void uninstallpluginsBtn_Click(object sender, EventArgs e) {
            if (pluginsList.SelectedItems.Count > 0) {
                DialogResult Result = MessageBox.Show("Are you sure you want to uninstall this plugin? This action can not be undone.", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (Result == DialogResult.Yes) {
                    string plugin = pluginsList.SelectedItems[0].Text;
                    string set = plugin.Replace(".dll", ".set");
                    string cfg = plugin.Replace(".dll", ".cfg");
                    string dllFile = Properties.Settings.Default.WIN64_FOLDER + "bakkesmod\\plugins\\" + pluginsList.SelectedItems[0].Text;
                    string setFile = Properties.Settings.Default.WIN64_FOLDER + "bakkesmod\\plugins\\settings\\" + set;
                    string cfgFile = Properties.Settings.Default.WIN64_FOLDER + "bakkesmod\\cfg\\" + cfg;
                    string dataFolder = Properties.Settings.Default.WIN64_FOLDER + "bakkesmod\\data\\" + plugin.Replace(".dll", "");

                    try {
                        utils.log(MethodBase.GetCurrentMethod(), "Attempting to uninstall the selected plugin: \"" + plugin + "\"");
                        if (File.Exists(dllFile)) {
                            File.Delete(dllFile);
                            utils.log(MethodBase.GetCurrentMethod(), "\"" + plugin + "\" has successfully been deleted.");
                        }

                        if (File.Exists(setFile)) {
                            File.Delete(setFile);
                            utils.log(MethodBase.GetCurrentMethod(), "\"" + set + "\" has successfully been deleted.");
                        }

                        if (File.Exists(cfgFile)) {
                            File.Delete(cfgFile);
                            utils.log(MethodBase.GetCurrentMethod(), "\"" + cfg + "\" has successfully been deleted.");
                        }

                        if (Directory.Exists(dataFolder)) {
                            Directory.Delete(dataFolder, true);
                            utils.log(MethodBase.GetCurrentMethod(), "\"" + plugin + "\" data folder has successfully been deleted.");
                        }
                    } catch (Exception ex) {
                        utils.log(MethodBase.GetCurrentMethod(), ex.ToString());
                    }

                    utils.log(MethodBase.GetCurrentMethod(), "Reloading plugins.");
                    loadPlugins();
                }
            }
        }

        private void refreshpluginsBtn_Click(object sender, EventArgs e) {
            loadPlugins();
        }

        private void downloadpluginsBtn_Click(object sender, EventArgs e) {
            Process.Start("https://bakkesplugins.com/");
        }
        #endregion

        #region "Setting Events"
        public void loadSettings()  {
            if (Properties.Settings.Default.AUTO_UPDATE) {
                autoUpdateBox.Checked = true;
            } else {
                autoUpdateBox.Checked = false;
            }
            checkAutoUpdate();

            if (Properties.Settings.Default.SAFE_MODE == true) {
                safeModeBox.Checked = true;
            } else if (Properties.Settings.Default.SAFE_MODE == false) {
                safeModeBox.Checked = false;
            }
            checkSafeMode();

            if (Properties.Settings.Default.STARTUP_RUN == true)  {
                startupRunBox.Checked = true;
            } else if (Properties.Settings.Default.STARTUP_RUN == false) {
                startupRunBox.Checked = false;
            }

            if (Properties.Settings.Default.STARTUP_MINIMIZE == true) {
                startupMinimizeBox.Checked = true;
            } else if (Properties.Settings.Default.STARTUP_MINIMIZE == false) {
                startupMinimizeBox.Checked = false;
            }
            checkHideStartup();

            if (Properties.Settings.Default.MINIMIZE_HIDE == true)  {
                hideMinimizeBox.Checked = true;
            } else if (Properties.Settings.Default.MINIMIZE_HIDE == false) {
                hideMinimizeBox.Checked = false;
            }

            if (Properties.Settings.Default.TOPMOST == true) {
                topMostBox.Checked = true;
            } else if (Properties.Settings.Default.TOPMOST == false) {
                topMostBox.Checked = false;
            }
            checkTopMost();

            if (Properties.Settings.Default.INJECTION_TYPE == "timeout") {
                injectionTimeoutBox.Checked = true;
            } else if (Properties.Settings.Default.INJECTION_TYPE == "manual") {
                injectionManualBox.Checked = true;
            } else if (Properties.Settings.Default.INJECTION_TYPE == "always") {
                injectionAlwaysBox.Checked = true;
            }

            injectionTimeBox.Text = Properties.Settings.Default.TIMEOUT_VALUE.ToString();
            utils.log(MethodBase.GetCurrentMethod(), "All settings have been loaded.");
        }

        public void resetSettings() {
            Properties.Settings.Default.AUTO_UPDATE = true;
            Properties.Settings.Default.SAFE_MODE = true;
            Properties.Settings.Default.STARTUP_RUN = false;
            Properties.Settings.Default.STARTUP_MINIMIZE = false;
            Properties.Settings.Default.MINIMIZE_HIDE = false;
            Properties.Settings.Default.TOPMOST = false;
            Properties.Settings.Default.INJECTION_TYPE = "timeout";
            Properties.Settings.Default.TIMEOUT_VALUE = 2500;
            utils.log(MethodBase.GetCurrentMethod(), "Reset all settings to default.");
            Properties.Settings.Default.Save();
            loadSettings();
        }

        public void setInjectionMethod() {
            string originalFile = Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod\\data\\iertutil.dll";
            string proxyFile = Properties.Settings.Default.WIN64_FOLDER + "\\iertutil.dll";
            string soundFile = Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod\\data\\injected.ogg";

            processTmr.Stop();

            if (File.Exists(soundFile)) {
                utils.log(MethodBase.GetCurrentMethod(), "Refreshing injected.ogg");
                File.Delete(soundFile);
            }

            if (File.Exists(proxyFile)) {
                utils.log(MethodBase.GetCurrentMethod(), "Refreshing proxy iertutil.dll");
                File.Delete(proxyFile);
            }

            if (File.Exists(originalFile)) {
                utils.log(MethodBase.GetCurrentMethod(), "Refreshing original iertutil.dll");
                File.Delete(originalFile);
            }

            if (injectionTimeoutBox.Checked == true) {
                Properties.Settings.Default.INJECTION_TYPE = "timeout";
            } else if (injectionManualBox.Checked == true) {
                Properties.Settings.Default.INJECTION_TYPE = "manual";
            } else if (injectionAlwaysBox.Checked == true) {
                Properties.Settings.Default.INJECTION_TYPE = "always";
            }
            
            //if (Properties.Settings.Default.INJECTION_TYPE == "always") {

            //}

            Properties.Settings.Default.Save();
            processTmr.Start();
        }

        private void autoUpdateBox_CheckedChanged(object sender, EventArgs e) {
            Properties.Settings.Default.AUTO_UPDATE = autoUpdateBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void safeModeBox_CheckedChanged(object sender, EventArgs e) {
            Properties.Settings.Default.SAFE_MODE = safeModeBox.Checked;
            Properties.Settings.Default.Save();
            checkSafeMode();
        }

        private void startupRunBox_CheckedChanged(object sender, EventArgs e) {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            
            if (startupRunBox.Checked == true) {
                key.SetValue("BakkesModInjectorCs", Application.ExecutablePath);
            } else {
                key.DeleteValue("BakkesModInjectorCs", false);
            }

            Properties.Settings.Default.STARTUP_RUN = startupRunBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void startupMinimizeBox_CheckedChanged(object sender, EventArgs e) {
            Properties.Settings.Default.STARTUP_MINIMIZE = startupMinimizeBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void hideMinimizeBox_CheckedChanged(object sender, EventArgs e) {
            Properties.Settings.Default.MINIMIZE_HIDE = hideMinimizeBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void topMostBox_CheckedChanged(object sender, EventArgs e) {
            if (topMostBox.Checked) {
                this.TopMost = true;
            } else {
                this.TopMost = false;
            }

            Properties.Settings.Default.TOPMOST = topMostBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void injectionTimeoutBox_CheckedChanged(object sender, EventArgs e) {
            setInjectionMethod();
        }

        private void injectionManualBox_CheckedChanged(object sender, EventArgs e)  {
            setInjectionMethod();
        }

        private void injectionAlwaysBox_CheckedChanged(object sender, EventArgs e) {
            setInjectionMethod();
        }

        private void injectionTimeBox_ValueChanged(object sender, EventArgs e) {
            Properties.Settings.Default.TIMEOUT_VALUE = Convert.ToInt32(injectionTimeBox.Value);
            Properties.Settings.Default.Save();
        }

        private void manualUpdateBtn_Click(object sender, EventArgs e) {
            checkForUpdates(true);
        }

        private void openFolderBtn_Click(object sender, EventArgs e) {
            string directory = Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod\\";

            if (!Directory.Exists(directory)) {
                utils.log(MethodBase.GetCurrentMethod(), "Could not find the BakkesMod folder.");
                checkInstall();
            } else {
                Process.Start(directory);
                utils.log(MethodBase.GetCurrentMethod(), "Opened the path: " + directory);
            }
        }

        private void exportLogsBtn_Click(object sender, EventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK) {
                string win64_path = Properties.Settings.Default.WIN64_FOLDER;
                string myDocuments_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string logs_path = myDocuments_path + "\\My Games\\Rocket League\\TAGame\\Logs";

                if (!Directory.Exists(Path.GetTempPath() + "\\BakkesModInjectorCs")) {
                    utils.log(MethodBase.GetCurrentMethod(), "Creating temp folder.");
                    Directory.CreateDirectory(Path.GetTempPath() + "BakkesModInjectorCs");
                    utils.log(MethodBase.GetCurrentMethod(), "Folder location: " + Path.GetTempPath());
                }

                List<string> filesToExport = new List<string>();

                if (File.Exists(win64_path + "\\bakkesmod\\bakkesmod.log")) {
                    filesToExport.Add(win64_path + "\\bakkesmod\\bakkesmod.log");
                }

                if (Directory.Exists(win64_path)) {
                    string[] win64 = Directory.GetFiles(win64_path);
                    foreach (string file in win64) {
                        if (file.IndexOf(".mdump") > 0 || file.IndexOf(".mdmp") > 0 || file.IndexOf(".dmp") > 0) {
                            utils.log(MethodBase.GetCurrentMethod(), "Adding files from: " + win64_path);
                            filesToExport.Add(file);
                        }
                    }
                }

                if (Directory.Exists(logs_path)) {
                    string[] logs = Directory.GetFiles(logs_path);
                    foreach (string file in logs) {
                        if (file.IndexOf(".mdump") > 0 || file.IndexOf(".mdmp") > 0 || file.IndexOf(".dmp") > 0 || file.IndexOf(".log") > 0) {
                            utils.log(MethodBase.GetCurrentMethod(), "Adding files from: " + logs_path);
                            filesToExport.Add(file);
                        }
                    }
                }

                string tempName = "crash_logs_" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                string tempDirectory = Path.GetTempPath() + "BakkesModInjectorCs\\" + tempName;
                Directory.CreateDirectory(tempDirectory);

                filesToExport.All((string x) => {
                    FileInfo fi = new FileInfo(x);
                    File.Copy(x, tempDirectory + "\\" + fi.Name);
                    return true;
                });

                utils.log(MethodBase.GetCurrentMethod(), "Creating zip file.");
                ZipFile.CreateFromDirectory(tempDirectory, tempDirectory + ".zip");
                File.Move(tempDirectory + ".zip", fbd.SelectedPath + "\\" + tempName + ".zip");
                utils.log(MethodBase.GetCurrentMethod(), "Deleting temp folder.");
                Directory.Delete(tempDirectory, true);
                MessageBox.Show("Successfully exported crash logs to: " + fbd.SelectedPath.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void resetSettingsBtn_Click(object sender, EventArgs e) {
            resetSettings();
        }

        private void windowTitleBtn_Click(object sender, EventArgs e) {
            nameFrm nf = new nameFrm();
            nf.Show();
            this.Hide();
        }

        private void reinstallBtn_Click(object sender, EventArgs e) {
            DialogResult dr = MessageBox.Show("This will fully remove all BakkesMod files and settings, are you sure you want to continue?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes) {
                string Path = Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod";
                if (Directory.Exists(Path)) {
                    Directory.Delete(Path, true);
                    installBakkesMod();
                }
            }
        }

        private void uninstallBtn_Click(object sender, EventArgs e) {
            try {
                utils.log(MethodBase.GetCurrentMethod(), "Writing BakkesModUninstaller.");
                byte[] fileBytes = Properties.Resources.BakkesModUninstaller;
                File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModUninstaller.exe", fileBytes);
                utils.log(MethodBase.GetCurrentMethod(), "Opening BakkesModUninstaller.");
                Process P = new Process();
                P.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModUninstaller.exe";
                P.Start();
                utils.log(MethodBase.GetCurrentMethod(), "Exiting own environment.");
                Environment.Exit(1);
            } catch (Exception ex) {
                utils.log(MethodBase.GetCurrentMethod(), ex.ToString());
            }
        }
        #endregion

        #region "Tab Events"
        public void activateTab(Label selectedBtn, PictureBox selectedImg, TabPage selectedTab) {
            homeBtn.BackColor = Color.Transparent;
            pluginsBtn.BackColor = Color.Transparent;
            settingsBtn.BackColor = Color.Transparent;
            aboutBtn.BackColor = Color.Transparent;
            homeImg.BackColor = Color.Transparent;
            pluginsImg.BackColor = Color.Transparent;
            settingsImg.BackColor = Color.Transparent;
            aboutImg.BackColor = Color.Transparent;

            selectedBtn.BackColor = Color.White;
            selectedImg.BackColor = Color.White;
            controlTabs.SelectedTab = selectedTab;
        }

        private void homeBtn_Click(object sender, EventArgs e) {
            activateTab(homeBtn, homeImg, homeTab);
        }

        private void homeImg_Click(object sender, EventArgs e) {
            activateTab(homeBtn, homeImg, homeTab);
        }

        private void pluginsBtn_Click(object sender, EventArgs e) {
            activateTab(pluginsBtn, pluginsImg, pluginsTab);
        }

        private void pluginsImg_Click(object sender, EventArgs e) {
            activateTab(pluginsBtn, pluginsImg, pluginsTab);
        }

        private void settingsBtn_Click(object sender, EventArgs e) {
            activateTab(settingsBtn, settingsImg, settingsTab);
        }

        private void settingsImg_Click(object sender, EventArgs e) {
            activateTab(settingsBtn, settingsImg, settingsTab);
        }

        private void aboutBtn_Click(object sender, EventArgs e) {
            activateTab(aboutBtn, aboutImg, aboutTab);
        }

        private void aboutImg_Click(object sender, EventArgs e) {
            activateTab(aboutBtn, aboutImg, aboutTab);
        }
        #endregion

        #region "Injector & Timers"
        public bool isProcessRunning() {
            Process[] currentProcesses = Process.GetProcesses();
            foreach (Process p in currentProcesses) {
                if (p.ProcessName == "RocketLeague")
                    return true;
            }
            return false;
        }

        public bool openProcess() {
            try {
                Process rocketLeague = new Process();
                rocketLeague.StartInfo.FileName = "steam.exe";
                rocketLeague.StartInfo.Arguments = "-applaunch 252950";
                rocketLeague.Start();
                return true;
            } catch {
                return false;
            }
        }

        public bool closeProcess() {
            try {
                Process[] currentProcesses = Process.GetProcesses();
                foreach (Process p in currentProcesses) {
                    string x64 = "Rocket League (64-bit, DX11, Cooked)";
                    if (p.ProcessName == "RocketLeague") {
                        if (p.MainWindowTitle == x64) {
                            p.Kill();
                            return true;
                        }
                    }
                }
                return false;
            } catch {
                return false;
            }
        }

        private void injectionTmr_Tick(object sender, EventArgs e) {
            checkSafeMode();
            if (!isInjected) {
                if (Properties.Settings.Default.INJECTION_TYPE == "timeout") {
                    injectInstance();
                } else if (Properties.Settings.Default.INJECTION_TYPE == "manual") {
                    statusLbl.Text = "Process found, waiting for user to manually inject.";
                    injectBtn.Visible = true;
                }
                injectionTmr.Stop();
            }
        }

        private void processTmr_Tick(object sender, EventArgs e) {
            if (!isProcessRunning()) {
                rocketLeagueLbl.Text = "Rocket League is not running.";
                statusLbl.Text = "Uninjected, waiting for user to start Rocket League.";
                isInjected = false;
                injectBtn.Visible = false;
            } else {
                rocketLeagueLbl.Text = "Rocket League is running.";
                if (!isInjected) {
                    injectionTmr.Interval = Properties.Settings.Default.TIMEOUT_VALUE;
                    if (Properties.Settings.Default.INJECTION_TYPE == "manual") {
                        injectionTmr.Start();
                    } else if (Properties.Settings.Default.INJECTION_TYPE == "timeout") {
                        injectBtn.Visible = false;
                        statusLbl.Text = "Process found, attempting injection.";
                        injectionTmr.Start();
                    } else if (Properties.Settings.Default.INJECTION_TYPE == "always") {
                        statusLbl.Text = "Always injected mode enabled.";
                        processTmr.Stop();
                    }
                }
            }
        }

        void injectInstance() {
            utils.log(MethodBase.GetCurrentMethod(), "Attempting to inject.");

            string dllPath = "null";
            string mainDll = Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod\\dll\\bakkesmod.dll";

            if (File.Exists(mainDll)) {
                dllPath = mainDll;
            }

            if (dllPath != "null") {
                result result = injector.injectorInstance.inject(dllPath);
                switch (result) {
                    case result.FILE_NOT_FOUND:
                        utils.log(MethodBase.GetCurrentMethod(), "Injection failed, could not locate the necessary files.");
                        statusLbl.Text = "Uninjected, could not locate the necessary files.";
                        processTmr.Stop();
                        isInjected = false;
                        break;
                    case result.PROCESS_NOT_FOUND:
                        utils.log(MethodBase.GetCurrentMethod(), "Injection failed, the process was not found.");
                        statusLbl.Text = "Uninjected, waiting for user to start Rocket League.";
                        isInjected = false;
                        break;
                    case result.NO_ENTRY_POINT:
                        utils.log(MethodBase.GetCurrentMethod(), "Injection failed, no entry point was found in the process.");
                        statusLbl.Text = "Injection failed, no entry point for process.";
                        processTmr.Stop();
                        isInjected = false;
                        break;
                    case result.MEMORY_SPACE_FAIL:
                        utils.log(MethodBase.GetCurrentMethod(), "Injection failed, not enough allocated memory space.");
                        statusLbl.Text = "Injection failed, not enough allocated memory space.";
                        processTmr.Stop();
                        isInjected = false;
                        break;
                    case result.MEMORY_WRITE_FAIL:
                        utils.log(MethodBase.GetCurrentMethod(), "Injection failed, could not write to memory.");
                        statusLbl.Text = "Injection failed, could not write to memory.";
                        processTmr.Stop();
                        isInjected = false;
                        break;
                    case result.REMOTE_THREAD_FAIL:
                        utils.log(MethodBase.GetCurrentMethod(), "Injection failed, could not create remote thread.");
                        statusLbl.Text = "Injection failed, could not create remote thread.";
                        processTmr.Stop();
                        isInjected = false;
                        break;
                    case result.NOT_SUPPORTED:
                        utils.log(MethodBase.GetCurrentMethod(), "Injection failed, user is on DX9.");
                        statusLbl.Text = "Injection failed, DX9 is no longer supported.";
                        processTmr.Stop();
                        isInjected = false;
                        break;
                    case result.SUCCESS:
                        utils.log(MethodBase.GetCurrentMethod(), "Successfully injected.");
                        statusLbl.Text = "Successfully injected, changes applied in-game.";
                        isInjected = true;
                        break;
                }
            } else {
                utils.log(MethodBase.GetCurrentMethod(), "Injection failed, could not locat the necessary files.");
                statusLbl.Text = "Uninjected, could not locate the necessary files.";
                processTmr.Stop();
                isInjected = false;
            }
        }
        #endregion

        #region "Installers & Updaters"
        public bool serversOnline() {
            try {
                Ping ping = new Ping();
                PingReply bakkesReply = ping.Send("bakkesmod.com");
                PingReply pastebinReply = ping.Send("pastebin.com");

                if (bakkesReply.Status != IPStatus.Success
                     || pastebinReply.Status != IPStatus.Success) {
                    return false;
                }
                return true;
            } catch (Exception ex) {
                return false;
            }
        }

        public void checkForUpdater() {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe")) {
                utils.log(MethodBase.GetCurrentMethod(), "\"AutoUpdaterCs.exe\" has been located, attempting to delete.");
                try {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe");
                    utils.log(MethodBase.GetCurrentMethod(), "\"AutoUpdaterCs.exe\" has successfully been deleted.");
                } catch (Exception ex) {
                    utils.log(MethodBase.GetCurrentMethod(), ex.ToString());
                }
            } else {
                utils.log(MethodBase.GetCurrentMethod(), "\"AutoUpdaterCs.exe\" was not located.");
            }
        }

        public void checkForUninstaller() {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModUninstaller.exe")) {
                utils.log(MethodBase.GetCurrentMethod(), "\"BakkesModUninstaller.exe\" has been located.");
                try {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModUninstaller.exe");
                    utils.log(MethodBase.GetCurrentMethod(), "\"BakkesModUninstaller.exe\" has successfully been deleted.");
                } catch (Exception ex) {
                    utils.log(MethodBase.GetCurrentMethod(), ex.ToString());
                }
            } else {
                utils.log(MethodBase.GetCurrentMethod(), "\"BakkesModUninstaller.exe\" was not located.");
            }
        }

        public void checkInstall() {
            if (!Directory.Exists(Properties.Settings.Default.WIN64_FOLDER)) {
                utils.log(MethodBase.GetCurrentMethod(), "Failed to locate the Win64 folder.");
                getFolderManually("Error: Could not find Win64 folder, please manually select where your RocketLeague.exe is located.");
            } else {
                utils.log(MethodBase.GetCurrentMethod(), "Successfully located the Win64 folder.");
                if (!Directory.Exists(Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod")) {
                    utils.log(MethodBase.GetCurrentMethod(), "Failed to locate the BakkesMod folder: " + Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod");
                    DialogResult dialogResult = MessageBox.Show("Error: Could not find the BakkesMod folder, would you like to install it? If you are on DX9 press no, DX9 is no longer supported.", "BakkesMod", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    
                    if (dialogResult == DialogResult.Yes) {
                        installBakkesMod();
                    } else {
                        MessageBox.Show("Error: Cannot continue without locating the BakkesMod folder.", "BakkesMod", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(1);
                    }
                } else {
                    utils.log(MethodBase.GetCurrentMethod(), "Successfully located the BakkesMod folder.");
                    getVersionInfo();
                }
            }
        }

        public void checkForUpdates(bool displayResult) {
            if (!offlineMode) {
                getVersionInfo();
                if (getJsonSuccess()) {
                    string injectorVersion = jsonObjects.branksConfig.injectorVersion;
                    utils.log(MethodBase.GetCurrentMethod(), "Checking injector version.");
                    utils.log(MethodBase.GetCurrentMethod(), "Current injector version: " + Properties.Settings.Default.INJECTOR_VERSION);
                    utils.log(MethodBase.GetCurrentMethod(), "Latest injector version: " + injectorVersion);

                    if (Properties.Settings.Default.INJECTOR_VERSION == injectorVersion) {
                        utils.log(MethodBase.GetCurrentMethod(), "Version match, no injector update found.");
                        utils.log(MethodBase.GetCurrentMethod(), "Checking BakkesMod version.");
                        utils.log(MethodBase.GetCurrentMethod(), "Current BakkesMod version: " + Properties.Settings.Default.BM_VERSION);
                        if (!jsonObjects.isUpdateRequired()) {
                            utils.log(MethodBase.GetCurrentMethod(), "No BakkesMod update was found.");

                            if (displayResult)
                                MessageBox.Show("No mod or injector updates were found.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        } else {
                            utils.log(MethodBase.GetCurrentMethod(), "Version mismatch, a BakkesMod update was found.");
                            DialogResult result = MessageBox.Show("A new version of BakkesMod was found, would you like to install it now?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (result == DialogResult.Yes) {
                                if (isProcessRunning()) {
                                    utils.log(MethodBase.GetCurrentMethod(), "Rocket League is running, asking user to close process.");
                                    DialogResult processResult = MessageBox.Show("Rocket League needs to be closed in order to update, would you like to close it now?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (processResult == DialogResult.Yes){
                                        if (closeProcess())
                                            updateBakkesMod();
                                    }
                                } else {
                                    updateBakkesMod();
                                }
                            }
                        }
                    } else {
                        utils.log(MethodBase.GetCurrentMethod(), "Version mismatch, injector update found.");
                        DialogResult result = MessageBox.Show("A new version of BakkesModInjectorCs was found, would you like to install it now?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (result == DialogResult.Yes) {
                            if (isProcessRunning()) {
                                utils.log(MethodBase.GetCurrentMethod(), "Rocket League is running, asking user to close process.");
                                DialogResult processResult = MessageBox.Show("Rocket League needs to be closed in order to update, would you like to close it now?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (processResult == DialogResult.Yes) {
                                    if (closeProcess())
                                        updateInjector();
                                }
                            } else {
                                updateInjector();
                            }
                        }
                    }
                } else {
                    utils.log(MethodBase.GetCurrentMethod(), "Get json objects failed, cannot get the most recent version!");
                    MessageBox.Show("Get json objects failed, cannot get the most recent version!", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            } else {
                utils.log(MethodBase.GetCurrentMethod(), "Offline mode has been activated, cannot check for updates.");
                MessageBox.Show("Offline mode has been activated, cannot check for updates at this time.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void installBakkesMod() {
            string path = Properties.Settings.Default.WIN64_FOLDER;
            string url = jsonObjects.outdatedConfig.update_info.download_url;

            if (!Directory.Exists(path + "\\bakkesmod")) {
                utils.log(MethodBase.GetCurrentMethod(), "Creating BakkesMod folder.");
                Directory.CreateDirectory(path + "\\bakkesmod");
            }

            using (WebClient client = new WebClient()) {
                try {
                    utils.log(MethodBase.GetCurrentMethod(), "Downloading BakkesMod archive.");
                    client.DownloadFile(url, AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
                } catch (Exception ex) {
                    utils.log(MethodBase.GetCurrentMethod(), ex.ToString());
                }
            }

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip")) {
                try {
                    utils.log(MethodBase.GetCurrentMethod(), "Extracting archive.");
                    ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip", path + "\\bakkesmod\\");
                    utils.log(MethodBase.GetCurrentMethod(), "Deleting archive.");
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
                } catch (Exception ex) {
                    utils.log(MethodBase.GetCurrentMethod(), ex.ToString());
                    MessageBox.Show(ex.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } else {
                utils.log(MethodBase.GetCurrentMethod(), "No archive found, cannot extract files.");
            }
        }

        public void updateInjector() {
            try {
                utils.log(MethodBase.GetCurrentMethod(), "Writing AutoUpdaterCs.");
                byte[] fileBytes = Properties.Resources.AutoUpdaterCs;
                File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe", fileBytes);
                utils.log(MethodBase.GetCurrentMethod(), "Opening AutoUpdaterCs.");
                Process P = new Process();
                P.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe";
                P.StartInfo.Arguments = jsonObjects.branksConfig.injectorUrl;
                P.Start();
                utils.log(MethodBase.GetCurrentMethod(), "Exiting own environment.");
                Environment.Exit(1);
            } catch (Exception ex) {
                utils.log(MethodBase.GetCurrentMethod(), ex.ToString());
            }
        }

        public void updateBakkesMod() {
            string url = jsonObjects.outdatedConfig.update_info.download_url;
            using (WebClient client = new WebClient()) {
                try {
                    utils.log(MethodBase.GetCurrentMethod(), "Downloading archive.");
                    client.DownloadFile(url, AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
                } catch (Exception ex) {
                    utils.log(MethodBase.GetCurrentMethod(), ex.ToString());
                    MessageBox.Show(ex.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            statusLbl.Text = "Update found, installing updates...";
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip")) {
                try {
                    using (ZipArchive archive = ZipFile.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip")) {
                        foreach (ZipArchiveEntry entry in archive.Entries) {
                            string destination = Path.GetFullPath(Path.Combine(Properties.Settings.Default.WIN64_FOLDER + "\\bakkesmod\\", entry.FullName));
                            if (entry.Name == "") {
                                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                                continue;
                            }

                            utils.log(MethodBase.GetCurrentMethod(), "Checking existing installed files.");
                            if (destination.ToLower().EndsWith(".cfg") || destination.ToLower().EndsWith(".json"))  {
                                if (File.Exists(destination))
                                    continue;
                            }
                            utils.log(MethodBase.GetCurrentMethod(), "Extracting archive.");
                            entry.ExtractToFile(destination, true);
                        }
                    }
                } catch (Exception ex) {
                    utils.log(MethodBase.GetCurrentMethod(), ex.ToString());
                    MessageBox.Show(ex.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } try {
                utils.log(MethodBase.GetCurrentMethod(), "Removing archive.");
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
            } catch {
                utils.log(MethodBase.GetCurrentMethod(), "Failed to Remove Archive.");
                MessageBox.Show("Failed to remove bakkesmod.zip, try running as administrator if you haven't arlready.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            getVersionInfo();
            getJsonSuccess();
            loadChangelog();
            statusLbl.Text = "Uninjected, waiting for user to start Rocket League.";
        }
        #endregion
    }
}