# Taurus

Full buildable source of `Taurus Menu`, a mod menu **Gorilla Tag**

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
- `MenuNetworking.cs`, `MenuPacket.cs`, `WorldEffectsNetworking.cs` — custom networked packet handling
- `DiscordPresence.cs` / `DiscordPresenceText.cs` — Discord status integration
- `TelemetryFormat.cs`, `TelemetryWebhook.cs` — telemetry/webhook reporting

## Build

Open `Taurus/Taurus.sln` in Visual Studio. All references point to managed DLLs under the local Gorilla Tag install OR Use the already compiled verison in releases




Thank you GPT for the readme
