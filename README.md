# AppFlush

A lightweight uninstaller for Windows. It shows every installed app in one place, uninstalls it through that app's own official uninstaller, and helps clean up leftover folders that regular uninstalls often leave behind.

It's a simpler alternative to Windows' built-in *Apps & Features*: no console windows, no hidden steps, no fake uninstall process. AppFlush always calls the official uninstaller that the app itself registered with Windows.

**Platform:** Windows 10/11 (x64) &nbsp;·&nbsp; **Built with:** .NET 8 + WinForms &nbsp;·&nbsp; **Install:** single-file `.exe` installer

## Features

- Lists every installed app (with version and publisher), with a search box to filter quickly.
- One-click uninstall that runs the app's own official uninstaller, so the result is identical to uninstalling manually from Control Panel.
- Scans common install locations (`Program Files`, `Program Files (x86)`, `ProgramData`, `AppData\Roaming`, `AppData\Local`) for leftover folders, and lets you delete them after confirming.
- Right-click **"Uninstall with AppFlush"** on `.lnk` shortcuts, so you can uninstall directly from the Desktop or Start Menu.
- Self-contained single file — no separate .NET Runtime install needed on the target machine.

## How it works

AppFlush reads the list of installed apps from the same Windows Registry `Uninstall` key that *Apps & Features* uses, then runs each app's own `UninstallString` as-is. It doesn't rewrite or guess at any app's uninstall process, so any confirmation dialog that shows up afterward belongs to that app, not to AppFlush.

The leftover-folder scan works by matching folder names in common install locations against the app's name/publisher (a heuristic match, similar to the default mode in Revo Uninstaller). It isn't a deep scan based on install-time registry traces, so it doesn't guarantee every leftover file or registry entry is found in every case.

The right-click menu is registered as a classic shell verb in the Windows Registry (not an MSIX context-menu extension). Because of that, on Windows 11 it shows up under **"Show more options"** instead of the top-level menu — that's how Windows 11 treats all classic third-party shell verbs, not something specific to AppFlush.

## Building from source (contributors)

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) and optionally Visual Studio 2022 (".NET desktop development" workload).

```powershell
dotnet restore AppFlush.sln
dotnet build AppFlush.sln -c Release
dotnet test tests/AppFlush.Core.Tests/AppFlush.Core.Tests.csproj -c Release
dotnet publish src/AppFlush.App/AppFlush.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

The `.exe` installer is built from `installer/installer.iss` with [Inno Setup], or automatically via GitHub Actions (`.github/workflows/build-windows.yml`).

## License

MIT — see [LICENSE](LICENSE).
