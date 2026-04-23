# ProjectHub — Developer Project & Workflow Manager

> One place to manage all your projects, tools, and workflows. Launch with a single click.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?logo=windows)](https://github.com/dotnet/wpf)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue)](https://github.com/dotnet/wpf)
[![License](https://img.shields.io/badge/License-MIT-green.svg)]()

**English | [简体中文](README/README.zh-CN.md)**

---

## What Problem Does It Solve?

- Your desktop is cluttered with project folders and hard to navigate
- Every day you manually open IDE, start debug server, open docs... too many steps
- VS Code, Android Studio, and CLion projects are mixed without a unified entry point
- Frequently used tools (Postman, DB clients) are scattered everywhere
- Multiple related projects need to be started one by one, wasting time

**ProjectHub brings them all together** — elegant like JetBrains Toolbox, clean like VS Code.

---

## Core Features

### Multiple Launch Types, One-Click Start

Not just code projects — anything you want to open quickly:

| Type | Description | Examples |
|------|-------------|----------|
| **Open File** | Open any file with default or custom program | `.sln`, `.csproj`, `.md`, documents |
| **Run Program** | Launch executable directly | `Postman.exe`, custom tools |
| **Open Web URL** | Open dev environment or docs site | Supports multiple URLs at once |
| **CMD Command** | Execute preset command-line scripts | `dotnet run`, `npm start` |

- **Smart program association detection**: Auto-detects available programs for a file, pick from dropdown
- **Custom launch arguments**: Configure专属 parameters per project
- **Run as administrator**: One-click elevation when needed

### WorkSpace — Batch Project Launch

Organize multiple related projects into a "WorkSpace" and **launch them all with one click**:

- Frontend + Backend + DB client, all at once
- Adjustable launch order per project
- Each project can be independently enabled/disabled

### WorkFolder — Tree-Structured Organization

- Unlimited nested folders, organize by business/client/tech stack
- Both projects and workspaces can be placed into folders
- Sidebar tree navigation, clear structure at a glance

### Find What You Need Quickly

- **Global search**: Search by name, path, or description across projects and workspaces
- **Recent items**: Auto-recorded open history, recent projects at your fingertips
- **Favorites**: Star frequently used projects for quick access

### Personalized Experience

- **Dark / Light theme**: Follow system or manual toggle, easy on the eyes
- **Bilingual UI**: Switch between English and Chinese with one click
- **Custom icons**: Set a unique icon for each project for quick recognition

### More Thoughtful Details

- Double-click card to launch project
- Context menu: Launch / Favorite / Edit / Open folder / Delete
- Auto-tracked launch count and last opened time
- Card hover animations with instant feedback
- Project description support for notes and progress

---

## UI Preview

![UI Preview](README/screenshot.png)

> Actual screenshots coming soon. Feedback welcome after trying it out!

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| UI Framework | WPF + .NET 10 |
| Reactive Programming | ReactiveUI |
| Data Persistence | SQLite + Entity Framework Core |
| Architecture | Clean Architecture / DDD |
| Dependency Injection | Microsoft.Extensions.DependencyInjection |
| Object Mapping | AutoMapper |

---

## Quick Start

### Requirements

- Windows 10 19041 or later
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Runtime for running, SDK for building)

### Run

```bash
# Clone repository
git clone <repository-url>
cd ProjectHub

# Build and run
dotnet run --project ProjectHub.Presentation.Wpf
```

### Publish as Single File

```bash
dotnet publish ProjectHub.Presentation.Wpf \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true
```

---

## Use Cases

### Case 1: Full-Stack Developer's Daily Routine

Alice maintains a microservices system. Every morning she needs to open:
1. API Gateway (VS project)
2. User Service (VS project)
3. Frontend Admin (VS Code folder)
4. Postman (testing tool)
5. Database management tool

**With ProjectHub**: Create a "Microservice Dev" workspace with all 5 projects in dependency order. Click "Launch All" every morning — everything opens automatically.

### Case 2: Multi-Client Project Management

Bob is a freelance developer serving 3 clients. He organizes projects into folders by client:

- Client A
  - Official Website
  - Mini Program
- Client B
  - ERP System
- Personal
  - Open Source Tool

**With ProjectHub**: Sidebar tree structure clearly shows everything. Type client name or tech keyword in search box — instant location.

### Case 3: Tool Hub

Charlie's desktop and Start Menu are filled with tools: screenshot, screen recorder, format converter, design viewer...

**With ProjectHub**: Add all as "Run Program" projects. One interface manages all tools — no more desktop icon chaos.

---

## Roadmap

- [x] Project / Workspace / Folder management
- [x] Four launch types (File / Program / Web / CMD)
- [x] Workspace batch launch
- [x] Global search
- [x] Dark / Light theme
- [x] English / Chinese UI
- [ ] Tag system
- [ ] Junk file cleanup
- [ ] Global hotkey / Tray icon
- [ ] Config import / export
- [ ] Project template marketplace

---

## Contributing

Issues and PRs welcome!

- Found a bug? Please submit an Issue
- Have a feature idea? Open a Discussion
- Want to contribute code? Read the [Development Guide](TYLMFiles/DEVELOPMENT_GUIDE.md)

---

## License

MIT License

---

> **ProjectHub** — Every start, just one click away.
