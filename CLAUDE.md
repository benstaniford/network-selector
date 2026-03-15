# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Network Switcher** — a Windows system tray application built with .NET 10 / WPF using the [H.NotifyIcon.Wpf](https://github.com/HardcodetNet/H.NotifyIcon) library. The app runs as a tray icon, monitors the active WiFi connection, and automatically reconnects to `vodafoneC72225` if the connection changes. It includes a WiX v3 MSI installer and a GitHub Actions release pipeline.

## Build & Test Commands

```bash
# Build
dotnet build SampleTrayApp.csproj -c Release

# Run tests (xUnit)
dotnet test SampleTrayApp.Tests/SampleTrayApp.Tests.csproj

# Publish
dotnet publish SampleTrayApp.csproj -c Release -o bin/Release/net10.0-windows/publish

# Create a release (increments patch version, tags, and pushes)
./scripts/make-release
```

## Architecture

- **NetworkSwitcher** (`SampleTrayApp.csproj`) - .NET 10 WPF WinExe. `App.xaml` sets `ShutdownMode="OnExplicitShutdown"` so the app stays alive in the tray. `MainWindow.xaml` is a zero-size invisible window that hosts a `TaskbarIcon` from H.NotifyIcon. `NetworkMonitor.cs` handles WiFi detection and reconnection via `netsh`.
- **SampleTrayApp.Tests** (`SampleTrayApp.Tests/`) - xUnit test project (currently placeholder).
- **SampleTrayApp.Installer** (`SampleTrayApp.Installer/`) - WiX v3 MSI installer. Installs to Program Files, creates Start Menu/Desktop shortcuts, and registers auto-start via `HKLM\...\Run`.

## Release Process

Releases are triggered by pushing a `v*.*.*` tag (or manual workflow_dispatch). The GitHub Actions workflow (`.github/workflows/release.yml`) builds the app, runs tests, builds the WiX MSI, and creates a GitHub Release with the MSI attached.
