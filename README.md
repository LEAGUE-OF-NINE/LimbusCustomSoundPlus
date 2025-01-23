# LimbusCustomSound

A simple mod that lets you replace Limbus Company's sound effects and voices with your own custom audio files.

## Installation

1. Install [.NET SDK 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.413-windows-x64-installer)
2. Install [BepInEx](https://builds.bepinex.dev/projects/bepinex_be/692/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.692%2B851521c.zip)
3. Download the [latest version](https://github.com/kimght/LimbusCustomSound/releases/latest) of the mod
4. Extract and place the `LimbusCustomSound` folder into your `BepInEx/plugins` directory

## How to Use

1. Create a `Sound` folder inside your `LimbusCustomSound` folder
2. Add your custom .wav files following this structure:
   ```
   Sound/
   ├── Voice/
   │   └── Default/
   │       └── D_Wick.wav
   └── BGM/
       └── LimbusCompany_BGM_Lobby.wav
   ...
   ```
3. The mod will print all playing sound events in the console, which you can use as a reference

### Example
To replace `D_Wick` sound:
- Original event path: `event:/Voice/Default/D_Wick`
- Place your .wav file at: `Sound/Voice/Default/D_Wick.wav`

## Features

- [x] Custom sound effects
- [x] Custom voice lines
- [ ] Correct volume control

## Need Help?
If you have any issues, please create a ticket in the [Issues](https://github.com/kimght/LimbusCustomSound/issues) section.
