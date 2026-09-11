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







            LogLine("INFO  ARK VR Installateur v1.4.0 - LordMadTrix", "#00F0FF");







            LogLine("─────────────────────────────────────────────", "#1A3040");







            LoadLastPath();







            if (string.IsNullOrWhiteSpace(TxtGamePath.Text) || !IsValidArkFolder(TxtGamePath.Text))







            {







                string detected = FindArkPath();







                if (!string.IsNullOrEmpty(detected))







                {







                    TxtGamePath.Text = detected;







                    ValidateArkPath(detected);







                    LogLine($"[OK] ARK: Survival Evolved detecte automatiquement : {detected}", "#00FF88");







                }







                else







                {







                    TxtDetectStatus.Text = "⚠️ ARK non detecte automatiquement. Cliquez sur Parcourir...";







                    TxtDetectStatus.Foreground = new WinMedia.SolidColorBrush(WinMedia.Color.FromRgb(255, 180, 0));







                    LogLine("Chemin ARK non detecte automatiquement. Veuillez le selectionner manuellement.", "#FFB400");







                }







            }







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















        // Cherche un EXE VR par chemin direct pour un demarrage instantane







        private static bool TrouverExeVR(string nom)







        {







            string[] directPaths = {







                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), nom),







                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), nom),







                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nom),







                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), nom, $"{nom}.exe"),







                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), nom, $"{nom}.exe"),







                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nom, $"{nom}.exe")







            };







            foreach (var p in directPaths)







            {







                if (File.Exists(p) || Directory.Exists(p)) return true;







            }







            return false;







        }















                private void BtnOpenDLSS5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dlss5Exe = @"D:\ROG_Tools\DLSS5-Swapper\DLSS5-Swapper.exe";
                if (File.Exists(dlss5Exe))
                {
                    Process.Start(new ProcessStartInfo { FileName = dlss5Exe, WorkingDirectory = Path.GetDirectoryName(dlss5Exe), UseShellExecute = true });
                    LogLine("[DLSS] DLSS 5 Swapper v2.2.1 ouvert. (En jeu, appuyez sur F8 pour l'overlay)", "#00FF88");
                }
                else
                {
                    Process.Start(new ProcessStartInfo("https://github.com/rakanki911/DLSS5-Swapper/releases/tag/v2.2.1") { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossible de lancer DLSS 5 Swapper :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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















                // ─── Lecture VRAM 64 bits registre (pour GPU modernes NVIDIA/AMD > 4 Go) ───
        private static ulong GetDedicatedGpuVramFromRegistry(string gpuName)
        {
            try
            {
                using var classKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
                if (classKey != null)
                {
                    foreach (var subKeyName in classKey.GetSubKeyNames())
                    {
                        if (subKeyName.StartsWith("00"))
                        {
                            using var subKey = classKey.OpenSubKey(subKeyName);
                            if (subKey != null)
                            {
                                var desc = subKey.GetValue("DriverDesc") as string;
                                if (!string.IsNullOrEmpty(desc) && (desc.Contains(gpuName, StringComparison.OrdinalIgnoreCase) || gpuName.Contains(desc, StringComparison.OrdinalIgnoreCase)))
                                {
                                    var qwMem = subKey.GetValue("HardwareInformation.qwMemorySize");
                                    if (qwMem != null)
                                    {
                                        return Convert.ToUInt64(qwMem);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return 0;
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

                    ulong maxBytes = 0;



                    using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");

                    ManagementObject bestObj = null;



                    foreach (ManagementObject obj in searcher.Get())

                    {

                        string name = obj["Name"]?.ToString() ?? "";

                        ulong ramBytes = 0;

                        if (obj["AdapterRAM"] != null)

                        {

                            try { ramBytes = Convert.ToUInt64(obj["AdapterRAM"]); } catch { }

                        }



                        bool isDedicated = name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)

                                        || name.Contains("GeForce", StringComparison.OrdinalIgnoreCase)

                                        || name.Contains("Radeon", StringComparison.OrdinalIgnoreCase);



                        if (bestObj == null)

                        {

                            bestObj = obj;

                            maxBytes = ramBytes;

                        }

                        else

                        {

                            string currentBest = bestObj["Name"]?.ToString() ?? "";

                            bool bestIsDedicated = currentBest.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)

                                                || currentBest.Contains("GeForce", StringComparison.OrdinalIgnoreCase)

                                                || currentBest.Contains("Radeon", StringComparison.OrdinalIgnoreCase);



                            if (isDedicated && !bestIsDedicated)

                            {

                                bestObj = obj;

                                maxBytes = ramBytes;

                            }

                            else if (isDedicated == bestIsDedicated && ramBytes > maxBytes)

                            {

                                bestObj = obj;

                                maxBytes = ramBytes;

                            }

                        }

                    }



                    if (bestObj != null)

                    {

                        gpuName = bestObj["Name"]?.ToString() ?? "GPU inconnu";

                        vramMb = (int)(maxBytes / (1024 * 1024));

                    }



                    // Lecture prioritaire du registre 64 bits (HardwareInformation.qwMemorySize)

                    ulong regVramBytes = GetDedicatedGpuVramFromRegistry(gpuName);

                    if (regVramBytes > 0)

                    {

                        maxBytes = regVramBytes;

                        vramMb = (int)(regVramBytes / (1024 * 1024));

                    }



                    int vramGb = (int)Math.Round((double)maxBytes / (1024.0 * 1024.0 * 1024.0));

                    if (vramGb == 0 && vramMb > 0)

                    {

                        vramGb = (vramMb + 512) / 1024;

                    }

                    if (gpuName.Contains("3080 Ti", StringComparison.OrdinalIgnoreCase) && vramGb < 12)

                    {

                        vramGb = 16;

                    }



                    string profileLabel;

                    Action selectProfile;



                    if (gpuName.Contains("4090") || gpuName.Contains("4080") || gpuName.Contains("3080 Ti") || gpuName.Contains("3090"))

                    {

                        profileLabel = "Ultra (auto)";

                        selectProfile = () => { RadioUltra.IsChecked = true; };

                    }

                    else if (gpuName.Contains("3080") || gpuName.Contains("4070")

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







                private void TxtGamePath_TextChanged(object sender, WinControls.TextChangedEventArgs e)







        {







            if (TxtGamePath != null)







            {







                ValidateArkPath(TxtGamePath.Text.Trim());







            }







        }















        private string FindArkPath()







        {







            string[] candidates =







            {







                @"D:\SteamLibrary\steamapps\common\ARK",







                @"C:\Program Files (x86)\Steam\steamapps\common\ARK",







                @"C:\Steam\steamapps\common\ARK",







                @"C:\SteamLibrary\steamapps\common\ARK",







                @"E:\SteamLibrary\steamapps\common\ARK",







                @"F:\SteamLibrary\steamapps\common\ARK",







                @"G:\SteamLibrary\steamapps\common\ARK"







            };















            foreach (var p in candidates)







            {







                if (IsValidArkFolder(p))







                    return p;







            }















            try







            {







                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");







                if (key?.GetValue("SteamPath") is string steamPath)







                {







                    steamPath = steamPath.Replace('/', '\\');







                    string vdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");







                    if (File.Exists(vdf))







                    {







                        var lines = File.ReadAllLines(vdf);







                        foreach (var line in lines)







                        {







                            var trimmed = line.Trim();







                            if (trimmed.StartsWith("\"path\""))







                            {







                                var parts = trimmed.Split('"');







                                if (parts.Length >= 4)







                                {







                                    string libPath = parts[3].Replace(@"\\", @"\");







                                    string arkCandidate = Path.Combine(libPath, "steamapps", "common", "ARK");







                                    if (IsValidArkFolder(arkCandidate))







                                    {







                                        return arkCandidate;







                                    }







                                }







                            }







                        }







                    }







                }







            }







            catch { }















            return string.Empty;







        }







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







            // OpenFolderDialog WPF natif (.NET 8+) — remplace WinForms.FolderBrowserDialog







            var dialog = new Microsoft.Win32.OpenFolderDialog







            {







                Title = "Selectionnez le dossier racine d'ARK: Survival Evolved",







                Multiselect = false







            };







            if (!string.IsNullOrEmpty(TxtGamePath.Text) && Directory.Exists(TxtGamePath.Text))







                dialog.InitialDirectory = TxtGamePath.Text;















            if (dialog.ShowDialog() == true)







            {







                TxtGamePath.Text = dialog.FolderName;







                ValidateArkPath(dialog.FolderName);







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















                private void LogConsole_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)



        {



            e.Handled = true;



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















                var assembly = System.Reflection.Assembly.GetExecutingAssembly();







                using var resStream = assembly.GetManifestResourceStream("ArkVRInstaller.payload.zip");







                if (resStream != null)







                {







                    using (var fs = File.Create(tempZip)) { resStream.CopyTo(fs); }







                    LogLine($"Payload embarque extrait ({new FileInfo(tempZip).Length / 1024} Ko).", "#00FF88");







                }







                else







                {







                    string exeDir = AppDomain.CurrentDomain.BaseDirectory;







                    string payloadZip = Path.Combine(exeDir, "payload.zip");







                    if (!File.Exists(payloadZip))







                        payloadZip = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\payload.zip"));















                    if (File.Exists(payloadZip))







                    {







                        File.Copy(payloadZip, tempZip, true);







                        LogLine($"Payload trouve sur disque : {new FileInfo(tempZip).Length / 1024} Ko", "#00FF88");







                    }







                    else







                        LogLine("Payload non trouve - extraction UEVR ignoree.", "#FF9800");







                }















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







                    string sourceUevr = Directory.Exists(Path.Combine(tempExtract, "uevr"))







                        ? Path.Combine(tempExtract, "uevr")







                        : tempExtract;







                    CopyDirectory(sourceUevr, uevrDir);







                    LogLine($"UEVR installe dans : {uevrDir}", "#00FF88");







                }















                // 3. Config UEVR







                UpdateStatus("Configuration des options VR...", 55);







                LogLine("Configuration des options VR...", "#8FA7B3");















                string appDataUevr = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"uevr\profiles\ShooterGame");







                Directory.CreateDirectory(appDataUevr);















                string configContent = "[VRSettings]\nVR_SyncedSequentialEnabled=1\nVR_DinoMountCamera=1\nVR_ComfortVignette=1\nVR_LaserPointerEnabled=1\nVR_FlyingMountBankRoll=1\n";















                bool quest3Opt = false;







                int hwProfile = 1; // 0=Eco, 1=Balanced, 2=Ultra







                Dispatcher.Invoke(() =>







                {







                    quest3Opt = ChkQuest3Optimize.IsChecked == true;







                    if (RadioEco.IsChecked == true) hwProfile = 0;







                    else if (RadioUltra.IsChecked == true) hwProfile = 2;







                    else hwProfile = 1;















                    if (ChkSyncedSequential.IsChecked == false) configContent = configContent.Replace("VR_SyncedSequentialEnabled=1", "VR_SyncedSequentialEnabled=0");







                    if (ChkDinoCamera.IsChecked == false) configContent = configContent.Replace("VR_DinoMountCamera=1", "VR_DinoMountCamera=0");







                    if (ChkVignette.IsChecked == false) configContent = configContent.Replace("VR_ComfortVignette=1", "VR_ComfortVignette=0");







                    if (ChkLaserSight.IsChecked == false) configContent = configContent.Replace("VR_LaserPointerEnabled=1", "VR_LaserPointerEnabled=0");







                    if (ChkPteroRoll.IsChecked == false) configContent = configContent.Replace("VR_FlyingMountBankRoll=1", "VR_FlyingMountBankRoll=0");







                });















                if (quest3Opt)







                {







                    configContent += "VR_RefreshRate=120\nVR_ResolutionScale=1.2\nVR_StreamingCodec=H265\nVR_StreamingBitrate=150\n";







                    LogLine("[VR] Profil Quest 3 applique : 120Hz, H.265 & debit 150 Mbps.", "#00F0FF");







                }







                else







                {







                    int refresh = hwProfile == 0 ? 72 : (hwProfile == 2 ? 120 : 90);







                    configContent += $"VR_RefreshRate={refresh}\n";







                }















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















                if (disableClouds || noMotionBlur || highTex || quest3Opt)







                {







                    ApplyEngineTweaks(engineIniPath, disableClouds, noMotionBlur, highTex, quest3Opt, hwProfile);







                    LogLine("Engine.ini optimise pour la VR.", "#00FF88");







                }















                // 5. Raccourcis







                UpdateStatus("Creation des raccourcis...", 88);







                bool createShortcut = false;







                Dispatcher.Invoke(() => createShortcut = ChkDesktopShortcut.IsChecked == true);







                if (createShortcut) { CreateDesktopShortcuts(arkPath); LogLine("Raccourcis Bureau crees.", "#00FF88"); }















                UpdateStatus("Installation terminee et prete pour la Realite Virtuelle !", 100);







                LogLine("─────────────────────────────────────────────", "#1A3040");







                LogLine("ARK VR (FR) v1.4.0 installe avec succes !", "#00FF88");















                Dispatcher.Invoke(() =>







                    System.Windows.MessageBox.Show("ARK VR (Edition Francaise v1.4.0) installe avec succes !\n\nAllumez votre casque VR et cliquez sur LANCER EN VR.",







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







        private void ApplyEngineTweaks(string engineIniPath, bool disableClouds, bool noMotionBlur, bool highTex, bool quest3Opt = false, int hwProfile = 1)







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







            lines.Add(highTex ? "r.Streaming.PoolSize=6144" : (hwProfile == 0 ? "r.Streaming.PoolSize=3072" : "r.Streaming.PoolSize=4096"));







            lines.Add("r.VSync=0");







            lines.Add("r.OneFrameThreadLag=1");







            int targetFps = quest3Opt ? 120 : (hwProfile == 0 ? 72 : (hwProfile == 2 ? 144 : 90));







            lines.Add($"t.MaxFPS={targetFps}");















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







                string win64 = Path.Combine(arkPath, @"ShooterGame\Binaries\Win64");















                var sb = new System.Text.StringBuilder();







                sb.AppendLine("@echo off");







                sb.AppendLine("title ARK VR - Lancement Haute Performance (LordMadTrix)");







                sb.AppendLine("chcp 65001 >nul");







                sb.AppendLine("echo Demarrage d'ARK en Realite Virtuelle 6DOF...");







                sb.AppendLine("tasklist | findstr /i \"vrserver.exe\" >nul || start \"\" \"steam://run/250820\"");







                sb.AppendLine("cd /d \"" + win64 + "\"");







                sb.AppendLine("start /high \"\" \"ShooterGame.exe\" -NoBattlEye");







                sb.AppendLine("exit");















                File.WriteAllText(Path.Combine(desktop, "ARK VR (FR).cmd"), sb.ToString(), System.Text.Encoding.UTF8);















                string urlPath = Path.Combine(desktop, "ARK VR (FR).url");







                string urlContent = $"[InternetShortcut]\nURL=steam://rungameid/346110\nIconIndex=0\nIconFile={Path.Combine(win64, "ShooterGame.exe")}\n";







                File.WriteAllText(urlPath, urlContent, System.Text.Encoding.ASCII);







            }







            catch (Exception ex) { LogLine($"Raccourci Bureau : {ex.Message}", "#FF9800"); }







        }















        private async void BtnLaunch_Click(object sender, RoutedEventArgs e)







        {







            string arkPath = TxtGamePath.Text.Trim();







            if (!IsValidArkFolder(arkPath))







            {







                System.Windows.MessageBox.Show("Dossier ARK invalide. Veuillez verifier le chemin.", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);







                return;







            }















            string win64 = Path.Combine(arkPath, @"ShooterGame\Binaries\Win64");







            string uevrDll = Path.Combine(win64, @"uevr\UEVRBackend.dll");







            string injectorExe = Path.Combine(win64, @"uevr\UEVRInjector.exe");







            string gameExe = Path.Combine(win64, "ShooterGame.exe");















            if (!File.Exists(uevrDll))







            {







                var res = System.Windows.MessageBox.Show("Les modules VR ne sont pas encore installes dans le dossier du jeu.\n\nVoulez-vous lancer l'installation maintenant ?",







                    "Composants VR manquants", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);







                if (res == System.Windows.MessageBoxResult.Yes) BtnInstall_Click(sender, e);







                return;







            }















            try







            {







                LogLine("Demarrage d'ARK en Realite Virtuelle (Auto-Injection Zero-Clic)...", "#00F0FF");







                UpdateStatus("Lancement d'ARK et initialisation VR...", 30);















                // Demarrage automatique de SteamVR si inactif







                if (Process.GetProcessesByName("vrserver").Length == 0 && Process.GetProcessesByName("vrmonitor").Length == 0)







                {







                    try







                    {







                        Process.Start(new ProcessStartInfo("steam://run/250820") { UseShellExecute = true });







                        LogLine("SteamVR demarre en arriere-plan.", "#8FA7B3");







                    }







                    catch { }







                }















                bool highPriority = ChkCpuPriority.IsChecked == true;







                var proc = Process.Start(new ProcessStartInfo







                {







                    FileName = gameExe,







                    Arguments = "-NoBattlEye",







                    WorkingDirectory = win64,







                    UseShellExecute = true







                });















                if (proc != null && highPriority)







                {







                    try { proc.PriorityClass = ProcessPriorityClass.High; }







                    catch (Exception ex) { LogLine($"Priorite CPU : {ex.Message}", "#FF9800"); }







                }















                // Surveillance et Auto-Injection en arriere-plan sans bloquer l'interface







                await Task.Run(async () =>







                {







                    bool injected = false;







                    for (int i = 0; i < 45; i++)







                    {







                        await Task.Delay(1000);







                        var targets = Process.GetProcessesByName("ShooterGame");







                        if (targets.Length > 0)







                        {







                            var targetProc = targets[0];







                            // Pause pour laisser DirectX initialiser la fenetre 3D







                            await Task.Delay(3500);















                            injected = DllInjector.Inject(targetProc.Id, uevrDll);







                            if (injected)







                            {







                                Dispatcher.Invoke(() =>







                                {







                                    UpdateStatus("ARK VR connecte et actif dans le casque !", 100);







                                    LogLine($"[OK] ShooterGame detecte (PID: {targetProc.Id}).", "#00FF88");







                                    LogLine("[OK] Auto-injection UEVR reussie ! Basculement VR 6DOF actif.", "#00FF88");







                                });







                                break;







                            }







                        }







                    }















                    // Fallback sur l'injecteur graphique standard si l'injection directe a echoue







                    if (!injected && File.Exists(injectorExe))







                    {







                        Dispatcher.Invoke(() =>







                        {







                            LogLine("[!] Injection directe differee. Demarrage de l'injecteur d'appoint...", "#FF9800");







                            Process.Start(new ProcessStartInfo { FileName = injectorExe, WorkingDirectory = Path.GetDirectoryName(injectorExe)!, UseShellExecute = true });







                        });







                    }







                });







            }







            catch (Exception ex)







            {







                LogLine($"Erreur lancement : {ex.Message}", "#FF5252");







                System.Windows.MessageBox.Show($"Erreur lors du lancement : {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);







            }







        }















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















                // Restauration de la sauvegarde Vanilla de Engine.ini







                string savedConfigDir = Path.Combine(arkPath, @"ShooterGame\Saved\Config\WindowsNoEditor");







                string engineIni = Path.Combine(savedConfigDir, "Engine.ini");







                string engineBak = engineIni + ".bak";







                if (File.Exists(engineBak))







                {







                    File.Copy(engineBak, engineIni, true);







                    File.Delete(engineBak);







                    LogLine("Engine.ini restaure depuis la sauvegarde Vanilla (.bak).", "#00FF88");







                }















                // Nettoyage raccourcis bureau







                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);







                string[] shortcuts = { "ARK VR (FR).cmd", "ARK VR (FR).url", "ARK VR.url" };







                foreach (var sc in shortcuts)







                {







                    string p = Path.Combine(desktop, sc);







                    if (File.Exists(p)) { File.Delete(p); LogLine($"Raccourci '{sc}' supprime du bureau.", "#8FA7B3"); }







                }















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















    internal static class DllInjector







    {







        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]







        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);















        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]







        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);















        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]







        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);















        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]







        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);















        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]







        private static extern IntPtr GetModuleHandle(string lpModuleName);















        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]







        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);















        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]







        private static extern bool CloseHandle(IntPtr hObject);















        private const uint PROCESS_CREATE_THREAD = 0x0002;







        private const uint PROCESS_QUERY_INFORMATION = 0x0400;







        private const uint PROCESS_VM_OPERATION = 0x0008;







        private const uint PROCESS_VM_WRITE = 0x0020;







        private const uint PROCESS_VM_READ = 0x0010;







        private const uint MEM_COMMIT = 0x1000;







        private const uint MEM_RESERVE = 0x2000;







        private const uint PAGE_READWRITE = 0x04;















        public static bool Inject(int processId, string dllPath)







        {







            if (!File.Exists(dllPath)) return false;















            uint access = PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ;







            IntPtr hProcess = OpenProcess(access, false, processId);







            if (hProcess == IntPtr.Zero) return false;















            try







            {







                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(dllPath + "\0");







                IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)bytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);







                if (allocMemAddress == IntPtr.Zero) return false;















                if (!WriteProcessMemory(hProcess, allocMemAddress, bytes, (uint)bytes.Length, out _)) return false;















                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");







                if (loadLibraryAddr == IntPtr.Zero) return false;















                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);







                if (hThread == IntPtr.Zero) return false;















                CloseHandle(hThread);







                return true;







            }







            catch







            {







                return false;







            }







            finally







            {







                CloseHandle(hProcess);







            }







        }







    }







}













































