using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace ArkVRInstaller
{
    public partial class MainWindow : Window
    {
        private string? _detectedArkPath;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DetectArkInstallation();
        }

        private void DetectArkInstallation()
        {
            string[] possiblePaths = new[]
            {
                @"D:\SteamLibrary\steamapps\common\ARK",
                @"C:\Program Files (x86)\Steam\steamapps\common\ARK",
                @"C:\SteamLibrary\steamapps\common\ARK",
                @"E:\SteamLibrary\steamapps\common\ARK",
                @"F:\SteamLibrary\steamapps\common\ARK"
            };

            foreach (var path in possiblePaths)
            {
                if (IsValidArkFolder(path))
                {
                    _detectedArkPath = path;
                    TxtGamePath.Text = path;
                    TxtDetectStatus.Text = "✅ ARK: Survival Evolved détecté avec succès !";
                    TxtDetectStatus.Foreground = System.Windows.Media.Brushes.LightGreen;
                    return;
                }
            }

            TxtDetectStatus.Text = "⚠️ ARK non détecté automatiquement. Cliquez sur 'Parcourir...'";
            TxtDetectStatus.Foreground = System.Windows.Media.Brushes.Orange;
        }

        private bool IsValidArkFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return false;
            string exe = Path.Combine(path, @"ShooterGame\Binaries\Win64\ShooterGame.exe");
            return File.Exists(exe);
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Sélectionnez le dossier principal de votre jeu ARK",
                InitialDirectory = Directory.Exists(TxtGamePath.Text) ? TxtGamePath.Text : @"C:\"
            };

            if (dialog.ShowDialog() == true)
            {
                string chosen = dialog.FolderName;
                if (IsValidArkFolder(chosen))
                {
                    _detectedArkPath = chosen;
                    TxtGamePath.Text = chosen;
                    TxtDetectStatus.Text = "✅ Dossier ARK valide !";
                    TxtDetectStatus.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
                else
                {
                    MessageBox.Show(
                        "Le dossier sélectionné ne contient pas ShooterGame\\Binaries\\Win64\\ShooterGame.exe.\nVérifiez qu'il s'agit bien du dossier 'ARK' de votre bibliothèque Steam.",
                        "Dossier invalide",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
        }

        private async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            string targetPath = TxtGamePath.Text.Trim();
            if (!IsValidArkFolder(targetPath))
            {
                MessageBox.Show(
                    "Veuillez sélectionner un dossier ARK valide contenant ShooterGame.exe avant de lancer l'installation.",
                    "Erreur de dossier",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            BtnInstall.IsEnabled = false;
            BtnLaunch.IsEnabled = false;
            BtnRestore.IsEnabled = false;

            try
            {
                await Task.Run(() => PerformInstallation(targetPath));
                MessageBox.Show(
                    "🎉 Félicitations ! ARK VR (Édition Française v1.2.0) a été installé et configuré avec succès !\n\nOptions actives :\n- Rendu Stéréoscopique Synced Sequential\n- Montures Dinosaures 6DOF (T-Rex & Ptéranodon)\n- Viseur Laser Tek 3D et Vignette Anti-Cinétose\n- Optimisations FPS sans nuages bugués\n\nVous pouvez désormais allumer votre casque VR et cliquer sur '🥽 LANCER EN VR'.",
                    "Installation réussie",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Une erreur est survenue lors du déploiement :\n{ex.Message}",
                    "Erreur d'installation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                BtnInstall.IsEnabled = true;
                BtnLaunch.IsEnabled = true;
                BtnRestore.IsEnabled = true;
            }
        }

        private void PerformInstallation(string arkPath)
        {
            UpdateStatus("Extraction des modules VR et runtime UEVR...", 15);

            string win64Dir = Path.Combine(arkPath, @"ShooterGame\Binaries\Win64");
            string uevrDestDir = Path.Combine(win64Dir, "uevr");
            Directory.CreateDirectory(uevrDestDir);

            // 1. Extraire le payload embarqué
            string tempZip = Path.Combine(Path.GetTempPath(), $"ArkVRPayload_{Guid.NewGuid():N}.zip");
            string tempExtract = Path.Combine(Path.GetTempPath(), $"ArkVRPayload_{Guid.NewGuid():N}");

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream? stream = assembly.GetManifestResourceStream("ArkVRInstaller.payload.zip"))
                {
                    if (stream == null)
                    {
                        throw new InvalidOperationException("La ressource interne payload.zip est introuvable.");
                    }
                    using (FileStream fs = new FileStream(tempZip, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                    }
                }

                ZipFile.ExtractToDirectory(tempZip, tempExtract, true);

                UpdateStatus("Copie des binaires UEVR OpenXR...", 40);

                // Copie des fichiers UEVR dans ShooterGame\Binaries\Win64\uevr
                string payloadUevr = Path.Combine(tempExtract, "uevr");
                if (Directory.Exists(payloadUevr))
                {
                    CopyDirectory(payloadUevr, uevrDestDir);
                }

                UpdateStatus("Configuration du profil 6DOF, Dinosaures & Laser...", 65);

                // 2. Déployer le profil UEVR dans %APPDATA%\uevr\profiles\ShooterGame\
                string appDataUevr = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"uevr\profiles\ShooterGame");
                Directory.CreateDirectory(appDataUevr);

                string configSource = Path.Combine(tempExtract, @"config\ShooterGame_config.txt");
                if (File.Exists(configSource))
                {
                    string configContent = File.ReadAllText(configSource);

                    // Ajustements selon les choix de l'utilisateur
                    Dispatcher.Invoke(() =>
                    {
                        if (ChkSyncedSequential.IsChecked == false)
                        {
                            configContent = configContent.Replace("VR_RenderingMethod=1", "VR_RenderingMethod=0");
                        }
                        if (ChkVignette.IsChecked == false)
                        {
                            configContent = configContent.Replace("VR_ComfortVignette=1", "VR_ComfortVignette=0");
                        }
                        if (ChkLaserSight.IsChecked == false)
                        {
                            configContent = configContent.Replace("VR_LaserPointerEnabled=1", "VR_LaserPointerEnabled=0");
                        }
                        if (ChkPteroRoll.IsChecked == false)
                        {
                            configContent = configContent.Replace("VR_FlyingMountBankRoll=1", "VR_FlyingMountBankRoll=0");
                        }
                    });

                    File.WriteAllText(Path.Combine(appDataUevr, "config.txt"), configContent);
                }

                UpdateStatus("Application des optimisations graphiques VR...", 85);

                // 3. Injecter les optimisations dans Engine.ini
                string savedConfigDir = Path.Combine(arkPath, @"ShooterGame\Saved\Config\WindowsNoEditor");
                Directory.CreateDirectory(savedConfigDir);
                string engineIniPath = Path.Combine(savedConfigDir, "Engine.ini");

                bool disableClouds = false;
                Dispatcher.Invoke(() => disableClouds = ChkDisableClouds.IsChecked == true);

                if (disableClouds)
                {
                    ApplyEngineTweaks(engineIniPath);
                }

                // 4. Raccourcis Bureau et scripts d'automatisation
                bool createShortcut = false;
                Dispatcher.Invoke(() => createShortcut = ChkDesktopShortcut.IsChecked == true);
                if (createShortcut)
                {
                    CreateDesktopShortcuts(arkPath);
                }

                UpdateStatus("✅ Installation terminée et prête pour la Réalité Virtuelle !", 100);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempZip)) File.Delete(tempZip);
                    if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true);
                }
                catch { }
            }
        }

        private void ApplyEngineTweaks(string engineIniPath)
        {
            string tweaks = "\n[SystemSettings]\nr.VolumetricCloud=0\nr.TrueSkyQuality=0\nr.ShadowQuality=2\nr.ContactShadows=0\nr.LightShaftQuality=0\nr.BloomQuality=1\nr.MotionBlurQuality=0\nr.DepthOfFieldQuality=0\nr.ViewDistanceScale=1.2\nr.Streaming.PoolSize=4096\n";
            if (File.Exists(engineIniPath))
            {
                string existing = File.ReadAllText(engineIniPath);
                if (!existing.Contains("r.VolumetricCloud"))
                {
                    File.AppendAllText(engineIniPath, tweaks);
                }
            }
            else
            {
                File.WriteAllText(engineIniPath, tweaks);
            }
        }

        private void CreateDesktopShortcuts(string arkPath)
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktop, "ARK VR (FR).cmd");
                string launchBat = $"@echo off\ntitle ARK VR - LordMadTrix\ncd /d \"{arkPath}\\ShooterGame\\Binaries\\Win64\"\nstart \"\" \"uevr\\UEVRInjector.exe\"\nstart /high \"\" \"ShooterGame.exe\" -NoBattlEye\nexit\n";
                File.WriteAllText(shortcutPath, launchBat);
            }
            catch { }
        }

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            string arkPath = TxtGamePath.Text.Trim();
            if (!IsValidArkFolder(arkPath))
            {
                MessageBox.Show("Dossier ARK invalide. Veuillez vérifier le chemin.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string win64 = Path.Combine(arkPath, @"ShooterGame\Binaries\Win64");
                string injector = Path.Combine(win64, @"uevr\UEVRInjector.exe");
                string gameExe = Path.Combine(win64, "ShooterGame.exe");

                if (File.Exists(injector))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = injector,
                        WorkingDirectory = Path.GetDirectoryName(injector)!,
                        UseShellExecute = true
                    });
                }

                bool highPriority = ChkCpuPriority.IsChecked == true;

                var startInfo = new ProcessStartInfo
                {
                    FileName = gameExe,
                    Arguments = "-NoBattlEye",
                    WorkingDirectory = win64,
                    UseShellExecute = true
                };

                var proc = Process.Start(startInfo);
                if (proc != null && highPriority)
                {
                    try { proc.PriorityClass = ProcessPriorityClass.High; } catch { }
                }

                UpdateStatus("🥽 Lancement d'ARK en Réalité Virtuelle en cours...", 100);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du lancement : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            string arkPath = TxtGamePath.Text.Trim();
            if (!IsValidArkFolder(arkPath)) return;

            var result = MessageBox.Show(
                "Voulez-vous restaurer ARK en mode classique écran plat (Vanilla) ?\nVos sauvegardes, mondes et dinosaures ne seront absolument pas touchés.",
                "Confirmer la restauration",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string uevrDir = Path.Combine(arkPath, @"ShooterGame\Binaries\Win64\uevr");
                    if (Directory.Exists(uevrDir)) Directory.Delete(uevrDir, true);

                    string appDataUevr = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"uevr\profiles\ShooterGame");
                    if (Directory.Exists(appDataUevr)) Directory.Delete(appDataUevr, true);

                    UpdateStatus("🔄 Restauration Vanilla effectuée avec succès !", 0);
                    MessageBox.Show("ARK a été restauré en configuration Vanilla d'origine.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la restauration : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateStatus(string message, int percent)
        {
            Dispatcher.Invoke(() =>
            {
                TxtStatus.Text = message;
                ProgressBar.Value = percent;
                TxtProgressPercent.Text = $"{percent}%";
            });
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) return;

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }
    }
}

