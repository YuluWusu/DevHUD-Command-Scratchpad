# ⚡ DevHUD — Tactical Command Scratchpad & Task Runner

<p align="center">
  <img src="DevHUD - Command Scratchpad/DevHUD - Command Scratchpad/Resources/app_icon.png" alt="DevHUD Logo" width="128" height="128" />
</p>

<p align="center">
  <strong>A keyboard-driven, cyberpunk tactical HUD scratchpad, micro-Pomodoro tracker, and quick-launch task runner engineered for high-velocity developers.</strong>
</p>

<p align="center">
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-8.0-blue.svg?style=flat-square" alt=".NET 8.0" /></a>
  <a href="https://www.microsoft.com/windows"><img src="https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6.svg?style=flat-square" alt="Windows" /></a>
  <a href="#"><img src="https://img.shields.io/badge/UI-WPF%20Acrylic%20Cyberpunk-00F0FF.svg?style=flat-square" alt="WPF Acrylic" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-green.svg?style=flat-square" alt="License MIT" /></a>
  <a href="#"><img src="https://img.shields.io/badge/Architecture-MVVM%20Toolkit-8A2BE2.svg?style=flat-square" alt="MVVM" /></a>
</p>

---

## 📖 Introduction

**DevHUD** transforms the conventional notepad and to-do list into a sleek, zero-friction developer command center. Inspired by cyberpunk tactical interfaces and high-performance terminal workflows, DevHUD stays out of your way until you need it, and executes actions at the speed of thought.

Whether you're debugging deep in an IDE, grinding LeetCode algorithms, or managing daily dev tasks, DevHUD provides a transparent, hotkey-accessible heads-up display directly on your screen.

---

## ⚡ Core Capabilities

### 1. 📝 Zero-Friction Plain-Text Engine
- Write notes and manage tasks in raw, natural plain text with syntax highlighting.
- **Debounced Auto-Save:** Automatically persists changes 600ms after typing stops—never lose a keystroke even during unexpected shutdowns.
- Tasks are simply bullet points (`- Task name`). Toggle completion with `Ctrl + Enter` (appends `(done)` or `(hoàn thành)` with subtle strike-through and dimmed styling).

### 2. ⏱️ Inline Micro-Pomodoro Deep Work Engine
- Hit **`F5`** on any task line to attach an inline countdown timer `[⏳ 25:00]`.
- As you work, the timer ticks down directly inside your document and in the title bar pill.
- Complete the task early via `Ctrl + Enter` to record elapsed minutes automatically (e.g., `(done - 18m)`).
- Distinct auditory cues for focus completion and milestones.

### 3. 🚀 Quick-Launch Tags (`Ctrl + Space`)
Place cursor on any line and press `Ctrl + Space` to trigger actions:
- **`@web:https://...`** or raw URLs — Instantly opens the URL in your default browser.
- **`@path:D:\Projects\...`** — Opens folders or files in Windows File Explorer.
- **`@cmd:...`** or **`@run:...`** — Executes commands directly via terminal.

### 4. 👻 Tactical Ghost HUD Mode (`Ctrl + T`)
- Transforms the application into a minimalist, 75% translucent overlay pinned to the corner of your screen.
- Filters out notes and completed tasks, spotlighting **only remaining uncompleted action items**.
- Perfect as an ambient companion display while coding in fullscreen IDEs.

### 5. 🔥 Daily Habit Auto-Respawn & Streak Tracker
- Lines marked with **`- [H]`** represent recurring daily habits.
- When you launch DevHUD on a new day, completed habits automatically respawn as uncompleted tasks.
- If you completed the habit on consecutive days, your streak counter advances automatically (e.g., `[🔥 5 days]`).

### 6. 🌐 Bilingual Engine (English & Vietnamese)
- Instant, real-time language switching via **`Ctrl + L`** or the **`🌐 VI / EN`** title bar toggle.
- UI labels, status logs, tooltips, and completion tags dynamically adapt between English and Vietnamese without restarting.

### 7. 🖥️ Global Win32 Hotkey (`Alt + Q`)
- Summon or dismiss DevHUD anywhere across Windows—even while inside a game, browser, or code editor.

---

## ⌨️ Tactical Keyboard Cheat-Sheet

