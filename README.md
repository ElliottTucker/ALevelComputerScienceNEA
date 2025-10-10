# CompSciNea

A small 3D vertical platformer built with Unity. The level is procedurally generated as a sequence of platforms that ascend upwards. Every 20th platform is a bouncy booster and the final platform is a win platform. The game includes a basic save/load system, a pause menu, and UI for height progress and win state.

This README documents the tech stack, requirements, setup, how to run, controls, key scripts, project structure, tests, and licensing notes.

## Tech stack
- Engine: Unity (Editor 6000.1.9f1 per environment info)
- Language: C# (.NET Framework equivalent; Player API: net471)
- Rendering: Universal Render Pipeline (URP)
- Input: Unity Input System
- UI: TextMeshPro (TMP), UGUI
- Package manager: Unity Package Manager (Packages/manifest.json)

Key packages (from Packages/manifest.json):
- com.unity.render-pipelines.universal 17.1.0 (URP)
- com.unity.inputsystem 1.14.0
- com.unity.cinemachine 3.1.4
- com.unity.ugui 2.0.0
- com.unity.textmeshpro (via built-in package: Unity.TextMeshPro)
- com.unity.ai.navigation 2.0.8
- com.unity.visualscripting 1.9.7
- com.unity.test-framework 1.5.1

## Overview
- Procedural generation of a vertical “path” of 100 platforms (normal, bouncy every 20th, and a gold win platform at the end).
- Third-person movement with camera orbit and jump.
- Progress UI shows your vertical progress percentage through the level.
- Pause menu with save-and-exit.
- Save system persists player/camera state, velocity, seed, and height percent to Application.persistentDataPath.

## Requirements
- Unity Editor 6000.1.9f1 recommended.
  - The repository includes a Packages/manifest.json compatible with the 6000.1.x line.
  - If you open with a different Unity version, Unity may prompt to upgrade/downgrade; prefer the exact version to avoid package/API incompatibilities.
- Platform targets: Desktop (Windows/macOS) by default.
- No external system dependencies beyond Unity packages.

## Getting started
1. Install Unity Hub and add Unity 6000.1.9f1.
2. Clone this repository.
3. In Unity Hub, click Add and select the project folder.
4. Open the project. Unity will resolve packages automatically.

### Running in the Editor
- Open the main menu scene and press Play.
  - Scene indices used by code: 0 = Main Menu, 1 = Game Scene.
  - TODO: Confirm scene asset names and their locations under Assets/ and ensure they are added to File > Build Settings in the listed order.
- Controls:
  - WASD/Arrow keys: Move
  - Space: Jump (with a short cooldown)
  - Mouse: Orbit camera
  - Esc: Toggle Pause (Resume, Save and Exit)

### Building
- Use File > Build Settings to configure target platform, add scenes in build (Main Menu at index 0, Game at index 1), and click Build.
- Optional CLI (adjust paths as needed):
  - macOS example:
    - "/Applications/Unity/Hub/Editor/6000.1.9f1/Unity.app/Contents/MacOS/Unity" -batchmode -quit -projectPath "/path/to/comp sci NEA" -executeMethod EditorBuildPipeline.BuildPlayer
  - TODO: Provide a concrete build script or editor method for CI if required.

## Save data
- Save file path: Application.persistentDataPath + "/playerData.dat"
- Format: BinaryFormatter (PlayerData class)
- Saved state includes: seed, player position, camera position and rotation, camera angles, player velocity, and computed height percent relative to final platform.
- Save triggers:
  - Pause menu: "Save and exit".
  - Main menu: "New Game" clears save.

## Scenes and entry points
- MainMenuUI loads scene index 1 for starting or loading a game; scene index 0 is assumed to be the Main Menu.
- PlatformGeneration Start() uses SaveData.instance to seed Random, generate the level, and restore player/camera state.
- TODO: Document exact scene names and confirm they are in Build Settings at the expected indices (0: Main Menu, 1: Game).

## Key scripts
- Assets/Scripts/PlatformGeneration.cs
  - Generates a path of platforms based on a seed (from SaveData). Every 20th platform is bouncy; the final platform is gold and triggers win.
  - Exposes tunable parameters for sizes, distances, angles, counts; tracks min/max height for UI.
- Assets/Scripts/Platform.cs, PlatformNode.cs, PlatformType.cs
  - Platform behavior and metadata for the generation system.
- Assets/Scripts/PlayerMovement.cs
  - Rigidbody-driven movement, camera-relative input, jump cooldown, gravity and drag, bouncy platform reaction, and win detection.
- Assets/Scripts/CameraScript.cs
  - Third-person orbit camera with sensitivity and clamped pitch; persists angles via SaveData.
- Assets/Scripts/SaveSystem.cs
  - Binary save/load/delete of PlayerData at persistentDataPath.
- Assets/Scripts/SaveData.cs
  - Singleton (DontDestroyOnLoad) facade over PlayerData; supplies seed and positions on load, computes height percent, and saves current game state.
- Assets/Scripts/UI Scripts/MainMenuUI.cs
  - Displays load/new game options with progress percent if available; loads scene index 1; "New Game" clears save then loads game.
- Assets/Scripts/UI Scripts/PauseUI.cs
  - Esc toggles pause; shows Resume and Save & Exit; saving returns to menu (scene 0).
- Assets/Scripts/UI Scripts/HeightUI.cs
  - Shows current height percentage based on PlatformGeneration min/max and player position.
- Assets/Scripts/UI Scripts/WinUI.cs
  - Shows win UI when final platform reached; allows returning to menu and clears save.

## Environment variables
- None required for local development/build.
- TODO: If CI/CD is added, document any required environment variables here.

## Tests
- Unity Test Framework is enabled via com.unity.test-framework.
- Currently no custom tests are present under Assets/ (package tests exist but are not part of this project’s validation).
- To add tests:
  - Create Assets/Tests/EditMode and/or Assets/Tests/PlayMode folders and add test assemblies.
  - Run via Window > General > Test Runner (or Window > Analysis > Test Runner in newer versions).
  - TODO: Add minimal playmode tests for player movement and platform generation.

## Project structure
- Assets/
  - Scripts/
    - CameraScript.cs
    - PlatformGeneration.cs
    - Platform.cs
    - PlatformNode.cs
    - PlatformType.cs
    - PlayerMovement.cs
    - PlayerHealth.cs
    - SaveSystem.cs
    - SaveData.cs
    - PlayerData.cs
    - UI Scripts/
      - MainMenuUI.cs
      - PauseUI.cs
      - HeightUI.cs
      - PlayerHealthUI.cs
      - WinUI.cs
  - [Other assets: materials, prefabs, scenes, third-party assets]
- Packages/
  - manifest.json (Unity packages)
- ProjectSettings/
  - Unity project settings

    
