# SurfaceKeysetAutoLayout

A Windows background service developed as part of a Microsoft Surface capstone project. It automatically switches the keyboard layout when a custom USB keyboard skin is connected or removed. 
## What It Does

Physical keyboard skins for laptops support alternate key layouts and languages. This app detects when a skin's USB dongle is plugged in or removed, and switches the Windows keyboard layout accordingly.

## Requirements

- Windows 10/11 (x64)
- .NET 8 SDK

## Build & Run

```bash
dotnet build
dotnet run
```

Press **Enter** to exit.

## Project Structure

| File | Description |
|---|---|
| `Program.cs` | Entry point and connection state machine |
| `DeviceWatcher.cs` | USB device event listener with debounce |
| `DeviceScanner.cs` | PnP device enumeration |
| `LayoutApplier.cs` | Keyboard layout switching via Win32 APIs |

## Note
This repository was created as a code sample for the MLH Fellowship application.
