# EasySave - Logiciel de Sauvegarde

![Version](https://img.shields.io/badge/version-3.0-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## Description

**EasySave** est un logiciel de sauvegarde permettant de créer et gérer jusqu'à 5 tâches de backup simultanées. Il supporte deux types de sauvegarde (complète et différentielle) avec suivi en temps réel de la progression, journalisation quotidienne automatique, et depuis la version 3.0, une exécution parallèle via Threads/Tasks, une gestion de fichiers prioritaires, des commandes de contrôle Play/Pause/Stop, la détection de logiciels métier, et une centralisation des logs via Docker.

### Fonctionnalités principales

- ✅ Création et gestion de tâches de sauvegarde (max 5)
- ✅ Deux types de backup : **Complet** et **Différentiel**
- ✅ Suivi en temps réel avec barre de progression
- ✅ Logs quotidiens automatiques (JSON)
- ✅ Fichier d'état pour traçabilité
- ✅ Interface multilingue (Français/Anglais)
- ✅ Architecture MVVM extensible
- ✅ **[v3.0]** Exécution parallèle via Threads/Tasks
- ✅ **[v3.0]** Gestion des fichiers prioritaires (extensions configurables)
- ✅ **[v3.0]** Commandes Play / Pause / Stop sur chaque job
- ✅ **[v3.0]** Détection de logiciel métier (blocage automatique)
- ✅ **[v3.0]** Centralisation des logs via Docker (serveur de logs distant)
- ✅ **[v3.0]** Choix de destination des logs : local, centralisé, ou les deux

---

## Installation

### Prérequis

- **.NET 8.0 SDK** ou supérieur
- **Windows** / **Linux** / **macOS**
- **Docker** (optionnel, pour la centralisation des logs)

### Cloner le projet
```bash
git clone https://github.com/FisaA3Info/EasySave.git
cd EasySave
```

### Compiler le projet
```bash
dotnet build
```

### Exécuter l'application
```bash
dotnet run
```

### Démarrer le serveur de logs Docker (optionnel)
```bash
docker-compose up -d --build
```

---

## Utilisation

### Menu principal
```
1. Create backup job
2. View all jobs
3. Execute a job
4. Execute all jobs
5. Delete a job
6. Change language
7. Settings
0. Exit
```

### Créer une tâche de backup
```
Enter job name: Mes_Documents
Enter source directory: C:\Users\John\Documents
Enter target directory: D:\Backups\Documents
Select backup type:
  1. Full backup
  2. Differential backup
Choice: 1

✓ Backup job created successfully!
```

### Exécution avec barre de progression et contrôles
```
  [█████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░] 47% /
  Files: 142/302 | Size: 2.34 GB/4.98 GB
  Current: rapport.xlsx  
```

---

## Fonctionnalités détaillées

### 1. Types de sauvegarde

#### Sauvegarde complète (Full Backup)
- Copie **tous les fichiers** du répertoire source
- Écrase les fichiers existants dans la cible
- Idéal pour une première sauvegarde

#### Sauvegarde différentielle (Differential Backup)
- Copie uniquement les fichiers **modifiés ou nouveaux**
- Compare les dates de modification
- Plus rapide pour les mises à jour

---

### 2. [v3.0] Exécution parallèle via Threads/Tasks

Chaque tâche de backup s'exécute dans un **Thread/Task dédié**, permettant de lancer plusieurs jobs simultanément sans bloquer l'interface.

- Chaque job est encapsulé dans un `Task` (ou `Thread`) géré par un `BackupJobRunner`
- Les accès concurrents aux ressources partagées (logs, fichier d'état) sont protégés par des **mutex/locks**
- Un `CancellationToken` permet d'interrompre proprement chaque job

```
Job 1 [Mes_Documents]  ████████████░░░░ 65%  [Running]
Job 2 [Photos_2024]    ██░░░░░░░░░░░░░░ 12%  [Running]
Job 3 [Projets_Dev]    ████████████████ 100% [Done]
```

---

### 3. [v3.0] Gestion des fichiers prioritaires

Certaines extensions de fichiers peuvent être marquées comme **prioritaires** dans les paramètres. Lors de l'exécution d'un job, ces fichiers sont copiés **en premier**, avant les autres.

**Configuration dans les paramètres :**
```
Priority extensions: .xlsx, .docx, .pdf
```

**Comportement :**
- Avant de commencer la copie standard, EasySave scanne la source et identifie les fichiers prioritaires
- Ces fichiers sont traités en tête de file
- Un indicateur `[PRIORITY]` est visible dans la barre de progression

---

### 4. [v3.0] Commandes Play / Pause / Stop

Chaque job en cours d'exécution peut être contrôlé individuellement :

| Commande | Comportement |
|----------|-------------|
| **Play / Resume** | Lance ou reprend un job mis en pause |
| **Pause** | Suspend l'exécution après le fichier en cours (pas de corruption) |
| **Stop** | Annule le job proprement via `CancellationToken` |

Ces commandes sont accessibles depuis l'interface en temps réel et via les raccourcis clavier `[P]`, `[R]`, `[S]` pendant l'affichage de la progression.

---

### 5. [v3.0] Détection de logiciel métier

EasySave peut détecter si un **logiciel métier** configuré est en cours d'exécution sur la machine. Si c'est le cas, **tous les jobs actifs sont automatiquement mis en pause** jusqu'à ce que le logiciel se ferme.

**Configuration dans les paramètres :**
```
Business software: CalculPaie.exe, SAP.exe
```

**Comportement :**
- Un thread de surveillance vérifie périodiquement la liste des processus actifs
- Si un logiciel métier est détecté, les jobs en cours sont mis en pause
- Dès que le logiciel se ferme, les jobs reprennent automatiquement
- Un message est affiché dans la console : `⚠ Business software detected: CalculPaie.exe — Jobs paused`

---

### 6. [v3.0] Centralisation des logs via Docker

EasySave supporte l'envoi des logs vers un **serveur de logs distant** conteneurisé avec Docker. Cela permet de centraliser les journaux de plusieurs instances d'EasySave sur différentes machines.

#### Architecture

```
[EasySave Instance 1] ──┐
[EasySave Instance 2] ──┼──► [Docker Log Server] ──► Logs centralisés (JSON)
[EasySave Instance 3] ──┘
```

#### Démarrage du serveur Docker
```bash
docker-compose up -d log-server
```

#### Configuration du serveur dans `appsettings.json`
```C#
{
    "Log_Server_Log": "localhost:5000",
    "Machine_Name": "Computer",
    "User_Name":"Bob"
}
```

Le serveur de logs écoute sur un socket TCP/UDP et reçoit les entrées JSON envoyées par les clients EasySave.

---

### 7. [v3.0] Choix de destination des logs

Dans les paramètres, l'utilisateur peut choisir **où envoyer les logs** parmi trois options :

| Option | Description |
|--------|-------------|
| **Local** | Les logs sont écrits uniquement en local (comportement v1/v2) |
| **Centralisé** | Les logs sont envoyés uniquement au serveur Docker distant |
| **Les deux** | Les logs sont écrits localement ET envoyés au serveur distant |

**Configuration dans les paramètres :**
```
Log destination:
  1. Local only
  2. Remote (Docker server) only
  3. Both (local + remote)
Choice: 3
```

---

### 8. Système de logs

Chaque opération de copie génère une entrée dans un fichier JSON quotidien :

**Structure du log :**
```json
{
  "TimeStamp": "2026-02-26T02:34:47.0861443+01:00",
  "BackupName": "TestTO",
  "SourcePath": "C:\\Riot Games\\League of Legends\\api-ms-win-core-console-l1-1-0.dll",
  "TargetPath": "C:\\Test\\League of Legends\\api-ms-win-core-console-l1-1-0.dll",
  "FileSize": 12240,
  "TransferTimeMs": 3,
  "EncryptionTimeMs": 11,
  "MachineName": "ASUS-TUF",
  "UserName": "Flora"
}
```

**Emplacement local :**
`Windows: \AppData\Roaming\EasySave\DailyLog`
`Linux/macOS: $HOME/.config/EasySave/DailyLog`

---

### 9. Fichier d'état (State File)

Conserve l'état de chaque backup en cours :
```json
{
  "JobName": "Mes_Documents",
  "TimeStamp": "2026-02-24T12:09:56.0614434+01:00",
  "State": "Paused",
  "TotalFiles": 739,
  "TotalSize": 36337364691,
  "Progress": 33,
  "FilesRemaining": 492,
  "SizeRemaining": 25155704104,
  "CurrentSourceFile": "C:\\Users\\John\\Documents\\rapport.xlsx",
  "CurrentTargetFile": "D:\\Backups\\Documents\\rapport.xlsx",
}
```

**Emplacement :**
`Windows: \AppData\Roaming\EasySave`
`Linux/macOS: $HOME/.config/EasySave`

---

## Design Patterns utilisés

| Pattern | Utilisation |
|---------|-------------|
| **MVVM** | Séparation View/ViewModel/Model |
| **Strategy** | Stratégies de backup interchangeables |
| **Observer** | Notification en temps réel (StateTracker) |
| **Singleton** | Logger unique, LogDispatcher |
| **Producer/Consumer** | File de priorité pour les fichiers à copier |
| **Command** | Commandes Play/Pause/Stop encapsulées |

---

## Technologies

- **Langage :** C# 12
- **Framework :** .NET 8.0
- **Sérialisation :** System.Text.Json
- **Parallélisme :** `System.Threading.Tasks`, `CancellationToken`, `Mutex`
- **Architecture :** MVVM + Strategy + Observer + Command
- **Conteneurisation :** Docker (serveur de logs centralisé)

---

## Roadmap

### Version 1.0 
- [x] Interface console
- [x] Backup complet et différentiel
- [x] Logs quotidiens
- [x] Multilingue (FR/EN)

### Version 2.0 
- [x] Interface graphique WPF/XAML
- [x] INotifyPropertyChanged pour binding
- [x] Gestion priorités des jobs
- [x] Filtres d'extension de fichiers
- [x] Encryption des backups

### Version 3.0 (En cours) 
- [x] Exécution parallèle via Threads/Tasks
- [x] Gestion des fichiers prioritaires (extensions configurables)
- [x] Commandes Play / Pause / Stop
- [x] Détection de logiciel métier (pause automatique)
- [x] Serveur de logs centralisé via Docker
- [x] Choix de destination des logs (local / centralisé / les deux)

### Version 4.0 (Futur) 
- [ ] Sauvegarde incrémentale
- [ ] Cloud storage (OneDrive, Google Drive)
- [ ] Planification automatique (cron jobs)
- [ ] Notifications par email

---

## Contributeurs

- **AÏT TAHT Fayçal** - Développeur
- **COZETTE Flora** - Développeur
- **GENCLER Melih** - Développeur
- **PRETRE Lukas** - Développeur
- **VAN CAMP Théotime** - Développeur

---

### .gitignore
```
# Build results
bin/
obj/
*.dll
*.exe

# Visual Studio
.vs/
*.user
*.suo

# OS files
.DS_Store
Thumbs.db

# Docker
docker-compose.override.yml
```
