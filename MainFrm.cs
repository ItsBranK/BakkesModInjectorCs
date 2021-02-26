using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace BakkesModInjectorCs
{
/*
    TO DO:
    - Figure out why takes forever to load when first opening the exe.
    - Rename stuff.

    - Proper commenting for everything explaining what it does, I was told you're suppose to do that for github projects but I'm lazy
 */

    public partial class MainFrm : Form
    {
        bool IsInjected = false;
        bool OfflineMode = false;

        #region "Form Events"
        public MainFrm()
        {
            InitializeComponent();
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            this.Text = Properties.Settings.Default.WINDOW_TTILE;
            CheckForUpdater();
            CheckForUninstaller();

            bool loadedDirs;
            Utils.LoadDirectories(out loadedDirs);

            if (loadedDirs)
            {
                LoadPlugins();
                SetVersionInfo();
                CheckServer();
                GetJsonSuccess();
                LoadChangelog();
                LoadSettings();
            }
            else
            {
                MessageBox.Show("Error: Failed to find needed directories, check the log for more info.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }
        }

        private void MainFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MainFrm_Resize(object sender, EventArgs e)
        {
            CheckHideMinimize();
        }

        private void OpenTrayBtn_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            TrayIcon.Visible = false;
        }

        private void ExitTrayBtn_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
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
        public bool GetJsonSuccess()
        {
            if (!OfflineMode)
            {
                bool succeeded;
                JsonObjects.GetJsonObjects(out succeeded);
                return succeeded;
            }
            else
            {
                return false;
            }
        }

        public void SetVersionInfo()
        {
            rlVersionLbl.Text = "Rocket League Version: " + Properties.Settings.Default.RL_VERSION;
            rlBuildLbl.Text = "Rocket League Build: " + Properties.Settings.Default.RL_BUILD;
            injectorVersionLbl.Text = "Injector Version: " + Properties.Settings.Default.INJECTOR_VERSION;
            bmVersionLbl.Text = "Mod Version: " + Properties.Settings.Default.BM_VERSION;
        }

        public void CheckRocketLeagueVersion()
        {
            if (!OfflineMode)
            {
                SetVersionInfo();

                Utils.Log(MethodBase.GetCurrentMethod(), "Checking Build ID.");
                Utils.Log(MethodBase.GetCurrentMethod(), "Current Build ID: " + Properties.Settings.Default.RL_BUILD);
                Utils.Log(MethodBase.GetCurrentMethod(), "Latest Build ID(s): " + JsonObjects.GetBuildIds());

                if (JsonObjects.IsUpdateRequired())
                {
                    if (Properties.Settings.Default.RL_BUILD == "FILE_BLANK" || Properties.Settings.Default.RL_BUILD == "FILE_NOT_FOUND")
                    {
                        Utils.Log(MethodBase.GetCurrentMethod(), "Corrupted appmanifest detected.");
                    }
                    else
                    {
                        Utils.Log(MethodBase.GetCurrentMethod(), "Build ID mismatch, activating Safe Mode.");

                        if (Properties.Settings.Default.SAFE_MODE)
                        {
                            ActivateSafeMode();
                        }
                    }
                }
                else
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), "Build ID match.");
                    ProcessTmr.Start();
                }
            }
        }

        public void ActivateSafeMode()
        {
            Utils.Log(MethodBase.GetCurrentMethod(), "Safe mode has been activated.");
            ProcessTmr.Stop();
            InjectionTmr.Stop();
            rocketLeagueLbl.Text = "Safe Mode Enabled.";
            statusLbl.Text = "Mod out of date, please wait for an update.";
        }

        public void ActivateOfflineMode()
        {
            Utils.Log(MethodBase.GetCurrentMethod(), "Offline mode has been activated.");
            this.Text = Properties.Settings.Default.WINDOW_TTILE + " (Offline Mode)";
            OfflineMode = true;
        }

        public void CheckServer()
        {
            if (!ServersOnline())
            {
                ActivateOfflineMode();
            }
        }

        public void CheckAutoUpdate()
        {
            if (!OfflineMode)
            {
                if (Properties.Settings.Default.AUTO_UPDATE)
                {
                    CheckForUpdates(false);
                }
            }
        }

        public void CheckSafeMode()
        {
            if (Properties.Settings.Default.SAFE_MODE)
            {
                GetJsonSuccess();
                CheckRocketLeagueVersion();
            }
            else
            {
                ProcessTmr.Start();
            }
        }

        public void CheckHideStartup()
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                if (Properties.Settings.Default.STARTUP_MINIMIZE)
                {
                    this.Hide();
                    TrayIcon.Visible = true;
                }
                else
                {
                    TrayIcon.Visible = false;
                }
            }
        }

        public void CheckHideMinimize()
        {
            if (Properties.Settings.Default.MINIMIZE_HIDE)
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Hide();
                    TrayIcon.Visible = true;
                }
            }
        }

        public void CheckTopMost()
        {
            if (Properties.Settings.Default.TOPMOST)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }

        public void LoadChangelog()
        {
            string message = "";

            if (!OfflineMode)
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Downloading latest changelog information.");
                message = JsonObjects.GetChangelog();
            }
            else
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Offline mode activated, cannot download changelog information.");
                message = "Offline mode has been activated, cannot download changelog information.";
            }

            if (Properties.Settings.Default.JUST_UPDATED)
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Downloading latest changelog information.");
                changelogBox.Visible = true;
                changelogBtnBackground.Location = new Point(12, 74);
                changelogBox.Text = message;
                Properties.Settings.Default.JUST_UPDATED = false;
                Properties.Settings.Default.Save();
            }
            else
            {
                if (Properties.Settings.Default.CHANGELOG_COLLAPSED)
                {
                    changelogBox.Visible = false;
                    changelogBtnBackground.Location = new Point(12, 294);
                }
                else
                {
                    changelogBox.Visible = true;
                    changelogBtnBackground.Location = new Point(12, 74);
                }

                changelogBox.Text = message;
            }
        }
        #endregion

        #region "Home Events"
        private void injectBtn_Click(object sender, EventArgs e)
        {
            Utils.Log(MethodBase.GetCurrentMethod(), "Manually injecting dll.");

            if (!CheckForUpdates(false) && !IsInjected)
            {
                InjectInstance();
            }
        }

        private void changelogBtn_Click(object sender, EventArgs e)
        {
            if (changelogBtnBackground.Top == 74)
            {
                changelogBackground.Visible = false;
                changelogBtn.Size = new Size(576, 25);
                changelogBtnBackground.Location = new Point(12, 294);
                Properties.Settings.Default.CHANGELOG_COLLAPSED = true;
            }
            else
            {
                changelogBackground.Visible = true;
                changelogBtn.Size = new Size(576, 26);
                changelogBtnBackground.Location = new Point(12, 74);
                Properties.Settings.Default.CHANGELOG_COLLAPSED = false;
            }

            Properties.Settings.Default.Save();
        }
        #endregion

        #region "Plugin Events"
        public void LoadPlugins()
        {
            pluginsList.Clear();

            if (Directory.Exists(Properties.Settings.Default.BAKKESMOD_FOLDER + "\\plugins"))
            {
                // Returns an array of all files in the plugins folder (returns their path)

                string[] files = Directory.GetFiles(Properties.Settings.Default.BAKKESMOD_FOLDER + "\\plugins");

                foreach (string f in files)
                {
                    // Adds each file in the array to the listbox

                    Utils.Log(MethodBase.GetCurrentMethod(), "Found plugin: " + Path.GetFileName(f));
                    pluginsList.Items.Add(Path.GetFileName(f));
                }
                Utils.Log(MethodBase.GetCurrentMethod(), "All plugins loaded.");
            }
            else
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Could not find plugins folder, no plugins loaded.");
            }
        }

        private void uninstallpluginsBtn_Click(object sender, EventArgs e)
        {
            if (pluginsList.SelectedItems.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to uninstall this plugin? This action can not be undone.", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
                if (dialogResult == DialogResult.Yes)
                {
                    string plugin = pluginsList.SelectedItems[0].Text;
                    string set = plugin.Replace(".dll", ".set");
                    string cfg = plugin.Replace(".dll", ".cfg");
                    string dllFile = Properties.Settings.Default.BAKKESMOD_FOLDER + "\\plugins\\" + pluginsList.SelectedItems[0].Text;
                    string setFile = Properties.Settings.Default.BAKKESMOD_FOLDER + "\\plugins\\settings\\" + set;
                    string cfgFile = Properties.Settings.Default.BAKKESMOD_FOLDER + "\\cfg\\" + cfg;
                    string dataFolder = Properties.Settings.Default.BAKKESMOD_FOLDER + "\\data\\" + plugin.Replace(".dll", "");

                    try
                    {
                        Utils.Log(MethodBase.GetCurrentMethod(), "Attempting to uninstall the selected plugin: \"" + plugin + "\"");

                        if (File.Exists(dllFile))
                        {
                            File.Delete(dllFile);
                            Utils.Log(MethodBase.GetCurrentMethod(), "\"" + plugin + "\" has successfully been deleted.");
                        }

                        if (File.Exists(setFile))
                        {
                            File.Delete(setFile);
                            Utils.Log(MethodBase.GetCurrentMethod(), "\"" + set + "\" has successfully been deleted.");
                        }

                        if (File.Exists(cfgFile))
                        {
                            File.Delete(cfgFile);
                            Utils.Log(MethodBase.GetCurrentMethod(), "\"" + cfg + "\" has successfully been deleted.");
                        }

                        if (Directory.Exists(dataFolder))
                        {
                            Directory.Delete(dataFolder, true);
                            Utils.Log(MethodBase.GetCurrentMethod(), "\"" + plugin + "\" data folder has successfully been deleted.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.Log(MethodBase.GetCurrentMethod(), ex.ToString());
                    }

                    Utils.Log(MethodBase.GetCurrentMethod(), "Reloading plugins.");
                    LoadPlugins();
                }
            }
        }

        private void refreshpluginsBtn_Click(object sender, EventArgs e)
        {
            LoadPlugins();
        }

        private void downloadpluginsBtn_Click(object sender, EventArgs e)
        {
            Process.Start("https://bakkesplugins.com/");
        }
        #endregion

        #region "Setting Events"
        // Sets the checkbox & textbox values to what the user last selected.
        public void LoadSettings()
        {
            if (Properties.Settings.Default.AUTO_UPDATE)
            {
                autoUpdateBox.Checked = true;
            }
            else
            {
                autoUpdateBox.Checked = false;
            }
            CheckAutoUpdate();

            if (Properties.Settings.Default.SAFE_MODE == true)
            {
                safeModeBox.Checked = true;
            }
            else if (Properties.Settings.Default.SAFE_MODE == false)
            {
                safeModeBox.Checked = false;
            }
            CheckSafeMode();

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
            CheckHideStartup();

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
            CheckTopMost();
            
            if (Properties.Settings.Default.INJECTION_TYPE == "timeout")
            {
                injectionTimeoutBox.Checked = true;
            }
            else if (Properties.Settings.Default.INJECTION_TYPE == "manual")
            {
                injectionManualBox.Checked = true;
            }

            injectionTimeBox.Text = Properties.Settings.Default.TIMEOUT_VALUE.ToString();
            Utils.Log(MethodBase.GetCurrentMethod(), "All settings have been loaded.");
        }

        public void ResetSettings()
        {
            Properties.Settings.Default.AUTO_UPDATE = true;
            Properties.Settings.Default.SAFE_MODE = true;
            Properties.Settings.Default.STARTUP_RUN = false;
            Properties.Settings.Default.STARTUP_MINIMIZE = false;
            Properties.Settings.Default.MINIMIZE_HIDE = false;
            Properties.Settings.Default.TOPMOST = false;
            Properties.Settings.Default.INJECTION_TYPE = "timeout";
            Properties.Settings.Default.TIMEOUT_VALUE = 2500;
            Utils.Log(MethodBase.GetCurrentMethod(), "Reset all settings to default.");
            Properties.Settings.Default.Save();
            LoadSettings();
        }

        public void SetInjectionMethod()
        {
            ProcessTmr.Stop();

            if (injectionTimeoutBox.Checked == true)
            {
                Properties.Settings.Default.INJECTION_TYPE = "timeout";
            }
            else if (injectionManualBox.Checked == true)
            {
                Properties.Settings.Default.INJECTION_TYPE = "manual";
            }

            Properties.Settings.Default.Save();
            ProcessTmr.Start();
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
            CheckSafeMode();
        }

        private void startupRunBox_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (startupRunBox.Checked == true)
            {
                key.SetValue("BakkesModInjectorCs", Application.ExecutablePath);
            }
            else
            {
                key.DeleteValue("BakkesModInjectorCs", false);
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
            if (topMostBox.Checked)
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
            SetInjectionMethod();
        }

        private void injectionManualBox_CheckedChanged(object sender, EventArgs e)
        {
            SetInjectionMethod();
        }

        private void injectionAlwaysBox_CheckedChanged(object sender, EventArgs e)
        {
            SetInjectionMethod();
        }

        private void injectionTimeBox_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TIMEOUT_VALUE = Convert.ToInt32(injectionTimeBox.Value);
            Properties.Settings.Default.Save();
        }

        private void manualUpdateBtn_Click(object sender, EventArgs e)
        {
            CheckForUpdates(true);
        }

        private void openFolderBtn_Click(object sender, EventArgs e)
        {
            string bakkesModFolder = Properties.Settings.Default.BAKKESMOD_FOLDER;

            if (Directory.Exists(bakkesModFolder))
            {
                Process.Start(bakkesModFolder);
                Utils.Log(MethodBase.GetCurrentMethod(), "Opened the path: " + bakkesModFolder);
            }
            else
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Could not find the BakkesMod folder, reloading directories.");
                Utils.LoadDirectories(out bool success);
            }
        }

        private void exportLogsBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string bakkesmodPath = Properties.Settings.Default.BAKKESMOD_FOLDER;
                string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string logsPath = myDocumentsPath + "\\My Games\\Rocket League\\TAGame\\Logs";

                if (!Directory.Exists(Path.GetTempPath() + "\\BakkesModInjectorCs"))
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), "Creating temp folder.");
                    Directory.CreateDirectory(Path.GetTempPath() + "BakkesModInjectorCs");
                    Utils.Log(MethodBase.GetCurrentMethod(), "Folder location: " + Path.GetTempPath());
                }

                List<string> filesToExport = new List<string>();

                if (File.Exists(bakkesmodPath + "\\bakkesmod\\bakkesmod.log"))
                {
                    filesToExport.Add(bakkesmodPath + "\\bakkesmod\\bakkesmod.log");
                }

                if (Directory.Exists(logsPath))
                {
                    string[] logs = Directory.GetFiles(logsPath);

                    foreach (string file in logs)
                    {
                        if (file.Contains(".mdump") || file.Contains(".mdmp") || file.Contains(".dmp") || file.Contains(".log"))
                        {
                            Utils.Log(MethodBase.GetCurrentMethod(), "Adding files from: " + logsPath);
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

                Utils.Log(MethodBase.GetCurrentMethod(), "Creating zip file.");
                ZipFile.CreateFromDirectory(tempDirectory, tempDirectory + ".zip");
                File.Move(tempDirectory + ".zip", fbd.SelectedPath + "\\" + tempName + ".zip");
                Utils.Log(MethodBase.GetCurrentMethod(), "Deleting temp folder.");
                Directory.Delete(tempDirectory, true);
                MessageBox.Show("Successfully exported crash logs to: " + fbd.SelectedPath.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void resetSettingsBtn_Click(object sender, EventArgs e)
        {
            ResetSettings();
        }

        private void windowTitleBtn_Click(object sender, EventArgs e)
        {
            NameFrm nf = new NameFrm();
            nf.Show();
            this.Hide();
        }

        private void reinstallBtn_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("This will fully remove all BakkesMod files and settings, are you sure you want to continue?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (dialogResult == DialogResult.Yes)
            {
                string Path = Properties.Settings.Default.BAKKESMOD_FOLDER;

                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, true);
                    InstallBakkesMod();
                }
            }
        }

        private void uninstallBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Writing BakkesModUninstaller.");
                byte[] fileBytes = Properties.Resources.BakkesModUninstaller;
                File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModUninstaller.exe", fileBytes);
                Utils.Log(MethodBase.GetCurrentMethod(), "Opening BakkesModUninstaller.");
                Process P = new Process();
                P.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModUninstaller.exe";
                P.Start();
                Utils.Log(MethodBase.GetCurrentMethod(), "Exiting own environment.");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Utils.Log(MethodBase.GetCurrentMethod(), ex.ToString());
            }
        }
        #endregion

        #region "Tab Events"
        public void SelectTab(Label selectedBtn, PictureBox selectedImg, TabPage selectedTab)
        {
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

        private void homeBtn_Click(object sender, EventArgs e)
        {
            SelectTab(homeBtn, homeImg, homeTab);
        }

        private void homeImg_Click(object sender, EventArgs e)
        {
            SelectTab(homeBtn, homeImg, homeTab);
        }

        private void pluginsBtn_Click(object sender, EventArgs e)
        {
            SelectTab(pluginsBtn, pluginsImg, pluginsTab);
        }

        private void pluginsImg_Click(object sender, EventArgs e)
        {
            SelectTab(pluginsBtn, pluginsImg, pluginsTab);
        }

        private void settingsBtn_Click(object sender, EventArgs e)
        {
            SelectTab(settingsBtn, settingsImg, settingsTab);
        }

        private void settingsImg_Click(object sender, EventArgs e)
        {
            SelectTab(settingsBtn, settingsImg, settingsTab);
        }

        private void aboutBtn_Click(object sender, EventArgs e)
        {
            SelectTab(aboutBtn, aboutImg, aboutTab);
        }

        private void aboutImg_Click(object sender, EventArgs e)
        {
            SelectTab(aboutBtn, aboutImg, aboutTab);
        }
        #endregion

        #region "Injector & Timers"
        public bool IsProcessRunning()
        {
            Process[] processList = Process.GetProcesses();

            foreach (Process process in processList)
            {
                if (process.ProcessName == "RocketLeague")
                {
                    return true;
                }
            }

            return false;
        }

        public bool CloseProcess()
        {
            try
            {
                Process[] processList = Process.GetProcesses();

                foreach (Process process in processList)
                {
                    if (process.ProcessName == "RocketLeague")
                    {
                        if (process.MainWindowTitle == "Rocket League (64-bit, DX11, Cooked)")
                        {
                            process.Kill();

                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        void InjectInstance()
        {
            Utils.Log(MethodBase.GetCurrentMethod(), "Attempting to inject.");

            string currentDll = "NULL";
            string firstDll = Properties.Settings.Default.BAKKESMOD_FOLDER + "\\bakkesmod.dll"; // This dll was used by Bakkes' x32 bit injector to inject the dll in "\dll\bakkesmod.dll"
            string secondDll = Properties.Settings.Default.BAKKESMOD_FOLDER + "\\dll\\bakkesmod.dll"; // The main BakkesMod dll

            if (File.Exists(firstDll) && !File.Exists(secondDll)) // If the dll in "\dll\bakkesmod.dll" doesn't exist that means Bakkes removed it so inject the first one.
            {
                currentDll = firstDll;
            }
            else if (File.Exists(secondDll)) // If it does exist, then inject that one instead.
            {
                currentDll = secondDll;
            }

            // Try to inject and report the result of the injection.

            if (currentDll != "NUll")
            {
                InjectorResult result = Injector.Instance.InjectDLL(currentDll);

                switch (result)
                {
                    case InjectorResult.FILE_NOT_FOUND:
                        Utils.Log(MethodBase.GetCurrentMethod(), "Uninjected, could not locate the necessary files.");
                        statusLbl.Text = "Uninjected, could not locate the necessary files.";
                        ProcessTmr.Stop();
                        IsInjected = false;
                        break;
                    case InjectorResult.PROCESS_NOT_FOUND:
                        Utils.Log(MethodBase.GetCurrentMethod(), "Injection failed, waiting for user to start Rocket League.");
                        statusLbl.Text = "Uninjected, waiting for user to start Rocket League.";
                        IsInjected = false;
                        break;
                    case InjectorResult.PROCESS_NOT_SUPPORTED:
                        Utils.Log(MethodBase.GetCurrentMethod(), "Injection failed, process architecture not supported.");
                        statusLbl.Text = "Injection failed, process architecture not supported.";
                        ProcessTmr.Stop();
                        IsInjected = false;
                        break;
                    case InjectorResult.PROCESS_HANDLE_NOT_FOUND:
                        Utils.Log(MethodBase.GetCurrentMethod(), "Injection failed, could not open the processes handle.");
                        statusLbl.Text = "Injection failed, could not open the processes handle.";
                        ProcessTmr.Stop();
                        IsInjected = false;
                        break;
                    case InjectorResult.LOADLIBRARY_NOT_FOUND:
                        Utils.Log(MethodBase.GetCurrentMethod(), "Injection failed, could not find the LoadLibraryA function in kernal32.dll.");
                        statusLbl.Text = "Injection failed, could not find necessary functions.";
                        ProcessTmr.Stop();
                        IsInjected = false;
                        break;
                    case InjectorResult.VIRTUAL_ALLOCATE_FAIL:
                        Utils.Log(MethodBase.GetCurrentMethod(), "Injection failed, could not allocate space in memory.");
                        statusLbl.Text = "Injection failed, could not allocate space in memory.";
                        ProcessTmr.Stop();
                        IsInjected = false;
                        break;
                    case InjectorResult.WRITE_MEMORY_FAIL:
                        Utils.Log(MethodBase.GetCurrentMethod(), "Injection failed, could not write to allocated space in memory.");
                        statusLbl.Text = "Injection failed, could not write to memory.";
                        ProcessTmr.Stop();
                        IsInjected = false;
                        break;
                    case InjectorResult.CREATE_THREAD_FAIL:
                        Utils.Log(MethodBase.GetCurrentMethod(), "Injection failed, could not create remote thread for dll.");
                        statusLbl.Text = "Injection failed, could not create remote thread.";
                        ProcessTmr.Stop();
                        IsInjected = false;
                        break;
                    case InjectorResult.SUCCESS:
                        Utils.Log(MethodBase.GetCurrentMethod(), "Successfully injected.");
                        statusLbl.Text = "Successfully injected, changes applied in-game.";
                        IsInjected = true;
                        break;
                }
            }
            else
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Uninjected, could not locate the necessary files.");
                statusLbl.Text = "Uninjected, could not locate the necessary files.";
                ProcessTmr.Stop();
                IsInjected = false;
            }
        }

        private void InjectionTmr_Tick(object sender, EventArgs e)
        {
            if (!CheckForUpdates(false) && !IsInjected)
            {
                if (Properties.Settings.Default.INJECTION_TYPE == "timeout")
                {
                    InjectInstance();
                }
                else if (Properties.Settings.Default.INJECTION_TYPE == "manual")
                {
                    statusLbl.Text = "Process found, waiting for user to manually inject.";
                    injectBtn.Visible = true;
                }

                InjectionTmr.Stop();
            }
        }

        // The process check timer will always be on (unless safe mode is activated).
        private void ProcessTmr_Tick(object sender, EventArgs e)
        {
            if (!IsProcessRunning())
            {
                rocketLeagueLbl.Text = "Rocket League is not running.";
                statusLbl.Text = "Uninjected, waiting for user to start Rocket League.";
                IsInjected = false;
                injectBtn.Visible = false;
            }
            else // If RL is running see what injection type the user has set and follow accordingly (if not already injected of course).
            {
                rocketLeagueLbl.Text = "Rocket League is running.";

                if (!IsInjected)
                {
                    InjectionTmr.Interval = Properties.Settings.Default.TIMEOUT_VALUE;

                    if (Properties.Settings.Default.INJECTION_TYPE == "manual")
                    {
                        InjectionTmr.Start();
                    }
                    else if (Properties.Settings.Default.INJECTION_TYPE == "timeout")
                    {
                        injectBtn.Visible = false;
                        statusLbl.Text = "Process found, attempting injection.";
                        InjectionTmr.Start();
                    }
                }
            }
        }
        #endregion

        #region "Installers & Updaters"
        public bool ServersOnline()
        {
            try
            {
                Ping ping = new Ping();
                PingReply bakkesReply = ping.Send("bakkesmod.com");
                PingReply pastebinReply = ping.Send("pastebin.com");

                // If either dont return successful then they aren't online

                if (bakkesReply.Status != IPStatus.Success || pastebinReply.Status != IPStatus.Success)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                // If it catches it could mean the user isn't connected to the internet.
                // I could use "NetworkInterface.GetIsNetworkAvailable();" to check this first but it's really slow, like takes almost 2 seconds per-check.

                return false;
            }
        }

        public void CheckForUpdater()
        {
            // If "AutoUpdaterCs" just updated we need to delete the exe out so there are no residual files.

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe"))
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "\"AutoUpdaterCs.exe\" has been located, attempting to delete.");
                try
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe");
                    Utils.Log(MethodBase.GetCurrentMethod(), "\"AutoUpdaterCs.exe\" has successfully been deleted.");
                }
                catch (Exception ex)
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), ex.ToString());
                }
            }
            else
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "\"AutoUpdaterCs.exe\" was not located.");
            }
        }

        // The uninstall and reinstall buttons both use "BakkesModUninstaller", we need to delete it if the user is reinstalling so there are no residual files.
        public void CheckForUninstaller()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModUninstaller.exe"))
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "\"BakkesModUninstaller.exe\" has been located.");
                try
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\BakkesModUninstaller.exe");
                    Utils.Log(MethodBase.GetCurrentMethod(), "\"BakkesModUninstaller.exe\" has successfully been deleted.");
                }
                catch (Exception ex)
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), ex.ToString());
                }
            }
            else
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "\"BakkesModUninstaller.exe\" was not located.");
            }
        }

        public bool CheckForUpdates(bool displayResult)
        {
            if (!OfflineMode)
            {
                // Grabs the current version and build info then redownloads the latest json objects.

                Utils.LoadDirectories(out bool succeeded);
                SetVersionInfo();

                if (GetJsonSuccess())
                {
                    string injectorVersion = JsonObjects.BranksConfig.InjectorVersion;
                    Utils.Log(MethodBase.GetCurrentMethod(), "Checking injector version.");
                    Utils.Log(MethodBase.GetCurrentMethod(), "Current injector version: " + Properties.Settings.Default.INJECTOR_VERSION);
                    Utils.Log(MethodBase.GetCurrentMethod(), "Latest injector version: " + injectorVersion);

                    // Compares the recently grabbed version info with the json ones, if any are different then prompt the user to update. Pretty self explanatory.

                    if (Properties.Settings.Default.INJECTOR_VERSION == injectorVersion)
                    {
                        Utils.Log(MethodBase.GetCurrentMethod(), "Version match, no injector update found.");
                        Utils.Log(MethodBase.GetCurrentMethod(), "Checking BakkesMod version.");
                        Utils.Log(MethodBase.GetCurrentMethod(), "Current BakkesMod version: " + Properties.Settings.Default.BM_VERSION);

                        if (!JsonObjects.IsUpdateRequired())
                        {
                            Utils.Log(MethodBase.GetCurrentMethod(), "No BakkesMod update was found.");

                            if (displayResult)
                            {
                                MessageBox.Show("No mod or injector updates were found.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            return false;
                        }
                        else
                        {
                            CheckRocketLeagueVersion();

                            Utils.Log(MethodBase.GetCurrentMethod(), "Version mismatch, a BakkesMod update was found.");
                            DialogResult dialogResult = MessageBox.Show("A new version of BakkesMod was found, would you like to install it now?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            
                            if (dialogResult == DialogResult.Yes)
                            {
                                if (IsProcessRunning())
                                {
                                    Utils.Log(MethodBase.GetCurrentMethod(), "Rocket League is running, asking user to close process.");
                                    DialogResult processResult = MessageBox.Show("Rocket League needs to be closed in order to update, would you like to close it now?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    
                                    if (processResult == DialogResult.Yes)
                                    {
                                        if (CloseProcess())
                                        {
                                            UpdateBakkesMod();
                                        }
                                    }
                                }
                                else
                                {
                                    UpdateBakkesMod();
                                }
                            }

                            return true;
                        }
                    }
                    else
                    {
                        Utils.Log(MethodBase.GetCurrentMethod(), "Version mismatch, injector update found.");
                        DialogResult dialogResult = MessageBox.Show("A new version of BakkesModInjectorCs was found, would you like to install it now?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        
                        if (dialogResult == DialogResult.Yes)
                        {
                            if (IsProcessRunning())
                            {
                                Utils.Log(MethodBase.GetCurrentMethod(), "Rocket League is running, asking user to close process.");
                                DialogResult processResult = MessageBox.Show("Rocket League needs to be closed in order to update, would you like to close it now?", "BakkesModInjectorCs", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                               
                                if (processResult == DialogResult.Yes)
                                {
                                    if (CloseProcess())
                                    {
                                        UpdateInjector();
                                    }
                                }
                            }
                            else
                            {
                                UpdateInjector();
                            }
                        }

                        return true;
                    }
                }
                else
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), "Get json objects failed, cannot get the most recent version!");
                    MessageBox.Show("Get json objects failed, cannot get the most recent version!", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Offline mode has been activated, cannot check for updates.");
                MessageBox.Show("Offline mode has been activated, cannot check for updates at this time.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return false;
        }

        public void InstallBakkesMod()
        {
            string url = JsonObjects.OutdatedConfig.update_info.download_url;

            if (!Directory.Exists(Properties.Settings.Default.BAKKESMOD_FOLDER))
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Creating BakkesMod folder.");
                Directory.CreateDirectory(Properties.Settings.Default.BAKKESMOD_FOLDER);
            }

            // Downloads the lastest BakkesMod version and places it in the same directory as the exe.

            using (WebClient client = new WebClient())
            {
                try
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), "Downloading BakkesMod archive.");
                    client.DownloadFile(url, AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
                }
                catch (Exception ex)
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), ex.ToString());
                }
            }

            // Extract the contents to a new BakkesMod folder in the Win64 folder, then deletes the archive afterwards.

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip"))
            {
                try
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), "Extracting archive.");
                    ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip", Properties.Settings.Default.BAKKESMOD_FOLDER);
                    Utils.Log(MethodBase.GetCurrentMethod(), "Deleting archive.");
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
                }
                catch (Exception ex)
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), ex.ToString());
                    MessageBox.Show(ex.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "No archive found, cannot extract files.");
            }
        }

        public void UpdateInjector()
        {
            // See the branch "AutoUpdaterCs" in this repo if you wish to see how the updater works.

            try
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Writing AutoUpdaterCs.");
                byte[] fileBytes = Properties.Resources.AutoUpdaterCs;
                File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe", fileBytes);
                Utils.Log(MethodBase.GetCurrentMethod(), "Opening AutoUpdaterCs.");
                Process P = new Process();
                P.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdaterCs.exe";
                P.StartInfo.Arguments = JsonObjects.BranksConfig.InjectorUrl; // Sets the download url as the arguments so the updater know's where to download the file from.
                P.Start();
                Utils.Log(MethodBase.GetCurrentMethod(), "Exiting own environment.");
                Environment.Exit(0); // We exit so the exe itself can be replaced as the updater opens the new one by itself.
            }
            catch (Exception ex)
            {
                Utils.Log(MethodBase.GetCurrentMethod(), ex.ToString());
            }
        }

        public void UpdateBakkesMod()
        {
            // Downloads the current BakkesMod version to a zip file and places it in the same directory as the exe.

            string url = JsonObjects.OutdatedConfig.update_info.download_url;
            using (WebClient client = new WebClient())
            {
                try
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), "Downloading archive.");
                    client.DownloadFile(url, AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
                }
                catch (Exception ex)
                {
                    Utils.Log(MethodBase.GetCurrentMethod(), ex.ToString());
                    MessageBox.Show(ex.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            statusLbl.Text = "Update found, installing updates..."; // The user never see's the status label when updating but I like to change it anyway.

            // Verify the download worked and start extracting it's contents to the BakkesMod folder.

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip"))
            {
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip"))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string destination = Path.GetFullPath(Path.Combine(Properties.Settings.Default.BAKKESMOD_FOLDER, entry.FullName));

                            if (entry.Name == "")
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                                continue;
                            }

                            // We don't want to override the user's settings and configurations, so this skips over those files when extracting.

                            Utils.Log(MethodBase.GetCurrentMethod(), "Checking existing installed files.");

                            if (destination.ToLower().EndsWith(".cfg") || destination.ToLower().EndsWith(".json"))
                            {
                                if (File.Exists(destination))
                                {
                                    continue;
                                }
                            }

                            Utils.Log(MethodBase.GetCurrentMethod(), "Extracting archive.");
                            entry.ExtractToFile(destination, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Might catch if the program doesn't have permission, or for whatever reason the archive got corrupted.

                    Utils.Log(MethodBase.GetCurrentMethod(), ex.ToString());
                    MessageBox.Show(ex.ToString(), "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            try
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Removing archive.");
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\bakkesmod.zip");
            }
            catch
            {
                Utils.Log(MethodBase.GetCurrentMethod(), "Failed to Remove Archive.");
                MessageBox.Show("Failed to remove bakkesmod.zip, try running as administrator if you haven't arlready.", "BakkesModInjectorCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Re-grab the new build/version info and download the json objects for the new version we just installed.

            GetJsonSuccess();
            LoadChangelog();
            SetVersionInfo();
            statusLbl.Text = "Uninjected, waiting for user to start Rocket League.";
        }
        #endregion
    }
}