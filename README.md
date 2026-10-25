# Taurus

Decompiled source of `Taurus.dll`, a mod menu plugin for the VR game **Gorilla Tag** (Meta Horizon / Oculus build). Source was exported from the compiled assembly via dnSpy/ILSpy (see header comment in `Taurus/Taurus.csproj`) — this is a reverse-engineered snapshot, not original authored source.

## Contents

- `Taurus/` — C# source, `.NET Framework 4.7.2`, `Taurus.csproj` / `Taurus.sln`
- `Taurus.dll` — original compiled plugin binary
- `Taurus/DiscordRPC/` — Discord Rich Presence client
- `Taurus/NativeUi`, `Taurus/Rexon` — custom UI/menu rendering framework
- `Taurus/ItzGameMenu/` — bundled Unity asset bundle + helper DLL
- `*.bundle` files — Unity asset bundles (fonts, themes, menu assets) loaded at runtime

## Notable modules

- `Plugin.cs` — plugin entry point
- `*Menu.cs` (`KyraMenu`, `FizzoMenu`, `ParrotMenu`, `TelAvivMenu`, `DesktopMenu`) — menu UI variants
- `GunLib.cs`, `LobbyActions.cs`, `BoardSurface.cs`, `CustomBoards.cs` — gameplay feature mods
- `MenuNetworking.cs`, `MenuPacket.cs`, `WorldEffectsNetworking.cs` — custom networked packet handling
- `DiscordPresence.cs` / `DiscordPresenceText.cs` — Discord status integration
- `RigPatchKit.cs`, `RemoteFeaturePolicy.cs`, `SignedFeaturePolicy.cs` — patching and feature-gating
- `TelemetryFormat.cs`, `TelemetryWebhook.cs` — telemetry/webhook reporting

## Build

Open `Taurus/Taurus.sln` in Visual Studio. All references point to managed DLLs under the local Gorilla Tag install:

```
C:/Program Files/Meta Horizon/Software/Software/another-axiom-gorilla-tag/Gorilla Tag_Data/Managed/
```

A working Gorilla Tag install at that path is required to resolve references and build.

## Status

Analysis/archive drop. No build verification or cleanup has been done — code reflects the decompiler's output as-is.




Thank you GPT for the readme
