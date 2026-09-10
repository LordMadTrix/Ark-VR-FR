# 🦖 ᚨ ᚱ ᚲ   ᚡ ᚱ • ARK VR (Édition Française)

<div align="center">

<img src="https://raw.githubusercontent.com/LordMadTrix/Ark-VR-FR/main/docs/banner.png" alt="Bannière ARK VR" width="100%" />

<br/>

[![Dernière Release](https://img.shields.io/badge/Release-v1.0.0--FR-gold.svg?style=for-the-badge&logo=github)](https://github.com/LordMadTrix/Ark-VR-FR/releases)
[![Compatibilité ARK](https://img.shields.io/badge/ARK-Survival%20Evolved%20%7C%20Ascended-blue.svg?style=for-the-badge&logo=steam)](https://store.steampowered.com/app/346110/ARK_Survival_Evolved/)
[![Langue](https://img.shields.io/badge/Langue-100%25%20Fran%C3%A7ais-green.svg?style=for-the-badge)](#-3-documentation-et-profils-en-fran%C3%A7ais)
[![OpenVR / OpenXR](https://img.shields.io/badge/VR-OpenXR%20%7C%20UEVR%206DOF-orange.svg?style=for-the-badge&logo=steamvr)](https://github.com/praydog/UEVR)
[![Licence](https://img.shields.io/badge/Licence-GPL--3.0-purple.svg?style=for-the-badge)](LICENSE)

### 🦖 Vivez la survie préhistorique et Tek en totale immersion à 360° en Réalité Virtuelle native 6DOF 🦖
*Édition enrichie et francisée avec installateur graphique dédié, profils matériels (Éco, Équilibré, Ultra), suppression des nuages volumétriques bugués (+20 FPS), rendu Synced Sequential anti-scintillement et calibrage dynamique des montures de dinosaures.*

[📦 Télécharger l'Installateur (Setup .exe)](https://github.com/LordMadTrix/Ark-VR-FR/releases/latest) • [🎮 Commandes en VR](#-commandes-et-immersion-vr) • [🛠️ Guide de Compilation](#-guide-de-compilation)

</div>

---

## 🌟 Nouveautés & Fonctionnalités Clés de l'Édition Française (v1.0.0)

<table>
<tr>
<td width="50%">

### 🥽 1. Installateur Graphique Dédié (`ArkVR-Setup.exe`)
* **Design Sci-Fi Tek & Obsidienne** : Interface soignée reprenant les teintes sombres jurassiques (`#070B0E`) et les néons Tek (`#00F0FF`, `#00FF88`) avec icône exclusive.
* **Détection Automatique Multi-Disques** : Détecte instantanément votre installation Steam d'ARK (`ShooterGame.exe`).
* **Sélecteur de Profils Matériels** : Choisissez en 1 clic entre **Éco / Quest 2**, **Équilibré (Recommandé)** ou **Ultra / Mythique**.
* **Déploiement en 1 Clic** : Installe le runtime UEVR OpenXR 64 bits et pré-configure les fichiers de profil sans aucune manipulation manuelle.
* **100% Réversible** : Un bouton *Restaurer Vanilla* permet de repasser en version écran plat classique à tout moment sans toucher à vos personnages ni sauvegardes.

</td>
<td width="50%">

### 🦖 2. Immersion 6DOF & Dinosaures
* **Caméra 1ère Personne Calibrée** : Profitez d'une vue à hauteur des yeux sur les selles de vos montures (T-Rex, Raptor, Ptéranodon, Carno, Spino...).
* **Visée Découplée & 6DOF** : Visez à l'arc, arbalète ou fusil Tek indépendamment de l'orientation de votre regard.
* **Rendu Synced Sequential** : Rendu stéréoscopique corrigé pour éliminer tout artefact d'ombre ou décalage entre l'œil gauche et l'œil droit.
* **Lancement Sécurisé Sans BattlEye** : Démarrage direct avec l'argument `-NoBattlEye` pour jouer sereinement en Solo, Coop ou serveurs locaux sans risque de faux-positif anti-cheat.

</td>
</tr>
<tr>
<td width="50%">

### ⚡ 3. Optimisations Extrêmes (+20 à 30 FPS)
* **Suppression des Nuages Volumétriques** : Désactive automatiquement `r.VolumetricCloud` qui provoque une vision double désagréable en VR et fait chuter le framerate de 20 FPS.
* **Profils de Résolution Ajustés** : Résolution stéréo calibrée selon votre GPU pour assurer un 72Hz / 90Hz parfaitement fluide.
* **Culling & Ombres Contact Allégés** : Élimine les calculs d'ombres superflus dans les jungles denses.

</td>
<td width="50%">

### 🇫🇷 4. Expérience 100% en Français
* **Installateur entièrement rédigé en français** avec infobulles claires et guidage pas à pas.
* **Guides et documentation détaillée** pour un démarrage immédiat.
* **Création automatique d'un raccourci Bureau 'ARK VR (FR)'** pour relancer vos parties en 1 double-clic.

</td>
</tr>
</table>

---

## 🚀 Guide d'Installation & Démarrage

### Installation en 1 Clic (Recommandée)
1. Téléchargez **[`ArkVR-Setup.exe`](https://github.com/LordMadTrix/Ark-VR-FR/releases/latest)**.
2. Lancez l'exécutable.
3. Le chemin de votre jeu ARK est détecté automatiquement (ex: `D:\SteamLibrary\steamapps\common\ARK`).
4. Choisissez votre profil matériel (**Éco**, **Équilibré** ou **Ultra**).
5. Cliquez sur **🦖 INSTALLER ARK VR (FR)**.
6. Allumez votre casque VR, lancez **SteamVR**, puis cliquez sur **🥽 LANCER EN VR** !

> [!TIP]
> **Restauration Vanilla instantanée :** Vous souhaitez repasser sur écran plat pour une session rapide ? Ouvrez simplement l'installateur et cliquez sur **🔄 Restaurer Vanilla**. Vos sauvegardes, tribus et dinosaures restent 100% intacts !

---

## 🥽 Casques & Matériel Compatibles

Compatible avec tous les casques PCVR et autonomes connectés via SteamVR ou OpenXR :
* **Meta Quest 2, Quest 3, Quest 3S & Quest Pro** (via Quest Link câble USB-C, AirLink, Virtual Desktop ou Steam Link).
* **Valve Index** (avec support tracking 6DOF complet).
* **HTC Vive, Vive Pro, Vive Focus 3 & Cosmos**.
* **Pico 4, 4 Ultra & Neo 3** (via Pico Connect ou Virtual Desktop).
* **Casques Windows Mixed Reality (WMR)** & **Bigscreen Beyond**.

---

## 🎮 Commandes et Immersion VR

| Action en Jeu | Manette / Contrôleur VR |
| :--- | :--- |
| **Orientation & Regard** | Mouvements libres de la tête à 360° (6DOF) |
| **Visée (Armes & Outils)** | Pointez directement votre manette droite vers la cible |
| **Déplacement / Sprint** | Stick analogique gauche (clic pour sprinter) |
| **Rotation (Snap / Fluide)** | Stick analogique droit |
| **Monture de Dinosaure** | Caméra 1ère personne assise sur la selle avec contrôle au stick |
| **Menu d'Inventaire** | Panneau 3D flottant repositionnable dans l'espace VR |
| **Menu UEVR en Jeu** | Touche `Inser` du clavier pour ajuster les réglages fins à tout moment |

---

## 🛠️ Guide de Compilation

Pour les développeurs souhaitant compiler l'installateur depuis les sources :

### Prérequis
* Windows 10/11 x64
* [.NET SDK 9.0](https://dotnet.microsoft.com/download)

### Commandes
```powershell
# 1. Cloner le dépôt
git clone https://github.com/LordMadTrix/Ark-VR-FR.git
cd Ark-VR-FR

# 2. Préparer le payload autonome
pwsh -NoProfile -File prepare_payload.ps1

# 3. Compiler l'installateur autonome Single-File
dotnet publish ArkVRInstaller\ArkVRInstaller.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o SetupOutput
```

---

## 📜 Licence & Crédits

* **Moteur d'Injection UEVR** : Développé avec brio par **Praydog** ([Praydog UEVR](https://github.com/praydog/UEVR)).
* **Jeu Original** : ARK: Survival Evolved par **Studio Wildcard**.
* **Édition Française, Optimisations & Setup GUI** : Réalisé par **LordMadTrix**.
* **Licence** : Ce projet est sous licence libre **GPL-3.0**. Consultez le fichier [LICENSE](LICENSE) pour plus d'informations.

---

## 👑 Signature & Auteur Officiel

<div align="center">

<a href="https://github.com/LordMadTrix">
  <img src="https://raw.githubusercontent.com/LordMadTrix/Ark-VR-FR/main/docs/lordmadtrix_logo.png" alt="LordMadTrix Official Brand" width="180" />
</a>

### ⚡ Conçu & Forgé par **[LordMadTrix](https://github.com/LordMadTrix)** ⚡
*Architecte Systèmes • Immersion VR & Gaming • Optimisation OS & IA*

[![GitHub Profile](https://img.shields.io/badge/GitHub-LordMadTrix-181717?style=for-the-badge&logo=github)](https://github.com/LordMadTrix)
[![Édition Française](https://img.shields.io/badge/Édition-Française%20Officielle-gold?style=for-the-badge)](https://github.com/LordMadTrix/Ark-VR-FR)

*« Forger l'excellence technologique au cœur du code et de l'immersion. »*

</div>
