using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Management;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using WinMedia = System.Windows.Media;
using WinForms = System.Windows.Forms;
using WinControls = System.Windows.Controls;

namespace ArkVRInstaller
{
    public partial class MainWindow : Window
    {
        private const string LastPathKey = "LastArkPath.txt";
        // Type de casque VR détecté : "ALVR", "WiVRn", "SteamVR", "Aucun"
        private string _casqueVrDetecte = "Aucun";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogLine("INFO  ARK VR Installateur v1.3.0 - LordMadTrix", "#00F0FF");
            LogLine("─────────────────────────────────────────────", "#1A3040");
            LoadLastPath();
            DetectGpuAndSetProfile();
            DetectUevrVersion();
            // Détection du casque VR Quest 3 / ALVR / WiVRn
            DetecterCasqueVR();
        }

        // ─── Detection Casque VR / Quest 3 ─────────────────────────────────────────
        private void DetecterCasqueVR()
        {
            Task.Run(() =>
            {
                string casque = "Aucun";
                string badge = "❌ Aucun client VR détecté";
                string flux = "";
                bool showALVR = true;

                // 1. Recherche ALVR (AppData, LocalAppData, registre, EXE)
                string alvrApp = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ALVR");
                string alvrLocal = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ALVR");
                bool alvrFound = Directory.Exists(alvrApp) || Directory.Exists(alvrLocal)
                    || VerifierRegistreApp("ALVR") || TrouverExeVR("ALVR");

                if (alvrFound)
                {
                    casque = "ALVR";
                    badge = "✅ ALVR détecté (streaming Quest)";
                    flux = "Quest 3 → Wi-Fi 6 → ALVR → SteamVR → ARK VR";
                    showALVR = false;
                }

                // 2. Recherche WiVRn
                if (casque == "Aucun")
                {
                    string wivrnPath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WiVRn");
                    bool wivrnFound = Directory.Exists(wivrnPath)
                        || VerifierRegistreApp("WiVRn") || TrouverExeVR("WiVRn");
                    if (wivrnFound)
                    {
                        casque = "WiVRn";
                        badge = "✅ WiVRn détecté (streaming open-source)";
                        flux = "Quest 3 → Wi-Fi 6 → WiVRn → OpenXR → ARK VR";
                        showALVR = false;
                    }
                }

                // 3. SteamVR natif (filé)
                if (casque == "Aucun")
                {
                    try
                    {
                        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\SteamVR");
                        if (key != null)
                        {
                            casque = "SteamVR";
                            badge = "✅ SteamVR détecté (filé / Index / Vive)";
                            flux = "Casque → USB/DP → SteamVR → UEVR → ARK VR";
                            showALVR = false;
                        }
                    }
                    catch { }
                }

                _casqueVrDetecte = casque;

                Dispatcher.Invoke(() =>
                {
                    TxtCasqueBadgeARK.Text = badge;
                    TxtFluxVRARK.Text = flux;

                    BadgeCasqueARK.Background = casque == "Aucun"
                        ? WinMedia.Brushes.DarkRed
                        : new WinMedia.SolidColorBrush(WinMedia.Color.FromRgb(0, 60, 20));
                    TxtCasqueBadgeARK.Foreground = casque == "Aucun"
                        ? WinMedia.Brushes.OrangeRed
                        : WinMedia.Brushes.LightGreen;

                    // Activer checkbox Quest 3 si ALVR ou WiVRn trouvé
                    if (casque == "ALVR" || casque == "WiVRn")
                    {
                        ChkQuest3Optimize.IsChecked = true;
                        ChkQuest3Optimize.IsEnabled = true;
                    }

                    BtnInstallALVR.Visibility = showALVR ? Visibility.Visible : Visibility.Collapsed;
                    LogLine($"[VR] {badge}", casque == "Aucun" ? "#FF6B6B" : "#00FF88");
                });
            });
        }

