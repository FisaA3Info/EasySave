# EasySave - Logiciel de Sauvegarde

![Version](https://img.shields.io/badge/version-1.0-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## Description

**EasySave** est un logiciel de sauvegarde permettant de créer et gérer jusqu'à 5 tâches de backup simultanées. Il supporte deux types de sauvegarde (complète et différentielle) avec suivi en temps réel de la progression et journalisation quotidienne automatique.

### Fonctionnalités principales

- ✅ Création et gestion de tâches de sauvegarde (max 5)
- ✅ Deux types de backup : **Complet** et **Différentiel**
- ✅ Suivi en temps réel avec barre de progression
- ✅ Logs quotidiens automatiques (JSON)
- ✅ Fichier d'état pour traçabilité
- ✅ Interface multilingue (Français/Anglais)
- ✅ Architecture MVVM extensible

---

## Installation

### Prérequis

- **.NET 8.0 SDK** ou supérieur
- **Windows** / **Linux** / **macOS**

### Cloner le projet
```bash
git clone https://github.com/votre-username/EasySave.git
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

### Exécution avec barre de progression
```
  [█████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░] 47% /
  Files: 142/302 | Size: 2.34 GB/4.98 GB
  Current: test.xlsx
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

### 2. Système de logs

Chaque opération de copie génère une entrée dans un fichier JSON quotidien :

**Structure du log :**
```json
{
    "Timestamp": "2025-02-05 14:32:18",
    "BackupName": "Mes_Documents",
    "SourcePath": "C:\\Users\\John\\Documents\\file.txt",
    "TargetPath": "D:\\Backups\\Documents\\file.txt",
    "FileSize": 2048576,
    "TransferTimeMs": 145
}
```

**Emplacement :** 
`Windows: \AppData\Roaming\EasySave\DailyLog` 

`Linux: $HOME/.config/EasySave\DailyLog` 

`Unix: $HOME/.config/EasySave\DailyLog`

### 3. Fichier d'état (State File)

Conserve l'état de chaque backup en cours :
```json
{
    "JobName": "Mes_Documents",
    "Timestamp": "2025-02-05T14:32:18",
    "State": "Active",
    "TotalFiles": 302,
    "TotalSize": 5234567890,
    "Progress": 47,
    "FilesRemaining": 160,
    "SizeRemaining": 2774567890,
    "CurrentSourceFile": "C:\\Users\\John\\Documents\\rapport.xlsx",
    "CurrentTargetFile": "D:\\Backups\\Documents\\rapport.xlsx"
}
```

**Emplacement :** 
Windows: \AppData\Roaming\EasySave 

Linux: $HOME/.config/EasySave

Unix: $HOME/.config/EasySave

---

## Design Patterns utilisés

| Pattern | Utilisation |
|---------|-------------|
| **MVVM** | Séparation View/ViewModel/Model |
| **Strategy** | Stratégies de backup interchangeables |
| **Observer** | Notification en temps réel (StateTracker) |
| **Singleton** (optionnel) | Logger unique |

---

## Technologies

- **Langage :** C# 12
- **Framework :** .NET 8.0
- **Sérialisation :** System.Text.Json
- **Architecture :** MVVM + Strategy + Observer

---

## Roadmap

### Version 1.0 (Actuelle) 
- [x] Interface console
- [x] Backup complet et différentiel
- [x] Logs quotidiens
- [x] Multilingue (FR/EN)

### Version 2.0 (En cours) 
- [ ] Interface graphique WPF/XAML
- [ ] INotifyPropertyChanged pour binding
- [ ] Gestion priorités des jobs
- [ ] Filtres d'extension de fichiers
- [ ] Encryption des backups

### Version 3.0 (Futur) 
- [ ] Sauvegarde incrémentale
- [ ] Cloud storage (OneDrive, Google Drive)
- [ ] Planification automatique (cron jobs)
- [ ] Notifications par email

---

##  Contributeurs

- **AIH TAHT Fayçal** - Développeur
- **COZETTE Flora** - Développeur
- **GENCLER Melih** - Développeur
- **PRETRE Lukas** - Développeur
- **VAN CAMP Théotime** - Développeur
- 

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
