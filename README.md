# Valorant External | Vanguard Bypass | Silent Aim + ESP

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Stars](https://img.shields.io/github/stars/valorant-ext/Valorant-External-Cheat-Aimbot?style=social)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-lightgrey)
![Status](https://img.shields.io/badge/status-undetected-green)
![Vanguard](https://img.shields.io/badge/Vanguard-bypassed-red)

> **Advanced external cheat for Valorant featuring kernel-level Vanguard bypass, silent aim, ESP with agent detection, triggerbot, and no-recoil. Built on .NET 9 with driver communication for secure memory access.**

---

## Screenshots

| ESP + Agent Detection | Menu Overlay | Silent Aim |
|:---------------------:|:------------:|:----------:|
| ![ESP](docs/screenshots/esp_agents.png) | ![Menu](docs/screenshots/menu_overlay.png) | ![Aim](docs/screenshots/silent_aim.png) |

---

## Feature Matrix

| Feature | Status | Method | Description |
|---------|--------|--------|-------------|
| ✅ Aimbot | Working | Mouse movement | Smooth aim with configurable FOV and smoothing |
| ✅ Silent Aim | Working | View angle write | Server-side aim correction, near-instant |
| ✅ Triggerbot | Working | Mouse simulation | Auto-fire on crosshair target with random delay |
| ✅ No Recoil | Working | Angle compensation | Recoil compensation with adjustable strength |
| ✅ Player ESP | Working | World-to-screen | 2D boxes with health bars |
| ✅ Agent Detection | Working | Agent ID lookup | Shows agent name above ESP box |
| ✅ Distance ESP | Working | 3D distance calc | Distance display in meters |
| ✅ Vanguard Bypass | Working | Kernel driver | Custom driver communication channel |
| ✅ Handle Hijack | Working | NtQuerySystemInfo | Fallback when driver unavailable |
| ✅ Driver Mapper | Working | Vulnerable driver | Manual map custom driver into kernel |
| ✅ ImGui Menu | Working | Custom renderer | In-game configuration overlay |

---

## Supported Agents (Auto-Detection)

| Duelists | Controllers | Initiators | Sentinels |
|----------|-------------|------------|-----------|
| Jett | Omen | Breach | Sage |
| Raze | Brimstone | Sova | Cypher |
| Phoenix | Viper | Skye | Killjoy |
| Reyna | Astra | Fade | Chamber |
| Yoru | Harbor | Gekko | Deadlock |
| Neon | Clove | KAY/O | Vyse |
| Iso | | Tejo | |
| | | Waylay | |

---

## Build Instructions

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Windows 10/11 x64
- Administrator privileges
- (Optional) Custom kernel driver for Vanguard bypass

### Build

```bash
git clone https://github.com/valorant-ext/Valorant-External-Cheat-Aimbot.git
cd Valorant-External-Cheat-Aimbot
dotnet build -c Release
```

### Run

```bash
dotnet run --project src/ValorantCheat/ValorantCheat.csproj -c Release
```

> **Important:** Must be run as Administrator. The driver must be loaded before launching Valorant.

---

## Usage

1. Load the kernel driver (if using Vanguard bypass)
2. Launch Valorant
3. Run the cheat as Administrator
4. Wait for `[+] All modules initialized` message
5. Press `DELETE` to toggle the in-game menu
6. Configure features via the overlay

---

## Project Structure

```
Valorant-External-Cheat-Aimbot/
├── Valorant-External-Cheat-Aimbot.slnx
├── README.md
├── .gitignore
├── docs/
│   └── CHANGELOG.md
└── src/
    └── ValorantCheat/
        ├── ValorantCheat.csproj
        ├── Program.cs
        ├── Core/
        │   ├── ValorantProcess.cs
        │   ├── DriverComm.cs
        │   ├── KernelReader.cs
        │   └── OffsetManager.cs
        ├── Features/
        │   ├── Aimbot.cs
        │   ├── SilentAim.cs
        │   ├── EspOverlay.cs
        │   ├── TriggerBot.cs
        │   ├── NoRecoil.cs
        │   └── AgentDetector.cs
        ├── SDK/
        │   ├── UWorld.cs
        │   ├── ActorArray.cs
        │   ├── FVector.cs
        │   ├── AController.cs
        │   └── USkeletalMesh.cs
        ├── Bypass/
        │   ├── VanguardBypass.cs
        │   ├── DriverMapper.cs
        │   └── HandleHijack.cs
        ├── Overlay/
        │   ├── RenderEngine.cs
        │   └── ImGuiWrapper.cs
        ├── Config/
        │   └── CheatConfig.cs
        └── Utils/
            └── Vector3Math.cs
```

---

## Disclaimer

This software is provided for **educational and research purposes only**. It is intended for use in controlled lab environments, CTF competitions, and authorized security research. The authors are not responsible for any misuse of this software. Use at your own risk and in compliance with all applicable laws and terms of service.

---

## License

This project is licensed under the MIT License.