        // Vérifie si une application est installée via le registre Windows
        private static bool VerifierRegistreApp(string nomApp)
        {
            string[] paths = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };
            foreach (var path in paths)
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(path);
                    if (key == null) continue;
                    foreach (var sub in key.GetSubKeyNames())
                    {
                        using var subKey = key.OpenSubKey(sub);
                        var name = subKey?.GetValue("DisplayName") as string;
                        if (name != null && name.Contains(nomApp, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
                catch { }
            }
            return false;
        }

        // Cherche un EXE par nom dans Program Files et AppData
        private static bool TrouverExeVR(string nom)
        {
            string[] roots = {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };
            foreach (var root in roots)
            {
                if (string.IsNullOrEmpty(root)) continue;
                try
                {
                    bool found = Directory.EnumerateFiles(root, $"*{nom}*.exe", SearchOption.AllDirectories)
                        .Take(1).Any();
                    if (found) return true;
                }
                catch { }
            }
            return false;
        }

        // Ouvre la page de téléchargement ALVR
        private void BtnInstallALVR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(
                    "https://github.com/alvr-org/ALVR/releases/latest") { UseShellExecute = true });
                LogLine("[VR] Page ALVR ouverte dans le navigateur.", "#00D0FF");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Impossible d'ouvrir le navigateur :\n{ex.Message}",
                    "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // ─── Detection GPU ──────────────────────────────────────────────────────
        private void DetectGpuAndSetProfile()
        {
            Task.Run(() =>
            {
                try
                {
                    string gpuName = "GPU inconnu";
                    int vramMb = 0;

                    using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        gpuName = obj["Name"]?.ToString() ?? "GPU inconnu";
                        if (obj["AdapterRAM"] != null)
                            vramMb = (int)((ulong)obj["AdapterRAM"] / (1024 * 1024));
                        break;
                    }

                    string profileLabel;
                    Action selectProfile;

                    if (gpuName.Contains("4090") || gpuName.Contains("4080") || gpuName.Contains("4070 Ti"))
                    {
                        profileLabel = "Ultra (auto)";
                        selectProfile = () => { RadioUltra.IsChecked = true; };
                    }
                    else if (gpuName.Contains("3080") || gpuName.Contains("3090") || gpuName.Contains("4070")
                          || gpuName.Contains("6800") || gpuName.Contains("6900") || gpuName.Contains("7900"))
                    {
                        profileLabel = "Equilibre (auto)";
                        selectProfile = () => { RadioBalanced.IsChecked = true; };
                    }
                    else if (gpuName.Contains("3060") || gpuName.Contains("3070") || gpuName.Contains("6700"))
                    {
                        profileLabel = "Equilibre (auto)";
                        selectProfile = () => { RadioBalanced.IsChecked = true; };
                    }
                    else
                    {
                        profileLabel = "Eco (auto)";
                        selectProfile = () => { RadioEco.IsChecked = true; };
                    }

                    int vramGb = vramMb / 1024;
                    string vramStr = vramGb > 0 ? $"{vramGb} Go VRAM" : "VRAM inconnu";

                    Dispatcher.Invoke(() =>
                    {
                        TxtGpuInfo.Text = $"GPU  {gpuName} - {vramStr}";
                        TxtGpuInfo.Foreground = new WinMedia.SolidColorBrush(WinMedia.Color.FromRgb(0, 200, 150));
                        TxtAutoProfile.Text = $"Profil auto : {profileLabel}";
                        selectProfile();
                        if (vramGb >= 8) ChkHighQualityTextures.IsChecked = true;
                    });

                    LogLine($"GPU detecte : {gpuName} ({vramStr}) -> profil {profileLabel}", "#00FF88");
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => TxtGpuInfo.Text = "Detection GPU indisponible");
                    LogLine($"Detection GPU : {ex.Message}", "#FF9800");
                }
            });
        }

        // ─── Detection UEVR ─────────────────────────────────────────────────────
        private void DetectUevrVersion()
        {
            Task.Run(() =>
            {
                try
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string uevrExe = Path.Combine(appData, @"uevr\UEVRInjector.exe");
                    if (File.Exists(uevrExe))
                    {
                        var info = FileVersionInfo.GetVersionInfo(uevrExe);
                        string ver = info.FileVersion ?? "installe";
                        Dispatcher.Invoke(() =>
                        {
                            TxtUevrBadge.Text = $"UEVR {ver}";
                            BadgeUEVR.Visibility = Visibility.Visible;
                        });
                        LogLine($"UEVR detecte : version {ver}", "#00FF88");
                    }
                    else
                    {
                        LogLine("UEVR non detecte dans AppData - sera installe lors du deploiement.", "#8FA7B3");
                    }
                }
                catch (Exception ex)
                {
                    LogLine($"Detection UEVR : {ex.Message}", "#FF9800");
                }
            });
        }

        // ─── Memoire chemin ─────────────────────────────────────────────────────
        private void LoadLastPath()
        {
            try
            {
                using var store = IsolatedStorageFile.GetUserStoreForAssembly();
                if (store.FileExists(LastPathKey))
                {
                    using var sr = new StreamReader(new IsolatedStorageFileStream(LastPathKey, FileMode.Open, store));
                    string saved = sr.ReadLine()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(saved) && Directory.Exists(saved))
                    {
                        TxtGamePath.Text = saved;
                        ValidateArkPath(saved);
                        LogLine($"Chemin restaure : {saved}", "#8FA7B3");
                    }
                }
            }
            catch (Exception ex)
            {
                LogLine($"Chargement chemin : {ex.Message}", "#FF9800");
            }
        }

        private void SaveLastPath(string path)
        {
            try
            {
                using var store = IsolatedStorageFile.GetUserStoreForAssembly();
                using var sw = new StreamWriter(new IsolatedStorageFileStream(LastPathKey, FileMode.Create, store));
                sw.WriteLine(path);
            }
            catch (Exception ex)
            {
                LogLine($"Sauvegarde chemin : {ex.Message}", "#FF9800");
            }
        }

        // ─── Navigation ─────────────────────────────────────────────────────────
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new WinForms.FolderBrowserDialog
            {
                Description = "Selectionnez le dossier racine d'ARK: Survival Evolved",
                UseDescriptionForTitle = true
            };
            if (!string.IsNullOrEmpty(TxtGamePath.Text) && Directory.Exists(TxtGamePath.Text))
                dialog.SelectedPath = TxtGamePath.Text;

            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                TxtGamePath.Text = dialog.SelectedPath;
                ValidateArkPath(dialog.SelectedPath);
            }
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = TxtGamePath.Text.Trim();
            if (Directory.Exists(path))
                Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{path}\"", UseShellExecute = true });
            else
                LogLine("Dossier invalide - impossible d'ouvrir l'explorateur.", "#FF5252");
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            LogConsole.Items.Clear();
        }

        // ─── Validation ARK ─────────────────────────────────────────────────────
        private bool IsValidArkFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return File.Exists(Path.Combine(path, @"ShooterGame\Binaries\Win64\ShooterGame.exe"));
        }

        private void ValidateArkPath(string path)
        {
            if (IsValidArkFolder(path))
            {
                TxtDetectStatus.Text = "OK ARK: Survival Evolved detecte avec succes !";
                TxtDetectStatus.Foreground = new WinMedia.SolidColorBrush(WinMedia.Color.FromRgb(0, 255, 136));
                SaveLastPath(path);
            }
            else
            {
                TxtDetectStatus.Text = "ERR ShooterGame.exe introuvable - verifiez le chemin.";
                TxtDetectStatus.Foreground = new WinMedia.SolidColorBrush(WinMedia.Color.FromRgb(255, 82, 82));
            }
        }

        // ─── Installation ───────────────────────────────────────────────────────
        private async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            string arkPath = TxtGamePath.Text.Trim();
            if (!IsValidArkFolder(arkPath))
            {
                System.Windows.MessageBox.Show("Dossier ARK invalide. Veuillez verifier le chemin.", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            BtnInstall.IsEnabled = false;
            BtnLaunch.IsEnabled = false;
            try { await Task.Run(() => RunInstall(arkPath)); }
            finally
            {
                Dispatcher.Invoke(() => { BtnInstall.IsEnabled = true; BtnLaunch.IsEnabled = true; });
            }
        }

        private void RunInstall(string arkPath)
        {
            string tempZip = Path.Combine(Path.GetTempPath(), "ark_vr_payload.zip");
            string tempExtract = Path.Combine(Path.GetTempPath(), "ark_vr_extract");

            try
            {
                // 1. Payload
                UpdateStatus("Extraction du payload UEVR...", 10);
                LogLine("Extraction du payload embarque...", "#8FA7B3");

                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string payloadZip = Path.Combine(exeDir, "payload.zip");
                if (!File.Exists(payloadZip))
                    payloadZip = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\payload.zip"));

                if (File.Exists(payloadZip))
                {
                    File.Copy(payloadZip, tempZip, true);
                    LogLine($"Payload trouve : {new FileInfo(tempZip).Length / 1024} Ko", "#00FF88");
                }
                else
                    LogLine("Payload non trouve - extraction UEVR ignoree.", "#FF9800");

                // 2. Installer UEVR
                UpdateStatus("Deploiement UEVR dans le dossier du jeu...", 30);
                LogLine("Deploiement UEVR...", "#8FA7B3");

                string win64 = Path.Combine(arkPath, @"ShooterGame\Binaries\Win64");
                string uevrDir = Path.Combine(win64, "uevr");
                Directory.CreateDirectory(uevrDir);

                if (File.Exists(tempZip))
                {
                    if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true);
                    System.IO.Compression.ZipFile.ExtractToDirectory(tempZip, tempExtract);
                    CopyDirectory(tempExtract, uevrDir);
                    LogLine($"UEVR installe dans : {uevrDir}", "#00FF88");
                }

                // 3. Config UEVR
                UpdateStatus("Configuration des options VR...", 55);
                LogLine("Configuration des options VR...", "#8FA7B3");

                string appDataUevr = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"uevr\profiles\ShooterGame");
                Directory.CreateDirectory(appDataUevr);

                string configContent = "[VRSettings]\nVR_SyncedSequentialEnabled=1\nVR_DinoMountCamera=1\nVR_ComfortVignette=1\nVR_LaserPointerEnabled=1\nVR_FlyingMountBankRoll=1\n";

                Dispatcher.Invoke(() =>
                {
                    if (ChkSyncedSequential.IsChecked == false) configContent = configContent.Replace("VR_SyncedSequentialEnabled=1", "VR_SyncedSequentialEnabled=0");
                    if (ChkDinoCamera.IsChecked == false) configContent = configContent.Replace("VR_DinoMountCamera=1", "VR_DinoMountCamera=0");
                    if (ChkVignette.IsChecked == false) configContent = configContent.Replace("VR_ComfortVignette=1", "VR_ComfortVignette=0");
                    if (ChkLaserSight.IsChecked == false) configContent = configContent.Replace("VR_LaserPointerEnabled=1", "VR_LaserPointerEnabled=0");
                    if (ChkPteroRoll.IsChecked == false) configContent = configContent.Replace("VR_FlyingMountBankRoll=1", "VR_FlyingMountBankRoll=0");
                });

                File.WriteAllText(Path.Combine(appDataUevr, "config.txt"), configContent);
                LogLine("Config UEVR ecrite.", "#00FF88");

                // 4. Engine.ini
                UpdateStatus("Application des optimisations graphiques VR...", 70);
                string savedConfigDir = Path.Combine(arkPath, @"ShooterGame\Saved\Config\WindowsNoEditor");
                Directory.CreateDirectory(savedConfigDir);
                string engineIniPath = Path.Combine(savedConfigDir, "Engine.ini");

                bool doBackup = false, disableClouds = false, highTex = false, noMotionBlur = false;
                Dispatcher.Invoke(() =>
                {
                    doBackup = ChkBackupIni.IsChecked == true;
                    disableClouds = ChkDisableClouds.IsChecked == true;
                    highTex = ChkHighQualityTextures.IsChecked == true;
                    noMotionBlur = ChkDisableMotionBlur.IsChecked == true;
                });

                if (doBackup && File.Exists(engineIniPath))
                {
                    File.Copy(engineIniPath, engineIniPath + ".bak", true);
                    LogLine("Backup Engine.ini cree : Engine.ini.bak", "#8FA7B3");
                }

                if (disableClouds || noMotionBlur || highTex)
                {
                    ApplyEngineTweaks(engineIniPath, disableClouds, noMotionBlur, highTex);
                    LogLine("Engine.ini optimise pour la VR.", "#00FF88");
                }

                // 5. Raccourcis
                UpdateStatus("Creation des raccourcis...", 88);
                bool createShortcut = false;
                Dispatcher.Invoke(() => createShortcut = ChkDesktopShortcut.IsChecked == true);
                if (createShortcut) { CreateDesktopShortcuts(arkPath); LogLine("Raccourcis Bureau crees.", "#00FF88"); }

                UpdateStatus("Installation terminee et prete pour la Realite Virtuelle !", 100);
                LogLine("─────────────────────────────────────────────", "#1A3040");
                LogLine("ARK VR (FR) v1.3.0 installe avec succes !", "#00FF88");

                Dispatcher.Invoke(() =>
                    System.Windows.MessageBox.Show("ARK VR (Edition Francaise v1.3.0) installe avec succes !\n\nAllumez votre casque VR et cliquez sur LANCER EN VR.",
                        "Installation reussie", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.None));
            }
            catch (Exception ex)
            {
                LogLine($"ERREUR : {ex.Message}", "#FF5252");
                UpdateStatus($"Echec : {ex.Message}", 0);
                Dispatcher.Invoke(() => System.Windows.MessageBox.Show($"Erreur lors de l'installation :\n{ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error));
            }
            finally
            {
                try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch (Exception ex) { LogLine($"Nettoyage temp : {ex.Message}", "#FF9800"); }
                try { if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true); } catch (Exception ex) { LogLine($"Nettoyage extract : {ex.Message}", "#FF9800"); }
            }
        }

        // ─── Tweaks Engine.ini ──────────────────────────────────────────────────
        private void ApplyEngineTweaks(string engineIniPath, bool disableClouds, bool noMotionBlur, bool highTex)
        {
            var lines = new List<string> { "", "[SystemSettings]" };
            if (disableClouds)
            {
                lines.Add("r.VolumetricCloud=0"); lines.Add("r.TrueSkyQuality=0");
                lines.Add("r.LightShaftQuality=0"); lines.Add("r.BloomQuality=1");
                lines.Add("r.ShadowQuality=2"); lines.Add("r.ContactShadows=0");
                lines.Add("r.DepthOfFieldQuality=0"); lines.Add("r.ViewDistanceScale=1.2");
            }
            if (noMotionBlur) lines.Add("r.MotionBlurQuality=0");
            lines.Add(highTex ? "r.Streaming.PoolSize=6144" : "r.Streaming.PoolSize=4096");

            string tweaks = string.Join(Environment.NewLine, lines) + Environment.NewLine;
            if (File.Exists(engineIniPath))
            {
                string existing = File.ReadAllText(engineIniPath);
                if (!existing.Contains("[SystemSettings]")) File.AppendAllText(engineIniPath, tweaks);
                else LogLine("Engine.ini deja configure - tweaks ignores.", "#8FA7B3");
            }
            else File.WriteAllText(engineIniPath, tweaks);
        }

        private void CreateDesktopShortcuts(string arkPath)
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string bat = $"@echo off\ntitle ARK VR - LordMadTrix\ncd /d \"{arkPath}\\ShooterGame\\Binaries\\Win64\"\nstart \"\" \"uevr\\UEVRInjector.exe\"\nstart /high \"\" \"ShooterGame.exe\" -NoBattlEye\nexit\n";
                File.WriteAllText(Path.Combine(desktop, "ARK VR (FR).cmd"), bat);
            }
            catch (Exception ex) { LogLine($"Raccourci Bureau : {ex.Message}", "#FF9800"); }
        }

        // ─── Lancement VR ───────────────────────────────────────────────────────
        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            string arkPath = TxtGamePath.Text.Trim();
            if (!IsValidArkFolder(arkPath))
            {
                System.Windows.MessageBox.Show("Dossier ARK invalide. Veuillez verifier le chemin.", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            string win64 = Path.Combine(arkPath, @"ShooterGame\Binaries\Win64");
            string injector = Path.Combine(win64, @"uevr\UEVRInjector.exe");
            string gameExe = Path.Combine(win64, "ShooterGame.exe");

            if (!File.Exists(injector))
            {
                var res = System.Windows.MessageBox.Show("UEVR n'est pas installe dans le dossier du jeu.\n\nVoulez-vous lancer l'installation maintenant ?",
                    "UEVR manquant", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
                if (res == System.Windows.MessageBoxResult.Yes) BtnInstall_Click(sender, e);
                return;
            }

            try
            {
                LogLine("Lancement d'ARK en Realite Virtuelle...", "#00F0FF");
                Process.Start(new ProcessStartInfo { FileName = injector, WorkingDirectory = Path.GetDirectoryName(injector)!, UseShellExecute = true });

                bool highPriority = ChkCpuPriority.IsChecked == true;
                var proc = Process.Start(new ProcessStartInfo { FileName = gameExe, Arguments = "-NoBattlEye", WorkingDirectory = win64, UseShellExecute = true });
                if (proc != null && highPriority)
                {
                    try { proc.PriorityClass = ProcessPriorityClass.High; }
                    catch (Exception ex) { LogLine($"Priorite CPU : {ex.Message}", "#FF9800"); }
                }

                UpdateStatus("ARK VR lance avec succes !", 100);
                LogLine("ARK VR en cours d'execution.", "#00FF88");
            }
            catch (Exception ex)
            {
                LogLine($"Erreur lancement : {ex.Message}", "#FF5252");
                System.Windows.MessageBox.Show($"Erreur lors du lancement : {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // ─── Restauration Vanilla ───────────────────────────────────────────────
        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            string arkPath = TxtGamePath.Text.Trim();
            if (!IsValidArkFolder(arkPath)) return;

            var result = System.Windows.MessageBox.Show(
                "Voulez-vous restaurer ARK en mode classique ecran plat (Vanilla) ?\nVos sauvegardes, mondes et dinosaures ne seront pas touches.",
                "Confirmer la restauration", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result != System.Windows.MessageBoxResult.Yes) return;

            try
            {
                string uevrDir = Path.Combine(arkPath, @"ShooterGame\Binaries\Win64\uevr");
                if (Directory.Exists(uevrDir)) { Directory.Delete(uevrDir, true); LogLine($"Dossier UEVR supprime : {uevrDir}", "#8FA7B3"); }

                string appDataUevr = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"uevr\profiles\ShooterGame");
                if (Directory.Exists(appDataUevr)) { Directory.Delete(appDataUevr, true); LogLine("Profil UEVR AppData supprime.", "#8FA7B3"); }

                UpdateStatus("Restauration Vanilla effectuee avec succes !", 0);
                LogLine("ARK restaure en configuration Vanilla d'origine.", "#00FF88");
                System.Windows.MessageBox.Show("ARK a ete restaure en configuration Vanilla d'origine.", "Succes", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogLine($"Erreur restauration : {ex.Message}", "#FF5252");
                System.Windows.MessageBox.Show($"Erreur lors de la restauration : {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // ─── Helpers ────────────────────────────────────────────────────────────
        private void UpdateStatus(string message, int percent)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = percent;
                TxtProgressPercent.Text = $"{percent}%";
                LogLine(message, percent == 100 ? "#00FF88" : "#8FA7B3");
            });
        }

        private void LogLine(string message, string hexColor = "#8FA7B3")
        {
            Dispatcher.Invoke(() =>
            {
                var color = (WinMedia.Color)WinMedia.ColorConverter.ConvertFromString(hexColor);
                var tb = new WinControls.TextBlock
                {
                    Text = $"[{DateTime.Now:HH:mm:ss}] {message}",
                    Foreground = new WinMedia.SolidColorBrush(color),
                    FontFamily = new WinMedia.FontFamily("Consolas"),
                    FontSize = 11.5
                };
                var item = new WinControls.ListBoxItem { Content = tb };
                LogConsole.Items.Add(item);
                LogConsole.ScrollIntoView(item);
            });
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) return;
            Directory.CreateDirectory(destinationDir);
            foreach (FileInfo file in dir.GetFiles())
                file.CopyTo(Path.Combine(destinationDir, file.Name), true);
            foreach (DirectoryInfo subDir in dir.GetDirectories())
                CopyDirectory(subDir.FullName, Path.Combine(destinationDir, subDir.Name));
        }
    }
}

