using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace BakkesModInjectorCs
{
    public partial class MainFrm : Form
    {
        string updaterURL = "http://149.210.150.107/updater/";
        string logPath = Path.GetTempPath() + "\\BakkesModInjectorCs.log";
        Boolean isInjected = false;

        #region "Form Events"
        public MainFrm()
        {
            InitializeComponent();
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            createLogger();
            checkForUpdater();
            getFolderPath();
            getVersions();
            loadSettings();
            loadChangelog();
        }

        private void MainFrm_Shown(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.STARTUP_MINIMIZE == true)
            {
                TrayIcon.Visible = true;
                this.Visible = false;
            }
        }

        private void MainFrm_Resize(object sender, EventArgs e)
        {
            checkHideMinimize();
        }

        private void OpenTrayBtn_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void ExitTrayBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void websiteLnk_Click(object sender, EventArgs e)
        {
            Process.Start("http://bakkesmod.com/");
        }

        private void discordLnk_Click(object sender, EventArgs e)
        {
            Process.Start("https://discordapp.com/invite/HsM6kAR");
        }

        private void patreonLnk_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.patreon.com/bakkesmod");
        }

        private void icons8Link_Click(object sender, EventArgs e)
        {
            Process.Start("https://icons8.com/");
        }
        #endregion

        #region "Loading Events"
        public void createLogger()
        {
            try
            {
                StreamWriter logFile = new StreamWriter(logPath);
                logFile.Close();
                reporter.writeToLog(logPath, "(createLogger) Initialized Logging.");
            }
            catch
            {

            }
        }

        public void getFolderPath()
        {
            string directory = reporter.getDirFromLog();

            if (directory == "FILE_BLANK")
            {
                reporter.writeToLog(logPath, "(getFolderPath) Launch.log Found, Return Empty. Calling GetFolderPathOverride.");
                getFolderPathOverride("Error: Launch.log file returned empty, usually restarting Rocket League and letting it load fixes this error. In the meantime please manually select where your RocketLeague.exe is located.");
            }
            else if (directory == "FILE_NOT_FOUND")
            {
                reporter.writeToLog(logPath, "(getFolderPath) Launch.log Not Found, Calling GetFolderPathOverride.");
                getFolderPathOverride("Error: Could not locate your Launch.log file, please manually select where your RocketLeague.exe is located.");
            }
            else
            {
                reporter.writeToLog(logPath, "(getFolderPath) Return: " + directory);
                Properties.Settings.Default.WIN32_FOLDER = directory;
                Properties.Settings.Default.Save();
                checkInstall();
                loadPlugins();
            }
        }

        public void getFolderPathOverride(string Message)
        {
            MessageBox.Show(Message, "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            reporter.writeToLog(logPath, "(getFolderPathOverride) Opening OpenFileDialog.");

            OpenFileDialog OFD = new OpenFileDialog
            {
                Title = "Select RocketLeague.exe",
                Filter = "EXE Files (*.exe)|*.exe"
            };

            if (OFD.ShowDialog() == DialogResult.OK)
            {
                string FilePath = OFD.FileName;
                FilePath = FilePath.Replace("\\RocketLeague.exe", "");
                Properties.Settings.Default.WIN32_FOLDER = FilePath;
                Properties.Settings.Default.Save();
                reporter.writeToLog(logPath, "(getFolderPathOverride) Return: " + FilePath);
                checkInstall();
                loadPlugins();
            }
            else
            {
                MessageBox.Show("Error: Canceled by user, BakkesModInjectorCs cannot run without locating your Win32 folder.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                reporter.writeToLog(logPath, "(getFolderPathOverride) Canceled by User.");
                Environment.Exit(1);
            }
        }

        public void getVersions()
        {
            Properties.Settings.Default.RL_VERSION = reporter.getRLVersion(Properties.Settings.Default.WIN32_FOLDER + "/../../../../");
            Properties.Settings.Default.BM_VERSION = reporter.getBMVersion(Properties.Settings.Default.WIN32_FOLDER);
            Properties.Settings.Default.Save();
            rlVersionLbl.Text = "Rocket League Build: " + Properties.Settings.Default.RL_VERSION;
            injectorVersionLbl.Text = "Injector Version: " + Properties.Settings.Default.INJECTOR_VERSION;
            bmVersionLbl.Text = "Mod Version: " + Properties.Settings.Default.BM_VERSION;
        }

        public void checkRocketLeague()
        {
            if (Properties.Settings.Default.OFFLINE_MODE == false)
            {
                reporter.writeToLog(logPath, "(checkRocketLeague) Checking Build ID.");
                reporter.writeToLog(logPath, "(checkRocketLeague) Current Build ID: " + Properties.Settings.Default.RL_VERSION);

                if (latestBuild(updaterURL + Properties.Settings.Default.BM_VERSION) == false)
                {
                    if (Properties.Settings.Default.RL_VERSION == null)
                    {
                        reporter.writeToLog(logPath, "(checkRocketLeague) Corrupted Appmanifest Detected.");
                    }
                    else
                    {
                        reporter.writeToLog(logPath, "(checkRocketLeague) Build ID Mismatch, Activating Safe Mode.");
                        if (Properties.Settings.Default.SAFE_MODE == true)
                        {
                            activateSafeMode();
                        }
                    }
                }
                else
                {
                    reporter.writeToLog(logPath, "(CheckRL) Build ID Match.");
                    processTmr.Start();
                }
            }
        }

        public void checkD3D9()
        {
            if (File.Exists(Properties.Settings.Default.WIN32_FOLDER + "\\d3d9.dll"))
            {
                reporter.writeToLog(logPath, "(checkD3D9) D3D9.dll has been located.");
                DialogResult DialogResult = MessageBox.Show("Warning: d3d9.dll detected. This file is used by ReShade/uMod and might prevent the GUI from working. Would you like BakkesModInjectorCs to remove this file?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (DialogResult == DialogResult.Yes)
                {
                    File.Delete(Properties.Settings.Default.WIN32_FOLDER + "\\d3d9.dll");
                    reporter.writeToLog(logPath, "(checkD3D9) D3D9.dll has successfully been deleted.");
                }
            }
            else
            {
                reporter.writeToLog(logPath, "(checkD3D9) D3D9.dll does not exist.");
            }
        }

        public void activateSafeMode()
        {
            reporter.writeToLog(logPath, "(activateSafeMode) Safe Mode Activated.");
            processTmr.Stop();
            rocketLeagueLbl.Text = "Safe Mode Enabled.";
            statusLbl.Text = "Mod out of date, please wait for an update.";
        }

        public void activateOfflineMode()
        {
            reporter.writeToLog(logPath, "(activateOfflineMode) Safe Mode Activated.");
            this.Text = "BakkesModInjectorCs - Community Edition (Offline Mode)";
            MessageBox.Show("Warning: Failed to connect to the update server, Offline Mode has been enabled. Some features are disabled.", "BakkesMod", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Properties.Settings.Default.OFFLINE_MODE = true;
            Properties.Settings.Default.Save();
        }

        public void checkServer()
        {
            if (serverOnline() != true)
            {
                activateOfflineMode();
            }
        }

        public void checkAutoUpdate()
        {
            if (Properties.Settings.Default.OFFLINE_MODE == false)
            {
                if (Properties.Settings.Default.AUTO_UPDATE == true)
                {
                    checkForUpdates(false);
                }
            }
        }

        public void checkSafeMode()
        {
            if (Properties.Settings.Default.SAFE_MODE == true)
            {
                checkRocketLeague();
            }
            else if (Properties.Settings.Default.SAFE_MODE == false)
            {
                processTmr.Start();
            }

            getVersions();
        }

        public void checkWarnings()
        {
            if (Properties.Settings.Default.DISABLE_WARNINGS == true)
            {

            }
            else if (Properties.Settings.Default.DISABLE_WARNINGS == false)
            {
                checkD3D9();
            }
        }

        public void checkHideMinimize()
        {
            if (Properties.Settings.Default.MINIMIZE_HIDE == true)
            {
                this.Hide();
                TrayIcon.Visible = true;
            }
            else if (Properties.Settings.Default.MINIMIZE_HIDE == false)
            {
                TrayIcon.Visible = false;
            }
        }

        public void checkTopMost()
        {
            if (Properties.Settings.Default.TOPMOST == true)
            {
                this.TopMost = true;
            }
            else if (Properties.Settings.Default.TOPMOST == false)
            {
                this.TopMost = false;
            }
        }

        public void loadSettings()
        {
            checkServer();
            if (Properties.Settings.Default.AUTO_UPDATE == true)
            {
                autoUpdateBox.Checked = true;
            }
            else if (Properties.Settings.Default.AUTO_UPDATE == false)
            {
                autoUpdateBox.Checked = false;
            }
            checkAutoUpdate();
            if (Properties.Settings.Default.SAFE_MODE == true)
            {
                safeModeBox.Checked = true;
            }
            else if (Properties.Settings.Default.SAFE_MODE == false)
            {
                safeModeBox.Checked = false;
            }
            checkSafeMode();
            if (Properties.Settings.Default.DISABLE_WARNINGS == true)
            {
                warningsBox.Checked = true;
            }
            else if (Properties.Settings.Default.DISABLE_WARNINGS == false)
            {
                warningsBox.Checked = false;
            }
            checkWarnings();
            if (Properties.Settings.Default.STARTUP_RUN == true)
            {
                startupRunBox.Checked = true;
            }
            else if (Properties.Settings.Default.STARTUP_RUN == false)
            {
                startupRunBox.Checked = false;
            }
            if (Properties.Settings.Default.STARTUP_MINIMIZE == true)
            {
                startupMinimizeBox.Checked = true;
            }
            else if (Properties.Settings.Default.STARTUP_MINIMIZE == false)
            {
                startupMinimizeBox.Checked = false;
            }
            if (Properties.Settings.Default.MINIMIZE_HIDE == true)
            {
                hideMinimizeBox.Checked = true;
            }
            else if (Properties.Settings.Default.MINIMIZE_HIDE == false)
            {
                hideMinimizeBox.Checked = false;
            }
            if (Properties.Settings.Default.TOPMOST == true)
            {
                topMostBox.Checked = true;
            }
            else if (Properties.Settings.Default.TOPMOST == false)
            {
                topMostBox.Checked = false;
            }
            checkTopMost();
            if (Properties.Settings.Default.INJECTION_TYPE == "timeout")
            {
                injectionTimeoutBox.Checked = true;
            }
            else if (Properties.Settings.Default.INJECTION_TYPE == "manual")
            {
                injectionManualBox.Checked = true;
            }
            else if (Properties.Settings.Default.INJECTION_TYPE == "always")
            {
                injectionAlwaysBox.Checked = true;
            }

            injectionTimeBox.Text = Properties.Settings.Default.TIMEOUT_VALUE.ToString();
            reporter.writeToLog(Properties.Settings.Default.WIN32_FOLDER, "(loadSettings) All Settings Loaded.");
        }

        public void loadChangelog()
        {
            if (Properties.Settings.Default.OFFLINE_MODE == false)
            {
                string message = changeLogMessage(updaterURL + Properties.Settings.Default.BM_VERSION);

                if (Properties.Settings.Default.JUST_UPDATED == true)
                {
                    reporter.writeToLog(logPath, "(loadChangelog) Downloading latest Changelog.");
                    changelogBox.Visible = true;
                    changelogBtn.Location = new Point(12, 72);
                    changelogBox.Text = message;
                    Properties.Settings.Default.JUST_UPDATED = false;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    if (Properties.Settings.Default.CHANGELOG_COLLAPSED == true)
                    {
                        changelogBox.Visible = false;
                        changelogBtn.Location = new Point(12, 295);
                    }
                    else
                    {
                        changelogBox.Visible = true;
                        changelogBtn.Location = new Point(12, 74);
                    }
                    changelogBox.Text = message;
                }
            }
            else
            {
                reporter.writeToLog(logPath, "(loadChangelog) Offline Mode Enabled, Cannot Download Changelog.");
                changelogBox.Visible = false;
                changelogBtn.Location = new Point(12, 292);
                changelogBox.Text = "Offline Mode Enabled";
            }
        }
        #endregion

        #region "Home Events"
        private void injectBtn_Click(object sender, EventArgs e)
        {
            reporter.writeToLog(logPath, "(injectBtn) Manually Injecting DLL.");
            injectInstance();
        }

        private void changelogBtn_Click(object sender, EventArgs e)
        {
            if (changelogBtn.Top == 74)
            {
                changelogBox.Visible = false;
                changelogBtn.Location = new Point(12, 294);
                Properties.Settings.Default.CHANGELOG_COLLAPSED = true;
            }
            else
            {
                changelogBox.Visible = true;
                changelogBtn.Location = new Point(12, 74);
                Properties.Settings.Default.CHANGELOG_COLLAPSED = false;
            }

            Properties.Settings.Default.Save();
        }
        #endregion

        #region "Plugin Events"
        public void loadPlugins()
        {
            pluginsList.Clear();
            if (Directory.Exists(Properties.Settings.Default.WIN32_FOLDER + "\\bakkesmod\\plugins"))
            {
                try
                {
                    string[] Files = Directory.GetFiles(Properties.Settings.Default.WIN32_FOLDER + "\\bakkesmod\\plugins");
                    foreach (string File in Files)
                    {
                        reporter.writeToLog(Properties.Settings.Default.WIN32_FOLDER, "(loadPlugins) All Plugins Loaded.");
                        pluginsList.Items.Add(Path.GetFileName(File));
                    }
                }
                catch (Exception)
                {
                    reporter.writeToLog(Properties.Settings.Default.WIN32_FOLDER, "(loadPlugins) Failed to Load Plugins.");
                }
            }
            else
            {
                reporter.writeToLog(Properties.Settings.Default.WIN32_FOLDER, "(loadPlugins) Could not find plugins folder.");
            }
        }

        private void uninstallpluginsBtn_Click(object sender, EventArgs e)
        {
            if (pluginsList.SelectedItems.Count > 0)
            {
                DialogResult Result = MessageBox.Show("Are you sure you want to uninstall this plugin? This action can not be undone.", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (Result == DialogResult.Yes)
                {
                    string Dll = Properties.Settings.Default.WIN32_FOLDER + "bakkesmod\\plugins\\" + pluginsList.SelectedItems[0].Text;
                    string Set = pluginsList.SelectedItems[0].Text.Replace(".dll", ".set");
                    string Settings = Properties.Settings.Default.WIN32_FOLDER + "bakkesmod\\plugins\\settings\\" + Set;

                    try
                    {
                        if (File.Exists(Dll))
                        {
                            reporter.writeToLog(logPath, "(uninstallpluginsBtn) " + Dll + " has successfully been deleted.");
                            File.Delete(Dll);
                            reporter.writeToLog(logPath, "(uninstallpluginsBtn) " + Set + " has successfully been deleted.");
                            File.Delete(Settings);
                        }
                    }
                    catch (Exception Ex)
                    {
                        reporter.writeToLog(logPath, "(uninstallpluginsBtn) " + Ex.ToString());
                    }

                    reporter.writeToLog(logPath, "(uninstallpluginsBtn) Reloading Plugins.");
                    loadPlugins();
                }
            }
        }

        private void refreshpluginsBtn_Click(object sender, EventArgs e)
        {
            loadPlugins();
        }

        private void downloadpluginsBtn_Click(object sender, EventArgs e)
        {
            Process.Start("https://bakkesplugins.com/");
        }
        #endregion

        #region "Setting Events"
        public void resetSettings()
        {
            Properties.Settings.Default.AUTO_UPDATE = true;
            Properties.Settings.Default.SAFE_MODE = true;
            Properties.Settings.Default.OFFLINE_MODE = false;
            Properties.Settings.Default.DISABLE_WARNINGS = false;
            Properties.Settings.Default.STARTUP_RUN = false;
            Properties.Settings.Default.STARTUP_MINIMIZE = false;
            Properties.Settings.Default.MINIMIZE_HIDE = false;
            Properties.Settings.Default.TOPMOST = false;
            Properties.Settings.Default.INJECTION_TYPE = "timeout";
            Properties.Settings.Default.TIMEOUT_VALUE = 2500;
            reporter.writeToLog(Properties.Settings.Default.WIN32_FOLDER, "(resetSettings) Reset settings to default.");
            Properties.Settings.Default.Save();
            loadSettings();
        }

        public void setInjectionMethod()
        {
            if (injectionTimeoutBox.Checked == true)
            {
                Properties.Settings.Default.INJECTION_TYPE = "timeout";
            }
            else if (injectionManualBox.Checked == true)
            {
                Properties.Settings.Default.INJECTION_TYPE = "manual";
            }
            else if (injectionAlwaysBox.Checked == true)
            {
                Properties.Settings.Default.INJECTION_TYPE = "always";
            }

            processTmr.Start();
            Properties.Settings.Default.Save();
        }

        private void autoUpdateBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AUTO_UPDATE = autoUpdateBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void safeModeBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SAFE_MODE = safeModeBox.Checked;
            Properties.Settings.Default.Save();
            checkSafeMode();
        }

        private void warningsBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DISABLE_WARNINGS = warningsBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void startupRunBox_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey RK = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (startupRunBox.Checked == true)
            {
                RK.SetValue("BakkesModInjectorCs", Application.ExecutablePath);
            }
            if (startupRunBox.Checked == false)
            {
                RK.DeleteValue("BakkesModInjectorCs", false);
            }
            Properties.Settings.Default.STARTUP_RUN = startupRunBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void startupMinimizeBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.STARTUP_MINIMIZE = startupMinimizeBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void hideMinimizeBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MINIMIZE_HIDE = hideMinimizeBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void topMostBox_CheckedChanged(object sender, EventArgs e)
        {
            if (topMostBox.Checked == true)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
            Properties.Settings.Default.TOPMOST = topMostBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void injectionTimeoutBox_CheckedChanged(object sender, EventArgs e)
        {
            setInjectionMethod();
        }

        private void injectionManualBox_CheckedChanged(object sender, EventArgs e)
        {
            setInjectionMethod();
        }

        private void injectionAlwaysBox_CheckedChanged(object sender, EventArgs e)
        {
            setInjectionMethod();
        }

        private void injectionTimeBox_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TIMEOUT_VALUE = Convert.ToInt32(injectionTimeBox.Value);
            Properties.Settings.Default.Save();
        }

        private void manualUpdateBtn_Click(object sender, EventArgs e)
        {
            checkForUpdates(true);
        }

        private void openFolderBtn_Click(object sender, EventArgs e)
        {
            string directory = Properties.Settings.Default.WIN32_FOLDER + "\\bakkesmod";

            if (!Directory.Exists(directory))
            {
                reporter.writeToLog(logPath, "(openFolderBtn) Directory Not Found.");
                checkInstall();
            }
            else
            {
                Process.Start(directory);
                reporter.writeToLog(logPath, "(openFolderBtn) Opened: " + directory);
            }
        }

        private void exportLogsBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                if (!Directory.Exists(Path.GetTempPath() + "BakkesModInjectorCs"))
                {
                    reporter.writeToLog(logPath, "(exportLogsBtn) Creating temp folder.");
                    Directory.CreateDirectory(Path.GetTempPath() + "BakkesModInjectorCs");
                    reporter.writeToLog(logPath, "(exportLogsBtn) Folder Location: " + Path.GetTempPath());
                }

                List<string> filesToExport = new List<string>();
                string[] files = Directory.GetFiles(Properties.Settings.Default.WIN32_FOLDER);

                foreach (string file in files)
                {
                    if (file.IndexOf(".mdump") > 0 || file.IndexOf(".mdmp") > 0)
                    {
                        reporter.writeToLog(logPath, "(exportLogsBtn) Adding minidump files.");
                        filesToExport.Add(file);
                    }
                }

                reporter.writeToLog(logPath, "(exportLogsBtn) Adding log files.");
                string myDoucments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (File.Exists(Properties.Settings.Default.WIN32_FOLDER + "bakkesmod\\bakkesmod.log"))
                {
                    filesToExport.Add(Properties.Settings.Default.WIN32_FOLDER + "bakkesmod\\bakkesmod.log");
                }
                if (File.Exists(logPath))
                {
                    filesToExport.Add(logPath);
                }
                if (File.Exists(myDoucments + "\\My Games\\Rocket League\\TAGame\\Logs\\launch.log"))
                {
                    filesToExport.Add(myDoucments + "\\My Games\\Rocket League\\TAGame\\Logs\\launch.log");
                }

                string tempName = "crash_logs_" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                string tempDirectory = Path.GetTempPath() + "BakkesModInjectorCs\\" + tempName;
                Directory.CreateDirectory(tempDirectory);

                filesToExport.All((string x) =>
                {
                    FileInfo fi = new FileInfo(x);
                    File.Copy(x, tempDirectory + "\\" + fi.Name);
                    return true;
                });

                reporter.writeToLog(logPath, "(exportLogsBtn) Creating zip file.");
                ZipFile.CreateFromDirectory(tempDirectory, tempDirectory + ".zip");
                File.Move(tempDirectory + ".zip", fbd.SelectedPath + "\\" + tempName + ".zip");
                reporter.writeToLog(logPath, "(exportLogsBtn) Deleting temp folder.");
                Directory.Delete(tempDirectory, true);
                MessageBox.Show("Successfully exported crash logs to: " + fbd.SelectedPath.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void resetSettingsBtn_Click(object sender, EventArgs e)
        {
            resetSettings();
        }

        private void reinstallBtn_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("This will fully remove all BakkesMod files, are you sure you want to continue?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes)
            {
                string Path = Properties.Settings.Default.WIN32_FOLDER + "bakkesmod";
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, true);
                    installBM();
                }
            }
        }

        private void uninstallBtn_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Are you sure you want to uninstall BakkesMod? This will remove all BakkesMod & BakkesModInjectorCs files and registry keys.", "BakkesMod Uninstaller", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes)
            {
                GetDirectory();
            }
        }
        #endregion

        #region "Tab Events"
        public void activateTab(Label selectedBtn, PictureBox selectedImg, TabPage selectedTab)
        {
            homeBtn.BackColor = Color.Transparent;
            pluginsBtn.BackColor = Color.Transparent;
            settingsBtn.BackColor = Color.Transparent;
            aboutBtn.BackColor = Color.Transparent;
            homeImg.BackColor = Color.Transparent;
            pluginsImg.BackColor = Color.Transparent;
            settingsImg.BackColor = Color.Transparent;
            aboutImg.BackColor = Color.Transparent;

            selectedBtn.BackColor = Color.FromArgb(255, 255, 255, 255);
            selectedImg.BackColor = Color.FromArgb(255, 255, 255, 255);
            controlTabs.SelectedTab = selectedTab;
        }

        private void homeBtn_Click(object sender, EventArgs e)
        {
            activateTab(homeBtn, homeImg, homeTab);
        }

        private void homeImg_Click(object sender, EventArgs e)
        {
            activateTab(homeBtn, homeImg, homeTab);
        }

        private void pluginsBtn_Click(object sender, EventArgs e)
        {
            activateTab(pluginsBtn, pluginsImg, pluginsTab);
        }

        private void pluginsImg_Click(object sender, EventArgs e)
        {
            activateTab(pluginsBtn, pluginsImg, pluginsTab);
        }

        private void settingsBtn_Click(object sender, EventArgs e)
        {
            activateTab(settingsBtn, settingsImg, settingsTab);
        }

        private void settingsImg_Click(object sender, EventArgs e)
        {
            activateTab(settingsBtn, settingsImg, settingsTab);
        }

        private void aboutBtn_Click(object sender, EventArgs e)
        {
            activateTab(aboutBtn, aboutImg, aboutTab);
        }

        private void aboutImg_Click(object sender, EventArgs e)
        {
            activateTab(aboutBtn, aboutImg, aboutTab);
        }
        #endregion

        #region "Injector & Timers"
        public Boolean isProcessRunning()
        {
            Process[] rocketLeague = Process.GetProcessesByName("RocketLeague");

            if (rocketLeague.Length == 0)
            {
                return false;
            }

            return true;
        }

        public Boolean openProcess()
        {
            try
            {
                Process rocketLeague = new Process();
                rocketLeague.StartInfo.FileName = "steam.exe";
                rocketLeague.StartInfo.Arguments = "-applaunch 252950";
                rocketLeague.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Boolean closeProcess()
        {
            try
            {
                Process[] rocketLeague = Process.GetProcessesByName("RocketLeague");

                rocketLeague[0].Kill();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void processTmr_Tick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.INJECTION_TYPE == "always")
            {
                statusLbl.Text = "Always Injected enabled.";
                processTmr.Stop();
            }
            else
            {
                //to do: find correct placement for always injected type
                if (isProcessRunning() == false)
                {
                    rocketLeagueLbl.Text = "Rocket League is not running.";
                    statusLbl.Text = "Uninjected, waiting for user to start Rocket League.";
                    isInjected = false;
                    injectBtn.Visible = false;
                }
                else
                {
                    rocketLeagueLbl.Text = "Rocket League is running.";
                    if (isInjected == false)
                    {
                        injectionTmr.Interval = Properties.Settings.Default.TIMEOUT_VALUE;

                        if (Properties.Settings.Default.INJECTION_TYPE == "manual")
                        {
                            injectionTmr.Start();
                        }
                        else if (Properties.Settings.Default.INJECTION_TYPE == "timeout")
                        {
                            statusLbl.Text = "Process found, attempting injection.";
                            injectionTmr.Start();
                        }
                    }
                }
            }
        }

        private void injectionTmr_Tick(object sender, EventArgs e)
        {
            checkSafeMode();
            if (isInjected == false)
            {
                if (Properties.Settings.Default.INJECTION_TYPE == "timeout")
                {
                    injectionTmr.Stop();
                    injectBtn.Visible = false;
                    injectInstance();
                }
                else if (Properties.Settings.Default.INJECTION_TYPE == "manual")
                {
                    injectionTmr.Stop();
                    statusLbl.Text = "Process found, waiting for user to manually inject.";
                    injectBtn.Visible = true;
                }
            }
        }

        void injectInstance()
        {
            reporter.writeToLog(logPath, "(injectInstance) Attempting injection.");
            Feedback Result = injector.instance.load("RocketLeague", Properties.Settings.Default.WIN32_FOLDER + "\\BakkesMod\\bakkesmod.dll");
            switch (Result)
            {
                case Feedback.FILE_NOT_FOUND:
                    reporter.writeToLog(logPath, "(injectInstance) Injection failed, DLL not found.");
                    statusLbl.Text = "Uninjected, could not locate DLL.";
                    isInjected = false;
                    break;
                case Feedback.PROCESS_NOT_FOUND:
                    reporter.writeToLog(logPath, "(injectInstance) Injection failed, process not found.");
                    isInjected = false;
                    statusLbl.Text = "Uninjected, waiting for user to start Rocket League.";
                    break;
                case Feedback.FAIL:
                    reporter.writeToLog(logPath, "(injectInstance) Injection failed, no reason provided.");
                    statusLbl.Text = "Injection failed, possible corruption?";
                    isInjected = false;
                    break;
                case Feedback.NO_ENTRY_POINT:
                    reporter.writeToLog(logPath, "(injectInstance) Injection failed, no entry point in process.");
                    statusLbl.Text = "Injection failed, no entry point for process.";
                    isInjected = false;
                    break;
                case Feedback.MEMORY_SPACE_FAIL:
                    reporter.writeToLog(logPath, "(injectInstance) Injection failed, not enough memory available.");
                    statusLbl.Text = "Injection failed, not enough memory space.";
                    isInjected = false;
                    break;
                case Feedback.MEMORY_WRITE_FAIL:
                    reporter.writeToLog(logPath, "(injectInstance) Injection Failed.");
                    statusLbl.Text = "Injection failed, could not write to memory.";
                    isInjected = false;
                    break;
                case Feedback.SUCCESS:
                    reporter.writeToLog(logPath, "(injectInstance) Successfully Injected.");
                    statusLbl.Text = "Successfully injected, changes applied in-game.";
                    isInjected = true;
                    break;
            }
        }
        #endregion

        #region "Installers & Updaters"
        public string httpDownloader(String url, String pattern, String contents)
        {
            string match = "";
            string download = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader SR = new StreamReader(response.GetResponseStream());
            download = SR.ReadToEnd();
            SR.Close();

            if (download.Contains(contents))
            {
                match = Regex.Match(download, pattern, RegexOptions.IgnoreCase | RegexOptions.RightToLeft).Groups[1].Value.Replace("\"", "");
            }

            return match;
        }

        public Boolean serverOnline()
        {
            var ping = new System.Net.NetworkInformation.Ping();
            var result = ping.Send("149.210.150.107");

            if (result.Status != System.Net.NetworkInformation.IPStatus.Success)
            {
                return false;
            }

            return true;
        }

        public Boolean updateRequired(string url)
        {
            string download = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            download = sr.ReadToEnd();
            sr.Close();

            if (download.Contains("\"update_required\": true"))
            {
                return true;
            }

            return false;
        }

        public string downloadURL(string url)
        {
            string download = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            download = sr.ReadToEnd();
            sr.Close();

            download = download.Substring(download.IndexOf("\"download_url\": \"") + 17);
            download = download.Substring(0, download.IndexOf("\""));

            return download;
        }

        public Boolean latestBuild(string url)
        {
            string download = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            download = sr.ReadToEnd();
            sr.Close();

            if (download.Contains(Properties.Settings.Default.RL_VERSION))
            {
                return true;
            }

            return false;
        }

        public string changeLogMessage(string url)
        {
            string download = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            download = sr.ReadToEnd();
            sr.Close();

            if (download.Contains("message"))
            {
                download = download.Substring(download.IndexOf("\"message\": \"") + 12);
                download = download.Substring(0, download.IndexOf("\""));
            }
            else
            {
                download = "No changelog provided for the most recent update.";
            }

            return download;
        }

        public void checkForUpdater()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe"))
            {
                reporter.writeToLog(logPath, "(checkForUpdater) AutoUpdaterCs.exe has been located.");
                try
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe");
                    reporter.writeToLog(logPath, "(checkForUpdater) AutoUpdaterCs.exe has successfully been deleted.");
                }
                catch (Exception Ex)
                {
                    reporter.writeToLog(logPath, "(checkForUpdater) " + Ex.ToString());
                }
            }
            else
            {
                reporter.writeToLog(logPath, "(checkForUpdater) AutoUpdaterCs.exe does not exist.");
            }
        }

        public void checkInstall()
        {
            string Path = (Properties.Settings.Default.WIN32_FOLDER);

            if (!Directory.Exists(Path))
            {
                reporter.writeToLog(logPath, "(checkInstall) Failed to locate the Win32 folder.");
                getFolderPathOverride("Error: Could not find Win32 folder, please manually select where your RocketLeague.exe is located.");
            }
            else if (Directory.Exists(Path))
            {
                reporter.writeToLog(logPath, "(checkInstall) Successfully located the Win32 folder.");
                if (!Directory.Exists(Path + "\\bakkesmod"))
                {
                    reporter.writeToLog(logPath, "(checkInstall) Failed to locate the BakkesMod folder.");
                    DialogResult DialogResult = MessageBox.Show("Error: Could not find the BakkesMod folder, would you like to install it?", "BakkesMod", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (DialogResult == DialogResult.Yes)
                    {
                        installBM();
                    }
                }
                else if (Directory.Exists(Path + "\\bakkesmod"))
                {
                    reporter.writeToLog(logPath, "(CheckInstall) Successfully located the BakkesMod folder.");
                    getVersions();
                }
            }
        }

        public void checkForUpdates(Boolean displayResult)
        {
            if (Properties.Settings.Default.OFFLINE_MODE == false)
            {
                getVersions();
                string injectorVersion = httpDownloader("https://pastebin.com/raw/BVMKZ4TZ", "(\"([^ \"]|\"\")*\")", "INJECTOR_VERSION");
                reporter.writeToLog(logPath, "(checkForUpdates) Checking Injector Version.");
                reporter.writeToLog(logPath, "(checkForUpdates) Current Injector Version: " + Properties.Settings.Default.INJECTOR_VERSION);
                reporter.writeToLog(logPath, "(checkForUpdates) Latest Injector Version: " + injectorVersion);

                if (Properties.Settings.Default.INJECTOR_VERSION == injectorVersion)
                {
                    reporter.writeToLog(logPath, "(checkForUpdates) Version Match, No Injector Update Found.");
                    reporter.writeToLog(logPath, "(checkForUpdates) Checking BakkesMod Version.");
                    reporter.writeToLog(logPath, "(checkForUpdates) Current BakkesMod Version: " + Properties.Settings.Default.BM_VERSION);
                    if (updateRequired(updaterURL + Properties.Settings.Default.BM_VERSION) == false)
                    {
                        reporter.writeToLog(logPath, "(checkForUpdates) No BakkesMod Update Detected. ");
                        if (displayResult == true)
                        {
                            MessageBox.Show("No mod or injector updates were detected.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        reporter.writeToLog(logPath, "(checkForUpdates) BakkesMod Update Found. ");
                        DialogResult result = MessageBox.Show("A new version of BakkesMod was detected, would you like to download it?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (result == DialogResult.Yes)
                        {
                            if (isProcessRunning() == true)
                            {
                                DialogResult processResult = MessageBox.Show("Rocket League needs to be closed in order to update, would you like to close it now?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (processResult == DialogResult.Yes)
                                {
                                    if (closeProcess() == true)
                                    {
                                        installUpdate();
                                    }
                                }
                            }
                            else
                            {
                                installUpdate();
                            }
                        }
                    }
                }
                else
                {
                    reporter.writeToLog(logPath, "(checkForUpdates) Version Mismatch, Injector Update Found.");
                    DialogResult result = MessageBox.Show("A new version of BakkesModInjectorCs was detected, would you like to download it?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (result == DialogResult.Yes)
                    {
                        if (isProcessRunning() == true)
                        {
                            DialogResult processResult = MessageBox.Show("Rocket League needs to be closed in order to update, would you like to close it now?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (processResult == DialogResult.Yes)
                            {
                                if (closeProcess() == true)
                                {
                                    installInjector();
                                }
                            }
                        }
                        else
                        {
                            installInjector();
                        }
                    }
                }
            }
            else
            {
                reporter.writeToLog(logPath, "(checkForUpdates) Offline Mode Enabled, Cannot Check for Updates.");
                MessageBox.Show("Offline Mode is enabled, cannot check for updates at this time.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void installBM()
        {
            string path = Properties.Settings.Default.WIN32_FOLDER;
            string url = downloadURL(updaterURL + Properties.Settings.Default.BM_VERSION);

            if (!Directory.Exists(path + "\\bakkesmod"))
            {
                reporter.writeToLog(logPath, "(installBM) Creating BakkesMod folder.");
                Directory.CreateDirectory(path + "\\bakkesmod");
            }

            using (WebClient client = new WebClient())
            {
                try
                {
                    reporter.writeToLog(logPath, "(installBM) Downloading BakkesMod Archive.");
                    client.DownloadFile(url, AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
                }
                catch (Exception ex)
                {
                    reporter.writeToLog(logPath, "(installBM) " + ex.ToString());
                }
            }

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip"))
            {
                try
                {
                    reporter.writeToLog(logPath, "(installBM) Extracting Archive.");
                    ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip", path + "\\bakkesmod\\");
                    reporter.writeToLog(logPath, "(installBM) Deleting Archive.");
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
                }
                catch (Exception ex)
                {
                    reporter.writeToLog(logPath, "(installBM) " + ex.ToString());
                    MessageBox.Show(ex.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                reporter.writeToLog(logPath, "(installBM) No Archive Found, Cannot Extract File");
            }
        }

        public void installInjector()
        {
            try
            {
                reporter.writeToLog(logPath, "(installInjector) Writing AutoUpdaterCs.");
                byte[] BMU = Properties.Resources.AutoUpdaterCs;
                File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe", BMU);
                reporter.writeToLog(logPath, "(installInjector) Opening AutoUpdaterCs.");
                Process P = new Process();
                P.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe";
                P.Start();
                this.Close();
            }
            catch (Exception ex)
            {
                reporter.writeToLog(logPath, "(installInjector) " + ex.ToString());
            }
        }

        public void installUpdate()
        {
            string url = downloadURL(updaterURL + Properties.Settings.Default.BM_VERSION);

            using (WebClient client = new WebClient())
            {
                try
                {
                    reporter.writeToLog(logPath, "(installUpdate) Downloading Archive.");
                    client.DownloadFile(url, AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
                }
                catch (Exception ex)
                {
                    reporter.writeToLog(logPath, "(installUpdate) " + ex.ToString());
                    MessageBox.Show(ex.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            statusLbl.Text = "Update found, installing updates...";
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip"))
            {
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip"))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string destination = Path.GetFullPath(Path.Combine(Properties.Settings.Default.WIN32_FOLDER + "\\bakkesmod\\", entry.FullName));

                            if (entry.Name == "")
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                                continue;
                            }
                            reporter.writeToLog(logPath, "(installUpdate) Checking Existing Installed Files.");
                            if (destination.ToLower().EndsWith(".cfg") || destination.ToLower().EndsWith(".json"))
                            {
                                if (File.Exists(destination))
                                    continue;
                            }
                            reporter.writeToLog(logPath, "(installUpdate) Extracting Archive.");
                            entry.ExtractToFile(destination, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    reporter.writeToLog(logPath, "(installUpdate) " + ex.ToString());
                    MessageBox.Show(ex.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            try
            {
                reporter.writeToLog(logPath, "(installUpdate) Removing Archive.");
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
            }
            catch
            {
                reporter.writeToLog(logPath, "(installUpdate) Failed to Remove Archive.");
                MessageBox.Show("Failed to remove bakkesmod.zip, try running as administrator if you haven't arlready.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            getVersions();
            loadChangelog();
            statusLbl.Text = "Uninjected, waiting for user to start Rocket League.";
        }
        #endregion

        #region "Uninstaller"
        Boolean DirectoryError = false;
        Boolean RegistryError = false;
        Boolean LogError = false;
        Boolean PickDirectory = false;
        string DirectoryPath;

        public void GetDirectory()
        {
            string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string LogDir = MyDocuments + "\\My Games\\Rocket League\\TAGame\\Logs\\";
            string LogFile = LogDir + "launch.log";
            if (PickDirectory == true)
            {
                OpenFileDialog OFD = new OpenFileDialog
                {
                    Title = "Select RocketLeague.exe",
                    Filter = "EXE Files (*.exe)|*.exe"
                };

                if (OFD.ShowDialog() == DialogResult.OK)
                {
                    string FilePath = OFD.FileName;
                    FilePath = FilePath.Replace("RocketLeague.exe", "");
                    DirectoryPath = FilePath;
                }
            }
            else
            {
                if (File.Exists(LogFile))
                {
                    string Line;
                    using (FileStream Stream = File.Open(LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        StreamReader File = new StreamReader(Stream);
                        while ((Line = File.ReadLine()) != null)
                        {
                            if (Line.Contains("Init: Base directory: "))
                            {
                                Line = Line.Replace("Init: Base directory: ", "");
                                DirectoryPath = Line;
                                break;
                            }
                        }
                    }
                }
            }
            RemoveDirectory();
        }

        public void RemoveDirectory()
        {
            string FolderPath = DirectoryPath + "bakkesmod";
            if (!Directory.Exists(FolderPath))
            {
                MessageBox.Show("Could not find the directory, please manually point to where your RocketLeague.exe is located.", "BakkesMod Uninstaller", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PickDirectory = true;
                GetDirectory();
            }
            else
            {
                try
                {
                    Directory.Delete(FolderPath, true);
                }
                catch (Exception Ex)
                {
                    DirectoryError = true;
                    reporter.writeToLog(logPath, "(RemoveDirectory) " + Ex.ToString());
                }
            }
            RemoveLog();
        }

        public void RemoveLog()
        {
            string BakkesLog = Path.GetTempPath() + "injectorlog.log";
            string BranksLog = Path.GetTempPath() + "BakkesModInjectorCs.log";

            if (File.Exists(BakkesLog))
            {
                try
                {
                    File.Delete(BakkesLog);
                }
                catch (Exception Ex)
                {
                    LogError = true;
                    reporter.writeToLog(logPath, "(removeLog) " + Ex.ToString());
                }
            }
            if (File.Exists(BranksLog))
            {
                try
                {
                    File.Delete(BranksLog);
                }
                catch (Exception Ex)
                {
                    reporter.writeToLog(logPath, "(removeLog) " + Ex.ToString());
                }
            }
            RemoveRegistry();
        }

        public void RemoveRegistry()
        {
            RegistryKey Key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            try
            {
                Key.DeleteValue("BakkesModInjectorCs", false);
            }
            catch (Exception Ex)
            {
                RegistryError = true;
                reporter.writeToLog(logPath, "(removeRegistry) " + Ex.ToString());
            }
            ConfirmUninstall();
        }

        public void ConfirmUninstall()
        {
            if (DirectoryError == true || RegistryError == true || LogError == true)
            {
                if (DirectoryError == true)
                {
                    DialogResult DirectoryResult = MessageBox.Show("There was an error trying to remove the directory, would you like to try again?", "BakkesMod Uninstaller", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (DirectoryResult == DialogResult.Yes)
                    {
                        RemoveDirectory();
                    }
                }
                else if (RegistryError == true)
                {
                    DialogResult RegistryResult = MessageBox.Show("There was an error trying to remove the startup registry, would you like to try again?", "BakkesMod Uninstaller", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (RegistryResult == DialogResult.Yes)
                    {
                        RemoveRegistry();
                    }
                }
                else if (LogError == true)
                {
                    DialogResult RegistryResult = MessageBox.Show("There was an error trying to remove the log files, would you like to try again?", "BakkesMod Uninstaller", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (RegistryResult == DialogResult.Yes)
                    {
                        RemoveLog();
                    }
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("BakkesMod has successfully been uninstalled.", "BakkesMod Uninstaller", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }

        #endregion

    }
}