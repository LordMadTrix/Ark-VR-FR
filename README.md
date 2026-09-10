# 🦖 ᚨ ᚱ ᚲ   ᚡ ᚱ • ARK VR (Édition Française)

<div align="center">

<img src="https://raw.githubusercontent.com/LordMadTrix/Ark-VR-FR/main/docs/banner.png" alt="Bannière ARK VR" width="100%" />

<br/>

[![Dernière Release](https://img.shields.io/badge/Release-v1.2.0--FR-gold.svg?style=for-the-badge&logo=github)](https://github.com/LordMadTrix/Ark-VR-FR/releases)
[![Compatibilité ARK](https://img.shields.io/badge/ARK-Survival%20Evolved%20%7C%20Ascended-blue.svg?style=for-the-badge&logo=steam)](https://store.steampowered.com/app/346110/ARK_Survival_Evolved/)
[![Langue](https://img.shields.io/badge/Langue-100%25%20Fran%C3%A7ais-green.svg?style=for-the-badge)](#-4-documentation-et-scripts-en-fran%C3%A7ais)
[![OpenVR / OpenXR](https://img.shields.io/badge/VR-OpenXR%20%7C%20UEVR%206DOF-orange.svg?style=for-the-badge&logo=steamvr)](https://github.com/praydog/UEVR)
[![Licence](https://img.shields.io/badge/Licence-GPL--3.0-purple.svg?style=for-the-badge)](LICENSE)

### 🦖 Vivez la survie préhistorique et Tek en totale immersion à 360° en Réalité Virtuelle native 6DOF 🦖
*Édition enrichie et francisée avec installateur graphique dédié, simulateur 3D interactif en temps réel, scripts automatisés en 1 clic, montures volantes et terrestres, suppression des nuages volumétriques bugués (+20 FPS), rendu Synced Sequential anti-scintillement et visée laser 3D.*

[📦 Télécharger l'Installateur (Setup .exe)](https://github.com/LordMadTrix/Ark-VR-FR/releases/latest) • [🌐 Tester le Simulateur 3D en Ligne](http://localhost:8080/) • [🎮 Commandes en VR](#-commandes-et-immersion-vr)

</div>

---

## 🌟 Nouveautés Majeures de la Version v1.2.0

<table>
<tr>
<td width="50%">

### 🥽 1. Installateur Graphique Amélioré (`ArkVR-Setup.exe`)
* **Design Sci-Fi Tek & Obsidienne** : Interface moderne (`#070B0E`, `#00F0FF`, `#00FF88`) avec icône applicative sur mesure.
* **Bouton d'Accès Direct au Simulateur** : Lancez le simulateur 3D interactif en un clic directement depuis l'interface de l'installateur !
* **Priorité Processeur Élevée (CPU High)** : Option pour forcer la priorité du jeu en temps réel pour éradiquer les micro-saccades en VR.
* **Détection Automatique Multi-Disques** : Détecte instantanément `D:\SteamLibrary\steamapps\common\ARK` (`ShooterGame.exe`).
* **Sélecteur de Profils Matériels** : **Éco / Quest 2** (72 FPS), **Équilibré (Recommandé)** (90 FPS), ou **Ultra / Mythique**.
* **100% Réversible** : Bouton *Restaurer Vanilla* pour revenir à l'état d'origine sans altérer vos sauvegardes.

</td>
<td width="50%">

### ⚡ 2. Scripts d'Automatisation en 1 Clic
* **`Installer-ArkVR-FR.cmd`** : Déploiement silencieux sans avoir à passer par l'interface graphique.
* **`Lancer-ArkVR-FR.cmd`** : Lance automatiquement l'injecteur UEVR, SteamVR et le jeu avec l'argument `-NoBattlEye` en priorité élevée.
* **`Desinstaller-ArkVR-FR.cmd`** : Nettoyage propre et retour à la version classique écran plat en 1 seconde.

</td>
</tr>
<tr>
<td width="50%">

### 🦅 3. Montures 6DOF & Dinosaures
* **Vol en Ptéranodon Réaliste** : Le profil UEVR compense et applique un roulis dynamique 6DOF dans les virages aériens.
* **Selle de T-Rex Calibrée** : Caméra calée à la hauteur des yeux du cavalier sans obstruction de vue.
* **Vignette de Confort Anti-Cinétose** : Assombrissement progressif des bords de vision lors des sprints et piqués pour éliminer le mal des transports.
* **Viseur Laser Tek 3D** : Faisceau laser émeraude pour viser à l'instinct aux manettes.

</td>
<td width="50%">

### 🌐 4. Simulateur 3D Temps Réel Déployé
* **Prévisualisation Stéréoscopique Immédiate** : Affiche en direct le rendu Side-by-Side (SBS) des lentilles Quest/Index sur votre PC avec écart pupillaire IPD calibré (64 mm).
* **Vol Aérien Interactif** : Testez le vol en Ptéranodon et la selle de T-Rex directement dans votre navigateur sur `http://localhost:8080/`.
* **Cycle Jour / Nuit & Audio Spatialisé** : Obélisque Tek bioluminescent nocturne et effets sonores synthétisés hors-ligne.

</td>
</tr>
</table>

---

## 🚀 Méthodes d'Installation

### Méthode A : Via l'Installateur Graphique (Recommandée)
1. Téléchargez et lancez **[`ArkVR-Setup.exe`](https://github.com/LordMadTrix/Ark-VR-FR/releases/latest)**.
2. Choisissez votre profil matériel (**Éco**, **Équilibré** ou **Ultra**).
3. Cliquez sur **🦖 INSTALLER ARK VR (FR)**.
4. Cliquez sur **🥽 LANCER EN VR** !

### Méthode B : Via les Scripts Directs
* **Installation :** Double-cliquez sur `Installer-ArkVR-FR.cmd`.
* **Lancement :** Double-cliquez sur `Lancer-ArkVR-FR.cmd`.
* **Restauration :** Double-cliquez sur `Desinstaller-ArkVR-FR.cmd`.

---

## 🎮 Commandes et Immersion VR

| Action en Jeu | Manette / Contrôleur VR |
| :--- | :--- |
| **Orientation & Regard** | Mouvements libres de la tête à 360° (6DOF) |
| **Visée (Armes & Outils)** | Pointez directement votre manette droite avec faisceau laser 3D |
| **Déplacement / Sprint** | Stick analogique gauche (clic pour sprinter avec vignette de confort) |
| **Rotation (Snap / Fluide)** | Stick analogique droit |
| **Monture Terrestre (T-Rex)** | Caméra 1ère personne calée sur la selle du dinosaure |
| **Monture Volante (Ptéranodon)** | Vol aérien 6DOF avec roulis dynamique dans les virages |
| **Menu d'Inventaire** | Panneau 3D flottant repositionnable dans l'espace VR |
| **Menu UEVR en Jeu** | Touche `Inser` du clavier pour ajuster les réglages fins à tout moment |

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