| Hotkey | Action | Description |
| :--- | :--- | :--- |
| `Ctrl + Enter` | **Toggle Task** | Mark task as completed `(done)` or revert to uncompleted |
| `F5` | **Micro-Pomodoro** | Start / Pause 25-minute inline focus timer on the current line |
| `Ctrl + Space` | **Quick-Launch** | Open `@web:` URL, `@path:` directory, or execute `@cmd:` |
| `Ctrl + Shift + S` | **Tactical Sort** | Move all completed tasks to bottom under a `# COMPLETED` header |
| `Ctrl + T` | **Ghost HUD** | Toggle 75% transparent, always-on-top pinned overlay mode |
| `Ctrl + L` | **Toggle Language** | Switch between English (`EN`) and Vietnamese (`VI`) |
| `Ctrl + H` | **Cheat-Sheet** | Toggle built-in tactical shortcuts and syntax guide overlay |
| `Ctrl + N` | **New Tab** | Create a new scratchpad tab |
| `Ctrl + 1..9` | **Switch Tab** | Jump directly to Tab 1 through 9 |
| `Ctrl + Tab` | **Cycle Tabs** | Cycle through open tabs sequentially |
| `Alt + Q` | **Global Summon** | Summon or hide DevHUD from anywhere in Windows |
| `Esc` | **Dismiss** | Close active modals (Help, dialogs) |

---

## 📐 Intelligent Syntax Guide

DevHUD utilizes a lightweight, human-readable syntax that colorizes dynamically:

```markdown
# ⚡ SPRINT TASKS & FOCUS
- Review Pull Request #42 @web:https://github.com/org/repo/pull/42
- [H] Solve daily LeetCode challenge [🔥 4 days]
- Build API integration module (done - 25m)
- Open local repository @path:D:\Work\DevHUD
- Run local unit tests @cmd:dotnet test

# 📌 QUICK NOTES
- Architecture discussion notes:
  - Use JWT tokens with refresh mechanism
  - Cache response in Redis
```

---

## 📥 Installation & Running

### Option 1: Standalone Portable Binary (Recommended)
DevHUD is distributed as a **self-contained, zero-dependency portable executable**.

1. Download the latest release from the [Releases](https://github.com/your-username/DevHUD/releases) page.
2. Extract the archive to any directory on your computer (e.g., `D:\Tools\DevHUD` or your Desktop).
3. Double-click `DevHUD - Command Scratchpad.exe`.
   - *No installation wizard, no registry tampering, no .NET runtime prerequisites required.*

---

### Option 2: Build From Source

#### Prerequisites
- [Git](https://git-scm.com/)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (Windows x64)
- Visual Studio 2022 (v17.8+) with *.NET desktop development* workload **OR** VS Code with C# Dev Kit.

#### 1. Clone the repository
```bash
git clone https://github.com/your-username/DevHUD.git
cd DevHUD
```

#### 2. Restore dependencies
```bash
cd "DevHUD - Command Scratchpad/DevHUD - Command Scratchpad"
dotnet restore
```

#### 3. Run in development mode
```bash
dotnet run
```

#### 4. Publish a Single-File Self-Contained Executable
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o bin/Publish
```
The output executable with embedded icon and resources will be located at:
`bin/Publish/DevHUD - Command Scratchpad.exe`

---

## 💾 Storage & Data Architecture

DevHUD stores data locally on your machine with **zero telemetry and complete privacy**:

- **Active Storage:**  
  `%LocalAppData%\DevHUD\scratchpad_data.json`  
  *(Resolved to `C:\Users\<Username>\AppData\Local\DevHUD\scratchpad_data.json`)*
  - Stores all workspace tabs, notes, window states, and language preferences.
- **Automated Daily Backups:**  
  `%LocalAppData%\DevHUD\snapshots\snapshot_YYYYMMDD.json`  
  - Automatically captures snapshots of your previous day's completed tasks before daily habit respawns occur.

---

## 🛠️ Tech Stack & Dependencies

- **Language:** C# 12
- **Framework:** .NET 8.0 Windows Desktop (WPF)
- **Editor Engine:** [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) (High-performance WPF text editor with custom syntax colorizers)
- **Architecture:** MVVM via [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- **Styling:** Custom Cyberpunk Dark Acrylic vector styles & drop-shadow neon glow shaders
- **Interoperability:** Win32 API User32 bindings (`RegisterHotKey`, `UnregisterHotKey`)

---

## 📄 License

This project is open-sourced under the [MIT License](LICENSE).
Feel free to use, modify, and distribute it for personal and commercial projects.
